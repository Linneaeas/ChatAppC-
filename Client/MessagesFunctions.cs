using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;

namespace Client
{
    public class MessagesFunctions
    {
        public static void HandleMessagesResponse(Socket clientSocket)
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
                    Console.WriteLine($"{usernameLoggedIn} {alertMessage}");
                    break;

                default:
                    Console.WriteLine("Invalid response received in HandleServerResponse. " + serverResponse);
                    break;
            }
        }

        public static void SendPrivateMessage(Socket clientSocket, User user)
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

        public static void HandleHistoryMessageResponse(Socket clientSocket)
        {
            byte[] historyData = new byte[5000];
            int responseLength = clientSocket.Receive(historyData);
            string historyResponse = Encoding.UTF8.GetString(historyData, 0, responseLength);

            string[] parts = historyResponse.Split('|');

            if (parts.Length >= 2)
            {
                string messageCase = parts[0];
                string messageHistory = parts[1];

                string[] messageList = messageHistory.Split('/');

                foreach (string eachMessage in messageList)
                {
                    string[] eachMessageList = eachMessage.Split(';');
                    if (eachMessageList.Length >= 3)
                    {
                        string historyFromUser = eachMessageList[0];
                        string historyToUser = eachMessageList[1];
                        string historyMessage = eachMessageList[2];

                        Console.WriteLine(historyFromUser + ": " + historyMessage);
                    }
                    else
                    {
                        Console.WriteLine("Du har inga meddelanden att visa.");
                    }
                }
            }
        }
    }
}




