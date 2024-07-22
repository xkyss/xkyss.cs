using System.Text;

namespace Ks.Net.Socket;

public class Constants
{
    public static readonly string DefaultSocketServerKey = "SocketServer";
    public static readonly string DefaultSocketClientKey = "SocketClient";
        
    public static readonly string DefaultSocketPortKey = "Port";
    public static readonly int DefaultSocketPort = 8765;
        
    public static readonly string DefaultSocketHostKey = "Host";
    public static readonly string DefaultSocketHost = "localhost";
    
    public static readonly byte[] CRLF = Encoding.ASCII.GetBytes("\r\n");
}