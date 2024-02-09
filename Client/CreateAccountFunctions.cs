using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace Client
{
    public class CreateAccountFunctions
    {
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
                    Menu.MainMenu(clientSocket);
                    break;
                case "ACCOUNT_CREATION_FAILED":
                    Console.WriteLine("Anvandarnamnet ar upptaget, valj ett annat anvandarnam!");
                    Menu.MainMenu(clientSocket);
                    break;
                default:
                    Console.WriteLine("Unexpected response from the server in HandleCreateAccountResponse.");
                    Menu.MainMenu(clientSocket);
                    break;
            }
        }
    }
}