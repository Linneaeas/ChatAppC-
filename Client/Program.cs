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
            // Server's IP address (localhost in this case)
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            // Endpoint for the server (IP address + port)
            IPEndPoint serverEndPoint = new IPEndPoint(ipAddress, 25500);

            // Create a socket for the client
            Socket clientSocket = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
            );
            // Display the main menu
            ShowMainMenu();

            {
                // Connect to the server using the specified endpoint
                clientSocket.Connect(serverEndPoint);
                Console.WriteLine("Connected to the server.");

                // Sending a message to the server
                string message = "Hello, server!";
                byte[] data = Encoding.UTF8.GetBytes(message);
                clientSocket.Send(data);

                // Receive a response from the server
                byte[] buffer = new byte[5000];
                int bytesRead = clientSocket.Receive(buffer);
                // Convert the received bytes to a string response
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // Display the response from the server
                Console.WriteLine("Server response: " + response);

                // Wait for user input before closing the console
                Console.ReadLine();
            }


            static void ShowMainMenu()
            {
                Console.WriteLine("Main Menu:");
                Console.WriteLine("1. Option 1");
                Console.WriteLine("2. Option 2");
                Console.WriteLine("3. Option 3");
                // Add more options as needed

                Console.Write("Select an option: ");
                string? userInput = Console.ReadLine();

                // Process the user input and perform actions based on the selected option
                switch (userInput)
                {
                    case "1":
                        Console.WriteLine("Selected Option 1");
                        break;
                    case "2":
                        Console.WriteLine("Selected Option 2");
                        break;
                    case "3":
                        Console.WriteLine("Selected Option 3");
                        break;
                    // Add more cases as needed
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }
    }
}
