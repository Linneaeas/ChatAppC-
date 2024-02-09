using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint serverEndPoint = new IPEndPoint(ipAddress, 25500);

            Socket clientSocket = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
            );

            clientSocket.Connect(serverEndPoint);
            Console.WriteLine("Connected to the server.");

            Menu.MainMenu(clientSocket);

            Console.ReadLine();
        }
    }
}
