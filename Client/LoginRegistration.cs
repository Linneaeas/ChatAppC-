using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class LoginRegistration
    {
        //
        /*4.A USER CLASS---------------------------------------------------------------------------------------------------------------------------USER CLASS 4.A */
        //
        public class User
        {
            public string? UserName { get; set; }
            public string? Password { get; set; }
        }

        //
        /*4.B MAIN MENU-----------------------------------------------------------------------------------------------------------------------------MAIN MENU 4.B */
        //
        public static void MainMenu(Socket clientSocket)
        {
            Console.WriteLine("Huvud Menu:");
            Console.WriteLine("1. Skapa konto");
            Console.WriteLine("2. Logga in");
            Console.WriteLine("Vad vill du göra?: ");
            string? userInput = Console.ReadLine();

            // Process the user input and perform actions based on the selected option:
            switch (userInput)
            {
                case "1":
                    CreateAccount(clientSocket);
                    break;
                case "2":
                    LogIn(clientSocket);
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }

        //
        /*4.C CREATE ACCOUNT-------------------------------------------------------------------------------------------------------------------CREATE ACCOUNT 4.C */
        //
        public static void CreateAccount(Socket clientSocket)
        {
            Console.WriteLine("Skapa nytt konto:");

            // Create a new User instance:
            User newUser = new User();

            // Get username from the user:
            Console.Write("Välj användarnamn: ");
            newUser.UserName = Console.ReadLine();

            // Get password from the user (you might want to handle password input securely):
            Console.Write("Välj lösenord: ");
            newUser.Password = Console.ReadLine();

            // Send the user information to the server:
            SendAccountInformation(newUser, clientSocket);
        }

        //
        /*4.D SEND ACCOUNT INFORMATION TO THE SERVER-------------------------------------------------------------------SEND ACCOUNT INFORMATION TO THE SERVER 4.D */
        //
        public static void SendAccountInformation(User user, Socket clientSocket)
        {
            // Formulate a message/data structure to send to the server. Specifying that an account creation request is being made:
            string accountCreationRequest = $"CREATE_ACCOUNT|{user.UserName}|{user.Password}";
            byte[] data = Encoding.UTF8.GetBytes(accountCreationRequest);
            // Send the account creation request to the server:
            clientSocket.Send(data);

            // Handle the response from the server with the separate method:
            HandleCreateAccountResponse(clientSocket);//10.B
        }

        //
        /*5.B+10.A RECIVE & PROCESS CREATE ACCOUNT RESPONSE FROM SERVER ----------------------------RECIVE & PROCESS CREATE ACCOUNT RESPONSE FROM SERVER 5.B+10.A */
        //
        public static void HandleCreateAccountResponse(Socket clientSocket)
        {
            // Read & decode the server's response:
            byte[] responseBytes = new byte[5000];
            int responseLength = clientSocket.Receive(responseBytes);
            string createAccountResponse = Encoding.UTF8.//13
            GetString(responseBytes, 0, responseLength);

            // Process the server's response & outcome depending on which response:
            switch (createAccountResponse)
            {
                case "ACCOUNT_CREATED":
                    Console.WriteLine("Kontot är nu skapat!");//5.B
                    MainMenu(clientSocket);
                    break;

                case "ACCOUNT_CREATION_FAILED":
                    Console.WriteLine("Användarnamnet är upptaget, valj ett annat användarnam!");//11.A
                    MainMenu(clientSocket);
                    break;
                default:
                    Console.WriteLine("Unexpected response from the server in HandleCreateAccountResponse.");
                    break;
            }
        }

        //
        /*7.A LOGIN FUNCTION--------------------------------------------------------------------------------------------------------------------LOGIN FUNCTION 7.A*/
        //
        public static void LogIn(Socket clientSocket)
        {
            Console.WriteLine("Logga in:");

            // Create a new User instance specifically for existning users:
            User existingUser = new User();

            // Get username from the user:
            Console.Write("Ange användarnamn: ");
            existingUser.UserName = Console.ReadLine();
            // Get password from the user (you might want to handle password input securely):
            Console.Write("Ange lösenord: ");
            existingUser.Password = Console.ReadLine();

            // Send the user information to the server for login with the separate method:
            SendLoginInformation(existingUser, clientSocket);
        }

        //
        /*7.B SEND LOGIN INFORMATION TO THE SERVER----------------------------------------------------------------------SEND LOGIN INFORMATION TO THE SERVER- 7.B */
        //
        public static void SendLoginInformation(User user, Socket clientSocket)
        {
            // Formulate a message/data structure to send to the server. Specifying that an account login request is being made:
            string loginRequest = $"LOGIN|{user.UserName}|{user.Password}";
            byte[] data = Encoding.UTF8.GetBytes(loginRequest);

            // Send the login request to the server:
            clientSocket.Send(data);

            // Handle the response from the server with the separate method:
            HandleLoginResponse(clientSocket);
        }

        //
        /*8.A + 10 RECIVE & PROCESS LOGIN RESPONSE FROM SERVER------------------------------------------------RECIVE & PROCESS LOGIN RESPONSE FROM SERVER  8.A+10 */
        //
        public static void HandleLoginResponse(Socket clientSocket)
        {
            // Read the server's response
            byte[] responseBytes = new byte[5000];
            int responseLength = clientSocket.Receive(responseBytes);
            string loginResponse = Encoding.UTF8.GetString(responseBytes, 0, responseLength);//13

            // Process the server's response & outcome depending on which response:
            switch (loginResponse)
            {
                case "LOGIN_SUCCESSFUL":
                    Console.WriteLine("Välkommen till Chattis!");
                    HandleConnectedClientsResponse(clientSocket);

                    break;

                case "LOGIN_FAILED":
                    Console.WriteLine("Login missslyckades, försök igen.");
                    MainMenu(clientSocket);
                    break;

                default:
                    Console.WriteLine("Unexpected response from the server in HandleLoginResponse.");
                    break;
            }
        }
        //
        /*14.A SPECIFIC RESPONSE FOR CONNECTED CLIENTS----------------------------------------------------------------------SPECIFIC RESPONSE FOR CONNECTED CLIENTS 14.A */
        //
        public static void HandleConnectedClientsResponse(Socket clientSocket)
        {
            // Read the server's response
            byte[] responseBytes = new byte[5000];
            int responseLength = clientSocket.Receive(responseBytes);
            string connectedClientsResponse = Encoding.UTF8.GetString(responseBytes, 0, responseLength);

            // Process the server's response & outcome depending on which response:
            if (connectedClientsResponse.StartsWith("CONNECTED_CLIENTS|"))
            {
                // Extract the connected clients list from the response
                string connectedClientsList = connectedClientsResponse.Substring("CONNECTED_CLIENTS|".Length);

                // Process the connected clients list
                Console.WriteLine("Inloggade just nu: " + connectedClientsList);
            }
            else
            {
                Console.WriteLine("Unexpected response from the server in HandleConnectedClientsResponse.");
            }
        }
    }
}
