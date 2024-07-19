using System.Net.Sockets;
using System.Text;


Console.WriteLine("Hello, SocketClient!");

TcpClient socket = new (AddressFamily.InterNetwork);
await socket.ConnectAsync("localhost", 5123);

await Task.Delay(1000);

Console.WriteLine("输入[bye]以结束.");
while (true)
{
    var line = Console.ReadLine();
    if (string.IsNullOrEmpty(line))
    {
        continue;
    }
    if (line.ToLower() == "bye")
    {
        break;
    }
    await socket.GetStream().WriteAsync(Encoding.UTF8.GetBytes(line));
    await socket.GetStream().WriteAsync(Encoding.UTF8.GetBytes("\r\n"));
}

Console.WriteLine("Goodbye, SocketClient!");
