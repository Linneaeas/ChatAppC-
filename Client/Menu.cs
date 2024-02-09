using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace Client
{
    public class User
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }

    public class Menu
    {
        public static void MainMenu(Socket clientSocket)
        {
            Console.WriteLine("Huvudmeny. Vad vill du gora?:");
            Console.WriteLine("1. Skapa konto");
            Console.WriteLine("2. Logga in");
            Console.WriteLine("----------------------------");

            string? userInput = Console.ReadLine();

            switch (userInput)
            {
                case "1":
                    CreateAccountFunctions.CreateAccount(clientSocket);
                    break;
                case "2":
                    LoginFunctions.LogIn(clientSocket);
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
        public static void ChattisMenu(Socket clientSocket, User user)
        {
            Console.WriteLine("----------------------------");
            Console.WriteLine("Chattis Meny:");
            Console.WriteLine("Visa denna meny igen, skriv: ^meny");
            Console.WriteLine("Visa inloggade, skriv: ^inloggade");
            Console.WriteLine("Logga ut, skriv: ^loggaut");
            Console.WriteLine("Privat meddelande, skriv: ^privat");
            Console.WriteLine("Skicka meddelande till alla skriv direkt i terminalen");
            Console.WriteLine("----------------------------");

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    string? userInput = Console.ReadLine();

                    switch (userInput)
                    {
                        case "^meny":
                            ChattisMenu(clientSocket, user);
                            break;

                        case "^inloggade":
                            SendConnectedClientsListRequest(clientSocket);
                            HandleConnectedClientsResponse(clientSocket);
                            break;

                        case "^loggaut":
                            LoginFunctions.SendLogoutRequest(clientSocket, user);
                            LoginFunctions.HandleLogoutResponse(clientSocket);
                            break;

                        case "^privat":
                            SendConnectedClientsListRequest(clientSocket);
                            HandleConnectedClientsResponse(clientSocket);
                            MessagesFunctions.SendPrivateMessage(clientSocket, user);
                            break;

                        default:

                            string message = userInput;
                            string sendMessageRequest = $"SEND_MESSAGE_ALL|{user.UserName}|{message}";
                            byte[] data = Encoding.UTF8.GetBytes(sendMessageRequest);
                            clientSocket.Send(data);

                            break;
                    }
                }
                if (clientSocket.Available != 0)
                {
                    MessagesFunctions.HandleMessagesResponse(clientSocket);
                }
                Thread.Sleep(200);
            }
        }

        public static void SendConnectedClientsListRequest(Socket clientSocket)
        {
            string getConnectedClientsList = "GET_CONNECTED_CLIENTS";
            byte[] connectedClientsData = Encoding.UTF8.GetBytes(getConnectedClientsList);
            clientSocket.Send(connectedClientsData);
        }

        public static void HandleConnectedClientsResponse(Socket clientSocket)
        {
            byte[] connectedClientsData = new byte[5000];
            int responseLength = clientSocket.Receive(connectedClientsData);
            string connectedClientsResponse = Encoding.UTF8.GetString(connectedClientsData, 0, responseLength);

            if (connectedClientsResponse.StartsWith("CONNECTED_CLIENTS|"))
            {
                string connectedClientsList = connectedClientsResponse.Substring("CONNECTED_CLIENTS|".Length);
                Console.WriteLine("Inloggade just nu: " + connectedClientsList);
            }
            else
            {
                Console.WriteLine("Unexpected response from the server in HandleConnectedClientsResponse: " + connectedClientsResponse);
            }
        }
    }
}
