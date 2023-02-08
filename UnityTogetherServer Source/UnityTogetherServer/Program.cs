using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Server
{
    public TcpListener server;
    private List<TcpClient?> clients = new();
    private List<Task> listenTasks = new();
    private TcpClient currentClient;
    
    private int packageIndex = 0;
    
    public static void Main(string[] args)
    {
        Server server = new();
        
        if (args.Length > 0)
        {
            server.StartServer(args[0], Convert.ToInt32(args[1]));
            return;
        }
        server.StartServer("25.48.78.141", 8085);
        return;
        Console.WriteLine("Enter IP address: ");
        string ip = Console.ReadLine() ?? string.Empty;
        Console.WriteLine("Enter port: ");
        int port = Convert.ToInt32(Console.ReadLine());
        server.StartServer(ip, port);
    }
    
    public void StartServer(string IP, int Port)
    {
        Console.WriteLine("Starting server on " + IP + ":" + Port);
        server?.Stop();
        server = new TcpListener(IPAddress.Parse(IP), Port);
        server.Start();
        Console.WriteLine("Server has started.");
        
        packageIndex = 0;
        
        while (true)
        {
            TcpClient? newClient = server.AcceptTcpClient();
            clients.Add(newClient);
            Console.WriteLine("Client connected: " + newClient.Client.RemoteEndPoint);
            Task listenTask = new Task(() => ListenToClient(newClient));
            listenTasks.Add(listenTask);
            listenTask.Start();
        }
    }

    protected void SendPackage(int packageType, string username, string package, TcpClient? packageSender)
    {
        packageIndex++;
        List<TcpClient> brokenClients = new();
        foreach (TcpClient? client in clients)
        {
            if (client == null || client is {Connected: false}) continue;
            Stream stream = client.GetStream();
            List<byte> data = Encoding.ASCII.GetBytes(packageType + "#" + username + "#" + package + ";").ToList();

            try
            {
                stream.Write(data.ToArray(), 0, data.Count);
            }
            catch (Exception e)
            {
                brokenClients.Add(client);
            }
        }
        
        foreach (TcpClient brokenClient in brokenClients)
            DisconnectClient(brokenClient);
    }

    protected void DisconnectClient(TcpClient? client)
    {
        if (client == null) return;
        clients.Remove(client);
        Console.WriteLine("Client disconnected: " + client?.Client?.RemoteEndPoint);
        client?.Close();
    }
    
    protected void ListenToClient(TcpClient? client)
    {
        NetworkStream ns = client.GetStream();
        while (client is {Connected: true})
        {
            while (client is {Connected: true} && !ns.DataAvailable) { }

            if (client is {Connected: false}) break;

            byte[] msgBuffer = new byte[50000];
            Task<int> readTask = ns.ReadAsync(msgBuffer, 0, msgBuffer.Length);
            msgBuffer = msgBuffer.Take(readTask.Result).ToArray();
            string msg = Encoding.ASCII.GetString(msgBuffer);

            string[] messages = msg.Split(';');
            foreach (string message in messages)
            {
                if (message == "") continue;
                string[] seperatedMessage = message.Split('#');
                int index = int.Parse(seperatedMessage[0]);
                string user = seperatedMessage[1];
                string package = seperatedMessage[2];
                SendPackage(index, user, package, client);
            }
            Console.WriteLine("Received package from " + client.Client.RemoteEndPoint + ": " + msg);
        }
    }
}