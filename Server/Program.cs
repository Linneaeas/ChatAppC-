using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Server;
class Program
{
    static List<Socket> connectedClients = new List<Socket>();
    static Dictionary<string, Socket> socketUserNames = new Dictionary<string, Socket>();
    static List<string> loggedInClients = new List<string>();
    static void Main(string[] args)
    {
        DatabaseHandler.Initialize();

        List<Socket> sockets = new List<Socket>();
        IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, 25500);

        Socket serverSocket = new Socket(
            ipAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp
        );
        serverSocket.Bind(iPEndPoint);
        serverSocket.Listen(5);

        while (true)
        {
            if (serverSocket.Poll(0, SelectMode.SelectRead))
            {
                Socket client = serverSocket.Accept();
                Console.WriteLine("A client has connected!");

                sockets.Add(client);
            }

            foreach (Socket client in sockets)
            {
                if (client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] incoming = new byte[5000];
                    int read = client.Receive(incoming);
                    if (read == 0)
                    {
                        Console.WriteLine("A client has disconnected");
                        continue;
                    }

                    string message = System.Text.Encoding.UTF8.GetString(incoming, 0, read);
                    Console.WriteLine("From a client: " + message);
                    ProcessMessage(client, message);
                }
            }
        }
    }

    static void ProcessMessage(Socket client, string message)
    {
        string[] parts = message.Split('|');
        string username, password, createAccountResponse, loginResponse, logoutResponse, chatMessage, sendPrivateMessageResponse, sendPublicMessageResponse, sendLoginAlert;
        byte[] responseData;

        switch (parts[0])
        {
            case "CREATE_ACCOUNT":
                username = parts[1];
                password = parts[2];
                if (DatabaseHandler.IsUsernameExists(username))
                {
                    createAccountResponse = "ACCOUNT_CREATION_FAILED";
                    responseData = Encoding.UTF8.GetBytes(createAccountResponse);
                    client.Send(responseData);
                }
                else
                {
                    DatabaseHandler.InsertUser(username, password);
                    Console.WriteLine($"Creating account for user: {username} with password: {password}");

                    createAccountResponse = "ACCOUNT_CREATED";
                    responseData = Encoding.UTF8.GetBytes(createAccountResponse);
                    client.Send(responseData);
                }
                break;

            case "LOGIN":
                username = parts[1];
                password = parts[2];
                if (DatabaseHandler.AuthenticateUser(username, password))
                {
                    loginResponse = "LOGIN_SUCCESSFUL";
                    responseData = Encoding.UTF8.GetBytes(loginResponse);
                    client.Send(responseData);

                    Console.WriteLine($"Login successful for user: {username}");

                    sendLoginAlert = $"LOGIN_ALERT|{username}|har loggat in";
                    responseData = Encoding.UTF8.GetBytes(sendLoginAlert);

                    foreach (Socket clientSocket in connectedClients)
                    {
                        clientSocket.Send(responseData);
                    }

                    Console.WriteLine("LOGIN_ALERT from server to client");

                    connectedClients.Add(client);//12.A
                    loggedInClients.Add(username);//12.B

                    Console.WriteLine($"Login successful for user: {username}");
                    socketUserNames[username] = client;

                    DatabaseHandler.GetMessageHistory(client, username);
                }
                else
                {
                    Console.WriteLine($"Login failed for user: {username}");
                    loginResponse = "LOGIN_FAILED";
                    responseData = Encoding.UTF8.GetBytes(loginResponse);
                    client.Send(responseData);
                }
                break;

            case "LOGOUT":
                if (parts.Length == 2)
                {
                    username = parts[1];
                    connectedClients.Remove(client);
                    loggedInClients.Remove(username);
                    logoutResponse = "LOGOUT_SUCCESSFUL";
                    responseData = Encoding.UTF8.GetBytes(logoutResponse);
                    client.Send(responseData);
                }
                else
                {
                    Console.WriteLine("Invalid format for LOGOUT message.");
                }
                break;

            case "GET_CONNECTED_CLIENTS":
                {
                    SendConnectedClientsList(client);
                }
                break;

            case "SEND_MESSAGE_PRIVATE":
                string fromUsername = parts[1];
                string toUsername = parts[2];
                chatMessage = parts[3];

                DatabaseHandler.InsertPrivateMessage(fromUsername, toUsername, chatMessage);
                sendPrivateMessageResponse = $"PRIVATE_MESSAGE_SENT|{fromUsername}|{toUsername}|{chatMessage}";

                responseData = Encoding.UTF8.GetBytes(sendPrivateMessageResponse);
                client.Send(responseData);

                if (socketUserNames.ContainsKey(toUsername))
                {
                    Socket clientRecipient = socketUserNames[toUsername];
                    clientRecipient.Send(responseData);
                }
                Console.WriteLine("PRIVATE_MESSAGE_SENT from server to client");
                break;


            case "SEND_MESSAGE_ALL":
                fromUsername = parts[1];
                chatMessage = parts[2];

                DatabaseHandler.InsertPublicMessage(fromUsername, chatMessage);

                sendPublicMessageResponse = $"PUBLIC_MESSAGE_SENT|{fromUsername}|{chatMessage}";
                responseData = Encoding.UTF8.GetBytes(sendPublicMessageResponse);

                foreach (Socket clientSocket in connectedClients)
                {
                    clientSocket.Send(responseData);
                }

                Console.WriteLine("PUBLIC_MESSAGE_SENT from server to client");
                break;

            default:
                Console.WriteLine("Invalid message received in ProcessMessage.");
                break;
        }

        static void SendConnectedClientsList(Socket client)
        {
            string connectedClientsList = string.Join(",", loggedInClients);
            byte[] connectedData = Encoding.UTF8.GetBytes($"CONNECTED_CLIENTS|{connectedClientsList}");
            client.Send(connectedData);
        }
    }
}
