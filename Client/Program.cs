using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint serverEndPoint = new IPEndPoint(ipAddress, 25500);

            Socket clientSocket = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
            );
            {
                clientSocket.Connect(serverEndPoint);
                Console.WriteLine("Connected to the server.");

                // Sending a message to the server
                string message = "Hello, server!";
                byte[] data = Encoding.UTF8.GetBytes(message);
                clientSocket.Send(data);

                // Receive a response from the server
                byte[] buffer = new byte[5000];
                int bytesRead = clientSocket.Receive(buffer);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Server response: " + response);
            }
            Console.ReadLine();
        }
    }

}
