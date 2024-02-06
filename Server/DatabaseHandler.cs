using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Server;

public class DatabaseHandler
{
    //6.E Set up MongoDB Connection:
    //The code establishes a connection to a MongoDB server running on localhost at port 27017.It selects the database named "ChatApp" and gets a collection named "users" as an IMongoCollection<BsonDocument>.
    private static IMongoCollection<BsonDocument>? usersCollection;
    private static IMongoCollection<BsonDocument>? messagesCollection;

    public static void Initialize()
    {
        MongoClient mongoClient = new MongoClient("mongodb://localhost:27017");
        IMongoDatabase database = mongoClient.GetDatabase("ChatApp");
        usersCollection = database.GetCollection<BsonDocument>("users");
        messagesCollection = database.GetCollection<BsonDocument>("messages");
    }

    public static bool AuthenticateUser(string username, string password)
    {
        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("username", username),
            Builders<BsonDocument>.Filter.Eq("password", password)
        );
        var user = usersCollection.Find(filter).FirstOrDefault();
        return user != null;
    }

    public static void InsertUser(string username, string password)
    {
        var document = new BsonDocument
            {
                { "username", username },
                { "password", password }
            };
        usersCollection?.InsertOne(document);
        Console.WriteLine($"User {username} inserted into MongoDB.");
    }

    public static bool IsUsernameExists(string username)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("username", username);
        var existingUser = usersCollection?.Find(filter).FirstOrDefault();
        return existingUser != null;
    }

    public static void InsertPrivateMessage(string fromUsername, string toUsername, string chatMessage)
    {
        // Check if the sender username exists in the database
        if (IsUsernameExists(fromUsername))
        {
            // Check if the receiver username exists in the database
            if (IsUsernameExists(toUsername))
            {
                var document = new BsonDocument
            {
                { "from", fromUsername },
                { "to", toUsername },
                { "chatMessage", chatMessage }
            };
                messagesCollection?.InsertOne(document);
                Console.WriteLine($"Message {chatMessage} from {fromUsername} to {toUsername} inserted into MongoDB.");
            }
            else
            {
                Console.WriteLine($"Invalid private message: The receiver username '{toUsername}' does not exist.");
            }
        }
        else
        {
            Console.WriteLine($"Invalid private message: The sender username '{fromUsername}' does not exist.");
        }

    }

    public static void InsertPublicMessage(string fromUsername, string chatMessage)
    {
        // Check if the sender username exists in the database
        if (IsUsernameExists(fromUsername))
        // Check if the receiver username exists in the database
        {
            var document = new BsonDocument
            {
                { "from", fromUsername },

                { "chatMessage", chatMessage }
            };
            messagesCollection?.InsertOne(document);
            Console.WriteLine($"Message {chatMessage} from {fromUsername} inserted into MongoDB.");
        }
        else
        {
            Console.WriteLine($"Invalid public message");
        }
    }

}