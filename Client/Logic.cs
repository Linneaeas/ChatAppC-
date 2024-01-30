using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class Logic
    {

        public class User
        {
            public string? UserName { get; set; }
            public string? Password { get; set; }
        }


        public static void HuvudMeny(Socket clientSocket)
        {
            Console.WriteLine("Huvud Menu:");
            Console.WriteLine("1. Logga in");
            Console.WriteLine("2. Skapa konto");

            Console.Write("Vad vill du göra?: ");
            string? userInput = Console.ReadLine();
            // Process the user input and perform actions based on the selected option
            switch (userInput)
            {
                case "1":
                    Console.WriteLine("Selected Option 1");
                    break;
                case "2":
                    SkapaKonto(clientSocket);
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }

        public static void SkapaKonto(Socket clientSocket)
        {
            Console.WriteLine("Skapa nytt konto:");

            // Create a new User instance
            User newUser = new User();

            // Get username from the user
            Console.Write("Välj användarnamn: ");
            newUser.UserName = Console.ReadLine();

            // Get password from the user (you might want to handle password input securely)
            Console.Write("Välj lösenord: ");
            newUser.Password = Console.ReadLine();

            // Send the user information to the server
            SendUserInformation(newUser, clientSocket);


        }
        public static void SendUserInformation(User user, Socket clientSocket)
        {
            // Formulate a message or data structure to send to the server
            // indicating that an account creation request is being made.
            string accountCreationRequest = $"CREATE_ACCOUNT|{user.UserName}|{user.Password}";
            byte[] data = Encoding.UTF8.GetBytes(accountCreationRequest);

            // Send the account creation request to the server
            clientSocket.Send(data);

            // Wait for and process the server's response
            // (Handle server response in a separate method or thread)
        }
    }
}
