using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;


namespace Client
{
    public class Chattis
    {
        public static void ChattisMenu(Socket clientSocket, User user)
        {
            Console.WriteLine("Chattis Meny:");
            Console.WriteLine("Visa denna meny igen, skriv: ^meny");
            Console.WriteLine("Visa inloggade användare, skriv: ^inloggade");
            Console.WriteLine("Logga ut, skriv: ^loggout");
            Console.WriteLine("För att skriva ett privatmeddelande, skriv: ^privat/Användarnamn");

            while (true)
            {
                string? userInput = Console.ReadLine();

                {
                    switch (userInput)
                    {
                        case "^meny":
                            ChattisMenu(clientSocket, user); //Show Chattis Meny
                            break;

                        case "^inloggade":
                            LoginRegistration.SendConnectedClientsListRequest(clientSocket);//Call the method that collects the logged in users
                            LoginRegistration.HandleConnectedClientsResponse(clientSocket);// Call the method that displays logged in users
                            break;

                        case "^loggaut":
                            LoginRegistration.SendLogoutRequest(clientSocket, user);//Call the method that collects the logged in users
                            LoginRegistration.HandleLogoutResponse(clientSocket);// Call the method that displays logged in 
                            break;

                        case "^privat/":
                            Console.WriteLine("Skicka privatmeddelande funktion");//Add and call function for lsending a private message
                            break;

                        default:
                            Console.WriteLine("Public messages function"); // Meddelande vid okänt kommando
                            break;
                    }
                }
            }
        }
    }
}