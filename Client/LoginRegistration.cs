using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;

namespace Client
//
/*4.A USER CLASS---------------------------------------------------------------------------------------------------------------------------USER CLASS 4.A */
//
{
    public class User
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }

    public class LoginRegistration
    {

        //
        /*4.B MAIN MENU-----------------------------------------------------------------------------------------------------------------------------MAIN MENU 4.B */
        //
        public static void MainMenu(Socket clientSocket)
        {
            Console.WriteLine("Huvud Menu. Vad vill du göra?:");
            Console.WriteLine("1. Skapa konto");
            Console.WriteLine("2. Logga in");
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
            // Send the account creation request to the socket:
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
            HandleLoginResponse(clientSocket, user);
        }

        //
        /*8.A + 10 RECIVE & PROCESS LOGIN RESPONSE FROM SERVER------------------------------------------------RECIVE & PROCESS LOGIN RESPONSE FROM SERVER  8.A+10 */



        //
        public static void HandleLoginResponse(Socket clientSocket, User user)
        {
            // Read the server's response
            byte[] loginData = new byte[5000];
            int responseLength = clientSocket.Receive(loginData);
            string loginResponse = Encoding.UTF8.GetString(loginData, 0, responseLength);//13
            Console.WriteLine(loginResponse);
            // Process the server's response & outcome depending on which response:
            switch (loginResponse)
            {
                case "LOGIN_SUCCESSFUL":

                    // Broadcast the "username logged in" message to all connected clients
                    //Call function to show the 30 last messages connected to that user
                    Console.WriteLine("Välkommen till Chattis!");
                    SendConnectedClientsListRequest(clientSocket);//Call the method that collects the logged in users// 15.D
                    HandleConnectedClientsResponse(clientSocket);//Call the method that displays the logged in users
                    Chattis.ChattisMenu(clientSocket, user); //15.B
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


        //16.B ADDED HANDLE LOGOUT RESPONSE
        public static void HandleLogoutResponse(Socket clientSocket)
        {
            // Read the server's response
            byte[] logoutData = new byte[5000];
            int responseLength = clientSocket.Receive(logoutData);
            string logoutResponse = Encoding.UTF8.GetString(logoutData, 0, responseLength);//13

            // Process the server's response & outcome depending on which response:
            switch (logoutResponse)
            {
                case "LOGOUT_SUCCESSFUL":
                    Console.WriteLine("Du är nu utloggad");
                    MainMenu(clientSocket);
                    break;


                default:
                    Console.WriteLine("Unexpected response from the server in HandleLogOutResponse.");
                    break;
            }
        }
        //
        /*14.A SPECIFIC RESPONSE FOR CONNECTED CLIENTS----------------------------------------------------------------------SPECIFIC RESPONSE FOR CONNECTED CLIENTS 14.A */
        //
        public static void HandleConnectedClientsResponse(Socket clientSocket)
        {
            // Read the server's response
            byte[] connectedClientsData = new byte[5000];
            int responseLength = clientSocket.Receive(connectedClientsData);
            string connectedClientsResponse = Encoding.UTF8.GetString(connectedClientsData, 0, responseLength);

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

        //
        /*15.C SPECIFIC REQUEST FOR CONNECTED CLIENTS----------------------------------------------------------------------SPECIFIC REQUEST FOR CONNECTED CLIENTS 15.C */
        //
        public static void SendConnectedClientsListRequest(Socket clientSocket) //FUNKTION SOM BER SERVERN SKICKA LISTAN
        {

            string getConnectedClientsList = "GET_CONNECTED_CLIENTS";
            byte[] connectedClientsData = Encoding.UTF8.GetBytes(getConnectedClientsList);


            clientSocket.Send(connectedClientsData);
        }

        //16.ADDED LOGOUT REQUEST 
        public static void SendLogoutRequest(Socket clientSocket, User user)
        {
            // Create a new User instance specifically for current users:
            // User currentUser = new User();
            string getLogoutRequest = $"LOGOUT|{user.UserName}";
            byte[] logoutData = Encoding.UTF8.GetBytes(getLogoutRequest);


            clientSocket.Send(logoutData);

            HandleLogoutResponse(clientSocket);
        }

    }
}

// SendConnectedClientsListRequest(clientSocket); 
//  HandleConnectedClientsResponse(clientSocket);

//  foreach client in ConnectedClientsList{
//      Console.WriteLine $"existingUser +(har loggat in) 
//   }