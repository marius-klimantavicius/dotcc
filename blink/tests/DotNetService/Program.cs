using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

if (args.Length != 1 || !int.TryParse(args[0], NumberStyles.None,
        CultureInfo.InvariantCulture, out int port) || port is < 0 or > 65535)
{
    Console.Error.WriteLine("usage: DotNetService PORT");
    return 2;
}

var listener = new TcpListener(IPAddress.Loopback, port);
try
{
    listener.Start(8);
    Console.WriteLine($"READY {((IPEndPoint)listener.LocalEndpoint).Port}");
    bool stopping = false;
    while (!stopping)
    {
        using Socket client = listener.AcceptSocket();
        client.ReceiveTimeout = client.SendTimeout = 5000;
        byte[] request = new byte[4096];
        int count = 0;
        while (true)
        {
            if (count == request.Length)
                throw new IOException("request headers exceed fixture limit");
            int received = client.Receive(request.AsSpan(count));
            if (received == 0)
                throw new IOException("request ended before headers");
            count += received;
            if (request.AsSpan(0, count).IndexOf("\r\n\r\n"u8) >= 0)
                break;
        }
        int end = request.AsSpan(0, count).IndexOf("\r\n"u8);
        string firstLine = Encoding.ASCII.GetString(request, 0, end);
        string body;
        string status;
        if (firstLine == "GET /health HTTP/1.1")
        {
            status = "200 OK";
            body = "ok\n";
        }
        else if (firstLine == "POST /stop HTTP/1.1")
        {
            status = "200 OK";
            body = "stopped\n";
            stopping = true;
        }
        else
        {
            status = "404 Not Found";
            body = "not found\n";
        }
        byte[] response = Encoding.ASCII.GetBytes($"HTTP/1.1 {status}\r\nContent-Type: text/plain\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n{body}");
        int sent = 0;
        while (sent < response.Length)
        {
            int written = client.Send(response.AsSpan(sent));
            if (written == 0) throw new IOException("response made no progress");
            sent += written;
        }
        client.Shutdown(SocketShutdown.Send);
    }
    Console.WriteLine("STOPPED");
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine($"{error.GetType().Name}: {error.Message}");
    return 1;
}
finally
{
    listener.Stop();
}
