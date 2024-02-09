using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace Client
{
    public class LoginFunctions
    {
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
            // Console.WriteLine(loginResponse);

            switch (loginResponse)
            {

                case "LOGIN_SUCCESSFUL":
                    MessagesFunctions.HandleHistoryMessageResponse(clientSocket);

                    Console.WriteLine("----------------------------");
                    Console.WriteLine("VALKOMMEN TILL CHATTIS!");

                    Menu.SendConnectedClientsListRequest(clientSocket);
                    Menu.HandleConnectedClientsResponse(clientSocket);

                    Menu.ChattisMenu(clientSocket, user);

                    break;

                case "LOGIN_FAILED":
                    Console.WriteLine("Login missslyckades, forsok igen.");
                    Menu.MainMenu(clientSocket);
                    break;

                default:
                    Console.WriteLine("Unexpected response from the server in HandleLoginResponse.");
                    Menu.MainMenu(clientSocket);
                    break;
            }
        }
        public static void SendLogoutRequest(Socket clientSocket, User user)
        {
            string getLogoutRequest = $"LOGOUT|{user.UserName}";
            byte[] logoutData = Encoding.UTF8.GetBytes(getLogoutRequest);
            clientSocket.Send(logoutData);

            HandleLogoutResponse(clientSocket);
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
                    Menu.MainMenu(clientSocket);
                    break;

                default:
                    Console.WriteLine("Unexpected response from the server in HandleLogOutResponse.");
                    break;
            }
        }
    }
}

