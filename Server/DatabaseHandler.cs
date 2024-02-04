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

    public static void InsertMessage(string fromUsername, List<string> toUsernames, string chatMessage)
    {
        var document = new BsonDocument
    {
        { "from", fromUsername },
        { "to", new BsonArray(toUsernames) },
        { "chatMessage", chatMessage }
    };
        messagesCollection?.InsertOne(document);
        Console.WriteLine($"Message {chatMessage} from {fromUsername} to {string.Join(",", toUsernames)} inserted into MongoDB.");
    }

    /* static void SendAllUsersList(Socket client)
     {
         // Projection to only get the "username" field
         var projection = Builders<BsonDocument>.Projection.Include("username");

         // Query MongoDB to get the list of all usernames
         var allUsers = usersCollection?.Find(new BsonDocument()).Project(projection).ToList();

         // Extract usernames from the documents
         List<string> allUsernames = allUsers?.Select(doc => doc["username"]?.AsString).ToList() ?? new List<string>();


         // Create a response message
         string allUsersResponse = $"ALL_USERS|{string.Join(",", allUsernames)}";

         // Send the response to the client
         byte[] data = Encoding.UTF8.GetBytes(allUsersResponse);
         client.Send(data);
     }*/
}
