namespace Project;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

class Program // Server 
{
    /*6.D INITIALIZE MONGO DB COLLECTION USERS----------------------------------------------------------------------------------------*/
    static IMongoCollection<BsonDocument>? usersCollection;
    // List to store all connected client sockets
    static List<Socket> connectedClients = new List<Socket>();
    // List to store logged-in client usernames
    static List<string> loggedInClients = new List<string>();

    // Method to send a list of connected clients to a specific client

    /*2.A CREATE SERVER---------------------------------------------------------------------------------------------------*/
    static void Main(string[] args)
    {
        /*6.E SET UP MONGODB CONNECTION----------------------------------------------------------------------------------------*/
        //The code establishes a connection to a MongoDB server running on localhost at port 27017.It selects the database named "ChatApp" and gets a collection named "users" as an IMongoCollection<BsonDocument>.
        MongoClient mongoClient = new MongoClient("mongodb://localhost:27017");
        IMongoDatabase database = mongoClient.GetDatabase("ChatApp");
        usersCollection = database.GetCollection<BsonDocument>("users");

        // List to store connected client sockets
        List<Socket> sockets = new List<Socket>();
        // Server's IP address (localhost in this case)
        IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        // Endpoint for the server (IP address + port)
        IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, 25500);

        // Create a socket for the server
        Socket serverSocket = new Socket(
            ipAddress.AddressFamily, // how ir handles the addressfamily 
            SocketType.Stream, // Specify sockettype, we choose stream to make it be able to keep sedning messages 
            ProtocolType.Tcp // Which protoype we want to use, which is TCP
        );
        // Bind the server socket to the specified endpoint
        serverSocket.Bind(iPEndPoint);
        // Start listening for incoming connections with a backlog of 5 /Error prev. in case of multiple requests. May not be needed for this small project.
        serverSocket.Listen(5);

        // Infinite loop to keep the server running
        while (true)
        {
            // Blockar inte koden om det inte finns en inkommande anslutning.
            // Check if there is an incoming connection
            if (serverSocket.Poll(0, SelectMode.SelectRead))
            {
                // Accept the incoming connection and get the client socket
                Socket client = serverSocket.Accept();
                Console.WriteLine("A client has connected!");
                // Add the client socket to the list
                sockets.Add(client);
            }

            // Loop through each connected client socket
            foreach (Socket client in sockets)
            {
                // Blockar inte koden om det inte finns något att läsa.
                // Check if there is data to read from the client
                if (client.Poll(0, SelectMode.SelectRead))
                {
                    // Read the incoming data from the client
                    byte[] incoming = new byte[5000];
                    int read = client.Receive(incoming);
                    // Convert the received bytes to a string message
                    string message = System.Text.Encoding.UTF8.GetString(incoming, 0, read);
                    Console.WriteLine("From a client: " + message);//Confirmation that the server recieved a message from client

                    // Process the received message based on the protocol
                    ProcessMessage(client, message);
                    SendConnectedClientsList(client); // Send the connected clients list to each client//Works for the first client that logs in
                }
            }
        }
    }

    /*5. RECIEVE MESSAGE DATA---------------------------------------------------------------------------------------------------*/
    static void ProcessMessage(Socket client, string message)
    {
        // Split the message into parts using the pipe character (|) as a separator
        string[] parts = message.Split('|'); //Someone can not choose this sign inside their username/password its gonna break. If we got time we can put in some kind of "allowed signs"

        // Declare variables outside the switch block
        string username, password, response;//kanske CreateAccountResponse eller Objekt: respons som ar av typ createAccount/Login etc.
        byte[] responseData;


        // Check the first part of the message to determine the action
        switch (parts[0])
        {

            /*10.A CASE FOR LOGIN RESPONSES----------------------------------------------------------*/
            case "CREATE_ACCOUNT":
                // Parts[1] contains the username, and Parts[2] contains the password
                username = parts[1];
                password = parts[2];


                // Check if the username already exists in the MongoDB collection
                if (IsUsernameExists(username))
                {
                    // Send a response back to the client indicating that the account creation failed
                    response = "ACCOUNT_CREATION_FAILED";
                    responseData = Encoding.UTF8.GetBytes(response);
                    client.Send(responseData);
                }
                else
                {
                    // Insert user into MongoDB
                    InsertUser(username, password);

                    // Process the account creation request (you may want to add more validation)
                    Console.WriteLine($"Creating account for user: {username} with password: {password}");//Confirmation registration has gone through.

                    // Send a response back to the client indicating successful account creation
                    response = "ACCOUNT_CREATED";
                    responseData = Encoding.UTF8.GetBytes(response);
                    client.Send(responseData);
                }
                break;

            /*7.E CASE FOR LOGIN RESPONSES----------------------------------------------------------*/
            case "LOGIN":
                // Parts[1] contains the username, and Parts[2] contains the password
                username = parts[1];
                password = parts[2];

                // Check if the provided credentials are valid
                if (AuthenticateUser(username, password))
                {
                    Console.WriteLine($"Login successful for user: {username}");//Displays on server.

                    connectedClients.Add(client);
                    loggedInClients.Add(username);

                    // Send a response back to the client indicating successful login:
                    response = "LOGIN_SUCCESSFUL";
                    responseData = Encoding.UTF8.GetBytes(response);
                    client.Send(responseData);
                }
                else
                {
                    Console.WriteLine($"Login failed for user: {username}");//Displays on Server.

                    // Send a response back to the client indicating failed login
                    response = "LOGIN_FAILED";
                    responseData = Encoding.UTF8.GetBytes(response);
                    client.Send(responseData);
                }
                break;

            // Add more cases for other message types as needed
            case "LOGOUT":
                username = parts[1];
                // Remove the client from the lists
                connectedClients.Remove(client);
                loggedInClients.Remove(username);
                Console.WriteLine($"User {username} logged out.");
                break;

            default:
                Console.WriteLine("Invalid message received.");
                break;
        }
    }

    /*7.D CHECKS THAT USER & PASSWORD ARE CORRECT/EXISTS ----------------------------------------------------------*/
    static bool AuthenticateUser(string username, string password)
    {
        // Query MongoDB to check if the provided username and password match any stored user
        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("username", username),
            Builders<BsonDocument>.Filter.Eq("password", password)
        );

        var user = usersCollection.Find(filter).FirstOrDefault();

        // If a user is found, authentication is successful
        return user != null;
    }

    /*6.F ADDS USER TO MONGODB----------------------------------------------------------*/
    static void InsertUser(string username, string password)
    {
        var document = new BsonDocument
            {
                { "username", username },
                { "password", password }
            };

        usersCollection?.InsertOne(document);

        Console.WriteLine($"User {username} inserted into MongoDB.");
    }

    /*11 CHECK IF USERNAME EXISTS---------------------------------------------------------------------*/
    static bool IsUsernameExists(string username)
    {
        // Query MongoDB to check if the provided username already exists
        var filter = Builders<BsonDocument>.Filter.Eq("username", username);
        var existingUser = usersCollection?.Find(filter).FirstOrDefault();

        // If an existing user is found, the username already exists
        return existingUser != null;
    }
    static void SendConnectedClientsList(Socket client)
    {
        string connectedClientsList = string.Join(",", loggedInClients);
        byte[] data = Encoding.UTF8.GetBytes($"CONNECTED_CLIENTS|{connectedClientsList}");
        client.Send(data);
    }
}

