# asb-message-receiver-demo
Message Receiver demo app

### SingleMessageReceiver

This can be used in place where only one message needs to be received. As Microsoft.Azure.Servicebus nuget comes with subscriptionClient that does not provide this feature out of the box.

#### To use SingleMessageReceiver:

```csharp
using (var singleMessageReceiver = new SingleMessageReceiver<ItemModel>())
{
    var item = singleMessageReceiver.ReceiveMessage();
    Console.WriteLine($"Item - Id: {item.Id}, Name: {item.Name}");
}
```
