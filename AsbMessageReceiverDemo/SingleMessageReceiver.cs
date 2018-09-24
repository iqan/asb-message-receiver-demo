namespace AsbMessageReceiverDemo
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Newtonsoft.Json;

    public class SingleMessageReceiver<T> : IDisposable where T : class
    {
        private SubscriptionClient subscriptionClient;
        private bool messageReceived = false;
        private T received;
        private readonly string subscriptionName = "your-subscription-name";
        private readonly string topicName = "your-topic-name";
        private readonly string connectionString = "your-service-bus-connection-string";
        private readonly int maxMinutes;
        private event EventHandler<bool> MessageReceivedEvent;

        public SingleMessageReceiver(int maxMinutes = 1)
        {
            this.maxMinutes = maxMinutes;
            Console.WriteLine("Creating subscription client.");
            this.subscriptionClient = new SubscriptionClient(this.connectionString, this.topicName, this.subscriptionName);

            Console.WriteLine("Listening to message received event.");
            this.MessageReceivedEvent += Receiver_messageReceivedEvent;
        }

        public T ReceiveMessage()
        {
            Console.WriteLine("Registering message handler.");
            this.subscriptionClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ErrorHandler) { AutoComplete = false, MaxConcurrentCalls = 1 });
            this.WaitTillMessageIsReceived();
            return received;
        }

        private async Task MessageHandler(Message message, CancellationToken ct)
        {
            if (!this.messageReceived)
            {
                Console.WriteLine("Message received");
                var content = Encoding.UTF8.GetString(message.Body);
                this.received = JsonConvert.DeserializeObject<T>(content);
                this.MessageReceivedEvent?.Invoke(this, true);
                await this.subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
            }
        }

        private Task ErrorHandler(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine("An error occurred.");
            Console.WriteLine("Exception:" + arg.Exception.Message);
            return Task.CompletedTask;
        }

        private void WaitTillMessageIsReceived()
        {
            var sw = new Stopwatch();
            sw.Start();
            while (!messageReceived)
            {
                if (sw.ElapsedMilliseconds > this.maxMinutes * 60 * 1000)
                {
                    throw new Exception("Max wait time over. No message received.");
                }
                Console.WriteLine("Message not received. waiting...");
                Thread.Sleep(2000);
            }
            sw.Stop();
        }

        private void Receiver_messageReceivedEvent(object sender, bool e)
        {
            Console.WriteLine("Message received event occurred.");
            this.messageReceived = true;
        }

        public void Dispose()
        {
            if (!this.subscriptionClient.IsClosedOrClosing)
            {
                Console.WriteLine("Closing client.");
                this.subscriptionClient.CloseAsync().GetAwaiter().GetResult();
            }
        }
    }
}
