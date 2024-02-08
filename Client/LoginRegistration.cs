using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace Client
{
    public class User
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }

    public class LoginRegistration
    {
        public static void MainMenu(Socket clientSocket)
        {
            Console.WriteLine("Huvud Menu. Vad vill du göra?:");
            Console.WriteLine("1. Skapa konto");
            Console.WriteLine("2. Logga in");
            Console.WriteLine("----------------------------");

            string? userInput = Console.ReadLine();

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

        public static void CreateAccount(Socket clientSocket)
        {
            Console.WriteLine("Skapa nytt konto:");
            User newUser = new User();

            Console.Write("Valj anvandarnamn: ");
            newUser.UserName = Console.ReadLine();
            Console.Write("Valj losenord: ");
            newUser.Password = Console.ReadLine();

            SendAccountInformation(newUser, clientSocket);
        }

        public static void SendAccountInformation(User user, Socket clientSocket)
        {
            string accountCreationRequest = $"CREATE_ACCOUNT|{user.UserName}|{user.Password}";
            byte[] data = Encoding.UTF8.GetBytes(accountCreationRequest);
            clientSocket.Send(data);

            HandleCreateAccountResponse(clientSocket);
        }

        public static void HandleCreateAccountResponse(Socket clientSocket)
        {
            byte[] responseBytes = new byte[5000];
            int responseLength = clientSocket.Receive(responseBytes);
            string createAccountResponse = Encoding.UTF8.
            GetString(responseBytes, 0, responseLength);

            switch (createAccountResponse)
            {
                case "ACCOUNT_CREATED":
                    Console.WriteLine("Kontot ar nu skapat!");
                    MainMenu(clientSocket);
                    break;
                case "ACCOUNT_CREATION_FAILED":
                    Console.WriteLine("Anvandarnamnet ar upptaget, valj ett annat anvandarnam!");
                    MainMenu(clientSocket);
                    break;
                default:
                    Console.WriteLine("Unexpected response from the server in HandleCreateAccountResponse.");
                    MainMenu(clientSocket);
                    break;
            }
        }

        public static void LogIn(Socket clientSocket)
        {
            Console.WriteLine("Logga in:");
            User existingUser = new User();

            Console.Write("Ange anvandarnamn: ");
            existingUser.UserName = Console.ReadLine();
            Console.Write("Ange losenord: ");
            existingUser.Password = Console.ReadLine();

            SendLoginInformation(existingUser, clientSocket);
        }

        public static void SendLoginInformation(User user, Socket clientSocket)
        {
            string loginRequest = $"LOGIN|{user.UserName}|{user.Password}";
            byte[] data = Encoding.UTF8.GetBytes(loginRequest);

            clientSocket.Send(data);
            HandleLoginResponse(clientSocket, user);
        }
        public static void HandleLoginResponse(Socket clientSocket, User user)
        {
            byte[] loginData = new byte[5000];
            int responseLength = clientSocket.Receive(loginData);
            string loginResponse = Encoding.UTF8.GetString(loginData, 0, responseLength);
            Console.WriteLine(loginResponse);

            switch (loginResponse)
            {
                case "LOGIN_SUCCESSFUL":
                    HandleHistoryMessageResponse(clientSocket);

                    Console.WriteLine("----------------------------");
                    Console.WriteLine("VALKOMMEN TILL CHATTIS!");

                    SendConnectedClientsListRequest(clientSocket);
                    HandleConnectedClientsResponse(clientSocket);

                    Chattis.ChattisMenu(clientSocket, user);
                    break;

                case "LOGIN_FAILED":
                    Console.WriteLine("Login missslyckades, forsok igen.");
                    MainMenu(clientSocket);
                    break;

                default:
                    Console.WriteLine("Unexpected response from the server in HandleLoginResponse.");
                    MainMenu(clientSocket);
                    break;
            }
        }

        public static void HandleLogoutResponse(Socket clientSocket)
        {
            byte[] logoutData = new byte[5000];
            int responseLength = clientSocket.Receive(logoutData);
            string logoutResponse = Encoding.UTF8.GetString(logoutData, 0, responseLength);

            switch (logoutResponse)
            {
                case "LOGOUT_SUCCESSFUL":
                    Console.WriteLine("Du ar nu utloggad");
                    MainMenu(clientSocket);
                    break;

                default:
                    Console.WriteLine("Unexpected response from the server in HandleLogOutResponse.");
                    break;
            }
        }

        public static void HandleConnectedClientsResponse(Socket clientSocket)
        {
            byte[] connectedClientsData = new byte[5000];
            int responseLength = clientSocket.Receive(connectedClientsData);
            string connectedClientsResponse = Encoding.UTF8.GetString(connectedClientsData, 0, responseLength);

            if (connectedClientsResponse.StartsWith("CONNECTED_CLIENTS|"))
            {
                string connectedClientsList = connectedClientsResponse.Substring("CONNECTED_CLIENTS|".Length);
                Console.WriteLine("Inloggade just nu: " + connectedClientsList);
            }
            else
            {
                Console.WriteLine("Unexpected response from the server in HandleConnectedClientsResponse: " + connectedClientsResponse);
            }
        }

        public static void SendConnectedClientsListRequest(Socket clientSocket)
        {
            string getConnectedClientsList = "GET_CONNECTED_CLIENTS";
            byte[] connectedClientsData = Encoding.UTF8.GetBytes(getConnectedClientsList);
            clientSocket.Send(connectedClientsData);
        }

        public static void SendLogoutRequest(Socket clientSocket, User user)
        {
            string getLogoutRequest = $"LOGOUT|{user.UserName}";
            byte[] logoutData = Encoding.UTF8.GetBytes(getLogoutRequest);
            clientSocket.Send(logoutData);

            HandleLogoutResponse(clientSocket);
        }

        public static void HandleHistoryMessageResponse(Socket clientSocket)
        {
            byte[] historyData = new byte[5000];
            int responseLength = clientSocket.Receive(historyData);
            string historyResponse = Encoding.UTF8.GetString(historyData, 0, responseLength);

            string[] parts = historyResponse.Split('|');

            if (parts.Length >= 2)
            {
                string messageCase = parts[0];
                string messageHistory = parts[1];


                string[] messageList = messageHistory.Split('/');

                foreach (string eachMessage in messageList)
                {
                    string[] eachMessageList = eachMessage.Split(';');
                    if (eachMessageList.Length >= 3)
                    {
                        string historyFromUser = eachMessageList[0];
                        string historyToUser = eachMessageList[1];
                        string historyMessage = eachMessageList[2];

                        Console.WriteLine(historyFromUser + ": " + historyMessage);
                    }
                    else
                    {
                        Console.WriteLine("No history available");
                    }
                }
            }
        }
    }
}

