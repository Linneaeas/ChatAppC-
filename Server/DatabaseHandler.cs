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
        if (IsUsernameExists(fromUsername))
        {
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
        if (IsUsernameExists(fromUsername))
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

    public static string GetMessageHistory(Socket client, string username)
    {
        var filter = Builders<BsonDocument>.Filter.Or(
            Builders<BsonDocument>.Filter.Eq("to", username),
            Builders<BsonDocument>.Filter.Eq("from", username)
        );

        var sort = Builders<BsonDocument>.Sort.Ascending("_id");

        var messages = messagesCollection?.Find(filter).Sort(sort).Limit(30).ToList();

        if (messages != null && messages.Count > 0)
        {
            List<string> messageList = new List<string>();
            foreach (var message in messages)
            {
                var fromUser = message["from"].AsString;
                var toUser = message.Contains("to") ? message["to"].AsString : "Everyone";
                var chatMessage = message["chatMessage"].AsString;

                var formattedMessage = $"{fromUser};{toUser};{chatMessage}";
                messageList.Add(formattedMessage);
            }

            var allMessages = string.Join("/", messageList);

            Console.WriteLine($"All messages for user {username}: {allMessages}");
            Console.WriteLine($"Last 30 messages for user {username} retrieved");

            byte[] historyData;
            allMessages = $"MESSAGE_HISTORY|{allMessages}";
            historyData = Encoding.UTF8.GetBytes(allMessages);
            client.Send(historyData);

            return allMessages;
        }
        else
        {
            var defaultMessage = $"MESSAGE_HISTORY|No messages to show";
            byte[] defaultData = Encoding.UTF8.GetBytes(defaultMessage);
            client.Send(defaultData);
            Console.WriteLine($"No messages found for user {username}.");
            return defaultMessage;
        }
    }
}