using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Client
{
    public class Chattis
    {
        public static void ChattisMenu(Socket clientSocket)
        {
            Console.WriteLine("Chattis Meny:");
            Console.WriteLine("Visa denna meny igen, skriv: ^meny");
            Console.WriteLine("Visa inloggade användare, skriv: ^inloggade");
            Console.WriteLine("Logga ut, skriv: ^loggaut");
            Console.WriteLine("För att skriva ett privatmeddelande, skriv: ^privat/Användarnamn");

            while (true)
            {
                string? userInput = Console.ReadLine();

                {

                    switch (userInput)
                    {
                        case "^meny":
                            ChattisMenu(clientSocket); //Show Chattis Meny
                            break;
                        case "^inloggade":
                            LoginRegistration.SendGetConnectedClientsRequest(clientSocket);//Call the method that collects the logged in users
                            LoginRegistration.HandleConnectedClientsResponse(clientSocket);// Call the method that displays logged in users
                            break;

                        case "^loggaut":
                            Console.WriteLine("Logga ut function");//Add and call function for logout
                            break;

                        case "^privat/":
                            Console.WriteLine("Skicka privatmeddelande funktion");//Add and call function for lsending a private message
                            break;

                        default:
                            Console.WriteLine("Okänt kommando. Försök igen."); // Meddelande vid okänt kommando
                            break;
                    }
                }
            }
        }


    }
}