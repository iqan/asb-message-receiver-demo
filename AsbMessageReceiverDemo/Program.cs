namespace AsbMessageReceiverDemo
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            using (var singleMessageReceiver = new SingleMessageReceiver<ItemModel>())
            {
                var item = singleMessageReceiver.ReceiveMessage();
                Console.WriteLine($"Item - Id: {item.Id}, Name: {item.Name}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
