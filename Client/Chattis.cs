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
            Console.WriteLine("Chattis Meny:");
            Console.WriteLine("Visa denna meny igen, skriv: ^meny");
            Console.WriteLine("Visa inloggade användare, skriv: ^inloggade");
            Console.WriteLine("Logga ut, skriv: ^loggaut");
            Console.WriteLine("Skicka meddelande till alla skriv: alla följt av meddelandet");
            Console.WriteLine("Skicka ett privat meddelande skriv: privat användarnamnet följt av meddelandet");

            while (true)
            {
                string? userInput = Console.ReadLine();

                {
                    switch (userInput)
                    {
                        case "^meny":
                            ChattisMenu(clientSocket, user); //Show Chattis Meny
                            break;

                        case "^inloggade":
                            LoginRegistration.SendConnectedClientsListRequest(clientSocket);//Call the method that collects the logged in users
                            LoginRegistration.HandleConnectedClientsResponse(clientSocket);// Call the method that displays logged in users
                            break;

                        case "^loggaut":
                            LoginRegistration.SendLogoutRequest(clientSocket, user);//Call the method that collects the logged in users
                            LoginRegistration.HandleLogoutResponse(clientSocket);// Call the method that displays logged in 
                            break;


                        // Inside the default case in the switch statement
                        default:
                            // Check if the input starts with "send all"
                            if (userInput.StartsWith("alla "))
                            {
                                // Extract the message to be sent to all users
                                string message = userInput.Substring("alla ".Length).Trim();

                                // Formulate the message to be sent to the server for broadcasting
                                string sendMessageRequest = $"SEND_MESSAGE|alla|{message}";

                                // Convert the message to bytes and send it to the server
                                byte[] data = Encoding.UTF8.GetBytes(sendMessageRequest);
                                clientSocket.Send(data);
                            }

                            // Check if the input starts with "send"
                            else if (userInput.StartsWith("privat "))
                            {
                                // Extract the recipient username and message
                                string[] parts = userInput.Split(' ', 3);
                                if (parts.Length == 3)
                                {
                                    string toUsername = parts[1];
                                    string message = parts[2];

                                    // Formulate the message to be sent to the server for private messaging
                                    string sendMessageRequest = $"SEND_MESSAGE_PRIVATE|{user.UserName}|{toUsername}|{message}";

                                    // Convert the message to bytes and send it to the server
                                    byte[] data = Encoding.UTF8.GetBytes(sendMessageRequest);
                                    clientSocket.Send(data);
                                }
                                else
                                {
                                    Console.WriteLine("Invalid command format. Type ^meny for menu options.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid command. Type ^meny for menu options.");
                            }
                            break;
                    }
                }
            }
        }
        /*  public static void SendAllUsersListRequest(Socket clientSocket)
          {
              string getAllUsersRequest = "GET_ALL_USERS";
              byte[] allUsersData = Encoding.UTF8.GetBytes(getAllUsersRequest);

              clientSocket.Send(allUsersData);
          }

          public static void HandleAllUsersResponse(Socket clientSocket)
          {
              // Read the server's response
              byte[] allUsersData = new byte[5000];
              int responseLength = clientSocket.Receive(allUsersData);
              string allUsersResponse = Encoding.UTF8.GetString(allUsersData, 0, responseLength);

              // Process the server's response & outcome depending on which response:
              if (allUsersResponse.StartsWith("ALL_USERS|"))
              {
                  // Extract the connected clients list from the response
                  string allUsersList = allUsersResponse.Substring("ALL_USERS|".Length);

              }
              else
              {
                  Console.WriteLine("Unexpected response from the server in HandleAllUsersResponse.");
              } 
          }*/
    }
}