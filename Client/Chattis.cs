using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;

namespace Client
{
    public class Chattis
    {
        public static void ChattisMenu(Socket clientSocket, User user)
        {
            Console.WriteLine("----------------------------");
            Console.WriteLine("Chattis Meny:");
            Console.WriteLine("Visa denna meny igen, skriv: ^meny");
            Console.WriteLine("Visa inloggade, skriv: ^inloggade");
            Console.WriteLine("Logga ut, skriv: ^loggaut");
            Console.WriteLine("Privat meddelande, skriv: ^privat");
            Console.WriteLine("Skicka meddelande till alla skriv: alla <meddelandet>");
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
                            LoginRegistration.SendConnectedClientsListRequest(clientSocket);
                            LoginRegistration.HandleConnectedClientsResponse(clientSocket);
                            break;

                        case "^loggaut":
                            LoginRegistration.SendLogoutRequest(clientSocket, user);
                            LoginRegistration.HandleLogoutResponse(clientSocket);
                            break;

                        case "^privat":
                            LoginRegistration.SendConnectedClientsListRequest(clientSocket);
                            LoginRegistration.HandleConnectedClientsResponse(clientSocket);
                            {
                                string? toUsername;
                                string? message;

                                Console.Write("Vem vill du skicka till: ");
                                toUsername = Console.ReadLine();
                                Console.Write("Meddelande: ");
                                message = Console.ReadLine();

                                string sendMessageRequest = $"SEND_MESSAGE_PRIVATE|{user.UserName}|{toUsername}|{message}";

                                byte[] data = Encoding.UTF8.GetBytes(sendMessageRequest);
                                clientSocket.Send(data);
                            }
                            break;

                        default:
                            if (userInput.StartsWith("alla "))
                            {
                                string[] parts = userInput.Split(' ', 2);
                                if (parts.Length == 2)
                                {
                                    string message = parts[1];
                                    string sendMessageRequest = $"SEND_MESSAGE_ALL|{user.UserName}|{message}";

                                    // Convert the message to bytes and send it to the server
                                    byte[] data = Encoding.UTF8.GetBytes(sendMessageRequest);
                                    clientSocket.Send(data);
                                }
                            }
                            break;
                    }
                }
                if (clientSocket.Available != 0)
                {
                    HandleServerResponse(clientSocket);
                }
                Thread.Sleep(200);
            }
        }

        public static void HandleServerResponse(Socket clientSocket)
        {
            byte[] responseBytes = new byte[5000];
            int responseLength = clientSocket.Receive(responseBytes);
            string serverResponse = Encoding.UTF8.GetString(responseBytes, 0, responseLength);
            string[] parts = serverResponse.Split('|');

            switch (parts[0])
            {

                case "PRIVATE_MESSAGE_SENT":
                    string fromUsername = parts[1];
                    string toUsername = parts[2];
                    string chatMessage = parts[3];

                    Console.WriteLine($"Privat från {fromUsername} till {toUsername}: {chatMessage}");
                    break;

                case "PUBLIC_MESSAGE_SENT":
                    fromUsername = parts[1];
                    chatMessage = parts[2];
                    Console.WriteLine($"{fromUsername}: {chatMessage}");
                    break;

                case "LOGIN_ALERT":
                    string usernameLoggedIn = parts[1];
                    string alertMessage = parts[2];
                    Console.WriteLine($"{usernameLoggedIn}: {alertMessage}");
                    break;

                default:
                    Console.WriteLine("Invalid response received in HandleServerResponse. " + serverResponse);
                    break;
            }
        }

        /*   public static void SendPrivateMessage(Socket clientSocket)
           {
               LoginRegistration.SendConnectedClientsListRequest(clientSocket);
               LoginRegistration.HandleConnectedClientsResponse(clientSocket);
               {
                   string? toUsername;
                   string? message;

                   Console.Write("Vem vill du skicka till: ");
                   toUsername = Console.ReadLine();
                   Console.Write("Meddelande: ");
                   message = Console.ReadLine();

                   string sendMessageRequest = $"SEND_MESSAGE_PRIVATE|{user.UserName}|{toUsername}|{message}";

                   byte[] data = Encoding.UTF8.GetBytes(sendMessageRequest);
                   clientSocket.Send(data);
               }
           }*/


    }
}




