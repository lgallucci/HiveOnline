using HiveClient;

var _client = new HiveGameClient("localhost", 7777);

Task task = Task.Run(_client.Connect);

var message = string.Empty;

int counter = 0;
while(!_client.IsConnected || counter > 6)
{
    await Task.Delay(1000);
    counter++;
}

while(_client.IsConnected)
{
    Console.WriteLine("Enter message to send to server:");
    message = Console.ReadLine();

    if (!string.IsNullOrWhiteSpace(message))
    {
        await _client.SendMessage(message);
    }
    await Task.Delay(1000);
}

_client.Dispose();
