namespace Project;

using System.Net;
using System.Net.Sockets;

class Program
{
    static void Main(string[] args)
    {
        // List to store connected client sockets
        List<Socket> sockets = new List<Socket>();
        // Server's IP address (localhost in this case)
        IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        // Endpoint for the server (IP address + port)
        IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, 25500);

        // Create a socket for the server
        Socket serverSocket = new Socket(
            ipAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp
        );
        // Bind the server socket to the specified endpoint
        serverSocket.Bind(iPEndPoint);
        // Start listening for incoming connections with a backlog of 5
        serverSocket.Listen(5);

        // Infinite loop to keep the server running
        while (true)
        {
            // Blockar inte koden om det inte finns en inkommande anslutning.
            // Check if there is an incoming connection
            if (serverSocket.Poll(0, SelectMode.SelectRead))
            {
                // Accept the incoming connection and get the client socket
                Socket client = serverSocket.Accept();
                Console.WriteLine("A client has connected!");
                // Add the client socket to the list
                sockets.Add(client);
            }

            // Loop through each connected client socket
            foreach (Socket client in sockets)
            {
                // Blockar inte koden om det inte finns något att läsa.
                // Check if there is data to read from the client
                if (client.Poll(0, SelectMode.SelectRead))
                {
                    // Read the incoming data from the client
                    byte[] incoming = new byte[5000];
                    int read = client.Receive(incoming);
                    // Convert the received bytes to a string message
                    string message = System.Text.Encoding.UTF8.GetString(incoming, 0, read);
                    Console.WriteLine("From a client: " + message);
                }
            }
        }
    }
}