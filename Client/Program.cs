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
            /*3.A CONNECTING THE CLIENT TO THE SERVER*/
            // Server's IP address (localhost in this case):
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            // Endpoint for the server (IP address + port):
            IPEndPoint serverEndPoint = new IPEndPoint(ipAddress, 25500);

            // Create a socket for the client:
            Socket clientSocket = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
            );

            // Connect to the server using the specified endpoint:
            clientSocket.Connect(serverEndPoint);
            Console.WriteLine("Connected to the server.");

            // Display the main menu and pass the clientSocket to the LoginRegistration class:
            LoginRegistration.MainMenu(clientSocket);

            // Wait for user input before closing the console
            Console.ReadLine();

        }
    }
}
