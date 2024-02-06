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
    // List to store all connected client sockets:
    //12.A
    static List<Socket> connectedClients = new List<Socket>();
    static Dictionary<string, Socket> socketUserNames = new Dictionary<string, Socket>(); //19

    // List to store logged-in client usernames:
    //12.B
    static List<string> loggedInClients = new List<string>();

    // 2.A CREATE SERVER
    static void Main(string[] args)
    {
        DatabaseHandler.Initialize();

        // List to store connected client sockets:
        List<Socket> sockets = new List<Socket>();
        // Server's IP address (localhost in this case):
        IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        // Endpoint for the server (IP address + port):
        IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, 25500);

        // Create a socket for the server:
        Socket serverSocket = new Socket(
            ipAddress.AddressFamily, // how ir handles the addressfamily 
            SocketType.Stream, // Specify sockettype, we choose stream to make it be able to keep sedning messages 
            ProtocolType.Tcp // Which protoype we want to use, which is TCP
        );
        // Bind the server socket to the specified endpoint:
        serverSocket.Bind(iPEndPoint);
        // Start listening for incoming connections with a backlog of 5 /Error prev. in case of multiple requests. May not be needed for this small project:
        serverSocket.Listen(5);

        // Infinite loop to keep the server running:
        while (true)
        {
            // Check if there is an incoming connection // Does'nt block the code in case no coming connection:
            if (serverSocket.Poll(0, SelectMode.SelectRead))
            {
                // Accept the incoming connection and get the client socket:
                Socket client = serverSocket.Accept();
                Console.WriteLine("A client has connected!");
                // Add the client socket to the list:
                sockets.Add(client);
            }

            // Loop through each connected client socket:
            foreach (Socket client in sockets)
            {

                // Check if there is data to read from the client // Does'nt block the code in case there's nothing to read:
                if (client.Poll(0, SelectMode.SelectRead))
                {
                    // Read the incoming data from the client:
                    byte[] incoming = new byte[5000];
                    int read = client.Receive(incoming);

                    if (read == 0)
                    {
                        Console.WriteLine("A client has disconnected");
                        continue;
                    }
                    // Convert the received bytes to a string message:
                    string message = System.Text.Encoding.UTF8.GetString(incoming, 0, read);
                    //Confirmation that the server recieved a message from client:
                    Console.WriteLine("From a client: " + message);

                    // Process the received message based on the protocol:
                    ProcessMessage(client, message);
                    // Send the connected clients list to each client: 

                }
            }
        }
    }

    /*5.A RECIEVE & PROCESS INFORMATION FROM THE CLIENT*/
    static void ProcessMessage(Socket client, string message)
    {
        // Split the message into parts using the pipe character (|) as a separator:
        string[] parts = message.Split('|');
        //Someone can not choose this sign inside their username/password its gonna break. If we got time we can put in some kind of "allowed signs"

        // Declare variables outside the switch block:
        string username, password, createAccountResponse, loginResponse, logoutResponse, chatMessage, sendPrivateMessageResponse, sendPublicMessageResponse, sendLoginAlert; //Added logoutResponse & chatmessage & sendMessageRespons
        byte[] responseData;


        // Check the first part of the message to determine the action:
        switch (parts[0])
        {

            /*10.A CASE FOR LOGIN RESPONSES*/
            case "CREATE_ACCOUNT":
                // Parts[1] contains the username, and Parts[2] contains the password:
                username = parts[1];
                password = parts[2];


                // Check if the username already exists in the MongoDB collection:
                if (DatabaseHandler.IsUsernameExists(username))//11.B
                {
                    // Send a response back to the client indicating that the account creation failed:
                    createAccountResponse = "ACCOUNT_CREATION_FAILED";
                    responseData = Encoding.UTF8.GetBytes(createAccountResponse);
                    client.Send(responseData);
                }
                else
                {
                    // Insert user into MongoDB:
                    DatabaseHandler.InsertUser(username, password);//6.G

                    // Process the account creation request (you may want to add more validation):
                    Console.WriteLine($"Creating account for user: {username} with password: {password}");//5.C Confirmation registration has gone through.

                    // Send a response back to the client indicating successful account creation:
                    createAccountResponse = "ACCOUNT_CREATED"; //5.B
                    responseData = Encoding.UTF8.GetBytes(createAccountResponse);
                    client.Send(responseData);
                }
                break;

            /*7.C, 8.A HANDLE LOGIN RESPONSES*/
            case "LOGIN":
                // Parts[1] contains the username, and Parts[2] contains the password:
                username = parts[1];
                password = parts[2];

                // Check if the provided credentials are valid:
                if (DatabaseHandler.AuthenticateUser(username, password))//7.E

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

                    //Displays in the server terminal:

                    // Broadcast the login message to all connected clients:



                    Console.WriteLine($"Login successful for user: {username}");
                    socketUserNames[username] = client;




                }
                else
                {
                    //Displays in the server terminal:
                    Console.WriteLine($"Login failed for user: {username}");
                    // Send a response back to the client indicating failed login:
                    loginResponse = "LOGIN_FAILED";
                    responseData = Encoding.UTF8.GetBytes(loginResponse);
                    client.Send(responseData);
                }
                break;


            case "LOGOUT":
                if (parts.Length == 2) // Check if there is exactly one part (username)
                {
                    username = parts[1];
                    // Remove the client from the lists:

                    connectedClients.Remove(client);
                    loggedInClients.Remove(username);
                    logoutResponse = "LOGOUT_SUCCESSFUL";
                    responseData = Encoding.UTF8.GetBytes(logoutResponse);
                    client.Send(responseData);
                }
                else
                {
                    // Handle unexpected format of "LOGOUT" message
                    Console.WriteLine("Invalid format for LOGOUT message.");
                }
                break;


            //  If a user requests the list of connected users, the serevr send this 
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
                client.Send(responseData);

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
    }

    //12.D
    static void SendConnectedClientsList(Socket client)
    {
        string connectedClientsList = string.Join(",", loggedInClients);
        byte[] connectedData = Encoding.UTF8.GetBytes($"CONNECTED_CLIENTS|{connectedClientsList}");
        client.Send(connectedData);
    }

}
