namespace Project;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

class Program
{
    static IMongoCollection<BsonDocument> usersCollection;

    static void Main(string[] args)
    {
        // Set up MongoDB connection
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
            ipAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp
        );
        // Bind the server socket to the specified endpoint
        serverSocket.Bind(iPEndPoint);
        // Start listening for incoming connections with a backlog of 5
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
                    Console.WriteLine("From a client: " + message);

                    // Process the received message based on the protocol
                    ProcessMessage(client, message);
                }
            }
        }
    }
    static void ProcessMessage(Socket client, string message)
    {
        // Split the message into parts using the pipe character (|) as a separator
        string[] parts = message.Split('|');

        // Check the first part of the message to determine the action
        switch (parts[0])
        {
            case "CREATE_ACCOUNT":
                // Parts[1] contains the username, and Parts[2] contains the password
                string username = parts[1];
                string password = parts[2];

                // Insert user into MongoDB
                InsertUser(username, password);

                // Process the account creation request (you may want to add more validation)
                Console.WriteLine($"Creating account for user: {username} with password: {password}");

                // Send a response back to the client (you may want to define a response protocol)
                string response = "Kontot är nu registrerat!";
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                client.Send(responseData);


                break;

            // Add more cases for other message types as needed

            default:
                Console.WriteLine("Invalid message received.");
                break;
        }
    }
    static void InsertUser(string username, string password)
    {
        var document = new BsonDocument
            {
                { "username", username },
                { "password", password }
            };

        usersCollection.InsertOne(document);

        Console.WriteLine($"User {username} inserted into MongoDB.");
    }
}

