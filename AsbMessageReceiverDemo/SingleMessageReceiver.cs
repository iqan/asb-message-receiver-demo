namespace AsbMessageReceiverDemo
{
    using System;
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
        private readonly string subscriptionName = "asos.legacy.retail.pricing.endpoint";
        private readonly string topicName = "pricechanges.iqan";
        private readonly string connectionString = "Endpoint=sb://asos29-test-eun-legacyretail-bus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=V98cZQMfkhwlqwN9KtqbhMFEAqDwEqMtxK3ZYfi8YBs=";

        private event EventHandler<bool> messageReceivedEvent;

        public SingleMessageReceiver()
        {
            Console.WriteLine("Creating subscription client.");
            this.subscriptionClient = new SubscriptionClient(connectionString, topicName, subscriptionName);

            Console.WriteLine("Listening to message received event.");
            this.messageReceivedEvent += Receiver_messageReceivedEvent;
        }

        public T ReceiveMessage()
        {
            Console.WriteLine("Registering message handler.");
            this.subscriptionClient.RegisterMessageHandler(MessageHandler, ErrorHandler);
            WaitTillMessageIsReceived();
            return received;
        }

        private Task MessageHandler(Message message, CancellationToken ct)
        {
            Console.WriteLine("Message received");
            var content = Encoding.UTF8.GetString(message.Body);
            this.received = JsonConvert.DeserializeObject<T>(content);
            this.messageReceivedEvent?.Invoke(this, true);
            return Task.CompletedTask;
        }

        private Task ErrorHandler(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine("An error occurred.");
            Console.WriteLine("Exception:" + arg.Exception.Message);
            return Task.CompletedTask;
        }

        private void WaitTillMessageIsReceived()
        {
            while (!messageReceived)
            {
                Console.WriteLine("Message not received. waiting...");
                Thread.Sleep(2000);
            }
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
