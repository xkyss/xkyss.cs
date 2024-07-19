using System.Net.Sockets;
using System.Text;


Console.WriteLine("Hello, SocketClient!");

TcpClient socket = new (AddressFamily.InterNetwork);
await socket.ConnectAsync("localhost", 5123);

await Task.Delay(1000);

Console.WriteLine("输入[bye]以结束.");

ReceiveMessagesAsync(socket);
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

socket.Close();
Console.WriteLine("Goodbye, SocketClient!");

static async Task ReceiveMessagesAsync(TcpClient socket)
{
    var buffer = new byte[1024];
    var stream = socket.GetStream();

    while (true)
    {
        // 从服务器读取数据
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

        // 如果没有读取到数据，表示连接已关闭
        if (bytesRead == 0)
        {
            break;
        }

        // 将读取到的字节转换为字符串并输出
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine("<: " + response);
    }
}