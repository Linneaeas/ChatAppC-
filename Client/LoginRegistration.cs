using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class LoginRegistration
    {
        /*4.A USER CLASS---------------------------------------------------------------------------------------------------*/
        public class User
        {
            public string? UserName { get; set; }
            public string? Password { get; set; }
        }

        /*4.B MAIN MENU---------------------------------------------------------------------------------------------------*/
        public static void HuvudMeny(Socket clientSocket)
        {
            Console.WriteLine("Huvud Menu:");
            Console.WriteLine("1. Skapa konto");
            Console.WriteLine("2. Logga in");

            Console.Write("Vad vill du göra?: ");
            string? userInput = Console.ReadLine();
            // Process the user input and perform actions based on the selected option
            switch (userInput)
            {
                case "1":
                    SkapaKonto(clientSocket);
                    break;
                case "2":
                    LoggaIn(clientSocket);
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }


        /*4.C CREATE ACCOUNT----------------------------------------------------------------------------------------------------------*/
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
            SendAccountInformation(newUser, clientSocket);
        }

        /*4.D SEND THE NEW ACCOUNT INFORMATION TO THE SERVER-------------------------------------------------------------------------*/
        public static void SendAccountInformation(User user, Socket clientSocket)
        {
            // Formulate a message or data structure to send to the server
            // indicating that an account creation request is being made.
            string accountCreationRequest = $"CREATE_ACCOUNT|{user.UserName}|{user.Password}";
            byte[] data = Encoding.UTF8.GetBytes(accountCreationRequest);

            // Send the account creation request to the server
            clientSocket.Send(data);

            // Wait for and process the server's response
            // (Handle server response in a separate method or thread)
            HandleCreateAccountResponse(clientSocket);

        }

        /*10 CREATE ACCOUNT: RECIEVE RESPONSE FROM SERVER & HANDLE THE RESPONSE ---------------------------------*/
        public static void HandleCreateAccountResponse(Socket clientSocket)
        {
            // Read the server's response
            byte[] responseBytes = new byte[5000];
            int responseLength = clientSocket.Receive(responseBytes);
            string response = Encoding.UTF8.GetString(responseBytes, 0, responseLength);

            // Process the server's response
            switch (response)
            {

                case "ACCOUNT_CREATED":
                    Console.WriteLine("Kontot är nu skapat!");
                    HuvudMeny(clientSocket);
                    break;

                case "ACCOUNT_CREATION_FAILED":
                    Console.WriteLine("Användarnamnet är upptaget, valj ett annat användarnam!");
                    HuvudMeny(clientSocket);
                    break;
                default:
                    Console.WriteLine("Unexpected response from the server.");
                    break;
            }
        }


        /*7.A LOGIN FUNCTION------------------------------------------------------------------------------------------------------------*/
        public static void LoggaIn(Socket clientSocket)
        {
            Console.WriteLine("Logga in:");

            // Create a new User instance
            User existingUser = new User();

            // Get username from the user
            Console.Write("Ange användarnamn: ");
            existingUser.UserName = Console.ReadLine();

            // Get password from the user (you might want to handle password input securely)
            Console.Write("Ange lösenord: ");
            existingUser.Password = Console.ReadLine();

            // Send the user information to the server for login
            SendLoginInformation(existingUser, clientSocket);
        }

        /*7.B SEND LOGIN INFORMATION TO THE SERVER------------------------------------------------------------------------------------*/
        public static void SendLoginInformation(User user, Socket clientSocket)
        {
            // Formulate a message or data structure to send to the server
            // indicating that a login request is being made.
            string loginRequest = $"LOGIN|{user.UserName}|{user.Password}";
            byte[] data = Encoding.UTF8.GetBytes(loginRequest);

            // Send the login request to the server
            clientSocket.Send(data);
            // Wait for and process the server's response
            // (Handle server response in a separate method or thread)
            HandleLoginResponse(clientSocket);
        }

        /*8.A + 10 RECIEVE RESPONSE FROM SERVER & HANDLE THE RESPONSE ---------------------------------*/
        public static void HandleLoginResponse(Socket clientSocket)
        {
            // Read the server's response
            byte[] responseBytes = new byte[5000];
            int responseLength = clientSocket.Receive(responseBytes);
            string response = Encoding.UTF8.GetString(responseBytes, 0, responseLength);

            // Process the server's response
            switch (response)
            {
                case "LOGIN_SUCCESSFUL":
                    Console.WriteLine("Välkommen till Chattis!");
                    // Add code to proceed after successful login if needed

                    break;
                case "LOGIN_FAILED":
                    Console.WriteLine("Login missslyckades, försök igen.");
                    // Prompt the user to log in again
                    HuvudMeny(clientSocket);
                    break;

                default:
                    Console.WriteLine("Unexpected response from the server.");
                    break;
            }
        }
    }
}
