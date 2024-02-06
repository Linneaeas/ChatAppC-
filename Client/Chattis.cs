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
            Console.WriteLine("Visa inloggade användare, skriv: ^inloggade");
            Console.WriteLine("Logga ut, skriv: ^loggaut");
            Console.WriteLine("Privat meddelande, skriv: ^privat");
            Console.WriteLine("Skicka meddelande till alla skriv: alla följt av meddelandet");
            Console.WriteLine("----------------------------");

            while (true)
            {

                if (Console.KeyAvailable) //19
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
                                // Get password from the user (you might want to handle password input securely):
                                Console.Write("Meddelande: ");
                                message = Console.ReadLine();

                                // Formulate the message to be sent to the server for private messaging
                                string sendMessageRequest = $"SEND_MESSAGE_PRIVATE|{user.UserName}|{toUsername}|{message}";

                                // Convert the message to bytes and send it to the server
                                byte[] data = Encoding.UTF8.GetBytes(sendMessageRequest);
                                clientSocket.Send(data);
                            }

                            break;


                        // Inside the default case in the switch statement
                        default:
                            // Check if the input starts with "send all"
                            if (userInput.StartsWith("alla "))
                            {
                                // Extract the recipient username and message
                                string[] parts = userInput.Split(' ', 2);
                                if (parts.Length == 2)
                                {

                                    string message = parts[1];

                                    // Formulate the message to be sent to the server for private messaging
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
                    Chattis.HandleServerResponse(clientSocket);
                }

                // Thread.Sleep(200);
            }
        }


        public static void HandleServerResponse(Socket clientSocket)
        {
            // Read & decode the server's response:
            byte[] responseBytes = new byte[5000];
            int responseLength = clientSocket.Receive(responseBytes);
            string sendPrivateMessageResponse = Encoding.UTF8.GetString(responseBytes, 0, responseLength);

            // Split the response into parts using the pipe character (|) as a separator:
            string[] parts = sendPrivateMessageResponse.Split('|');

            // Check the first part of the response to determine the action:
            switch (parts[0])
            {
                // Process the server's response & outcome depending on which response:
                case "PRIVATE_MESSAGE_SENT":
                    string fromUsername = parts[1];
                    string toUsername = parts[2];
                    string chatMessage = parts[3];

                    // Handle the private message (print or do whatever is needed)
                    Console.WriteLine($"Privat från {fromUsername} till {toUsername}: {chatMessage}");
                    break;

                case "PUBLIC_MESSAGE_SENT":
                    fromUsername = parts[1];
                    chatMessage = parts[2];


                    // Handle the public message (print or do whatever is needed)
                    Console.WriteLine($"{fromUsername}: {chatMessage}");
                    break;

                case "LOGIN_ALERT":
                    string usernameLoggedIn = parts[1];
                    string alertMessage = parts[2];


                    // Handle the public message (print or do whatever is needed)
                    Console.WriteLine($"{usernameLoggedIn}: {alertMessage}");
                    break;


                default:
                    Console.WriteLine("Invalid response received in HandleServerResponse.");
                    break;
            }
        }


    }
}