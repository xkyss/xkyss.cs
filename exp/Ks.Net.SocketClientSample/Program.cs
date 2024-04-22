using System.Net.Sockets;


Console.WriteLine("Hello, SocketClient!");

TcpClient socket = new (AddressFamily.InterNetwork);
await socket.ConnectAsync("localhost", 5123);
await socket.GetStream().WriteAsync("Hello!"u8.ToArray());

await Task.Delay(1000);

Console.WriteLine("Goodbye, SocketClient!");
