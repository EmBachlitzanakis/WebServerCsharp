using System;

using Clifton.WebServer;

namespace ConsoleWebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] prefixes = { "http://localhost:8080/", "http://localhost:8000/" };
            Server.Start(prefixes);

            Console.WriteLine("Server started. Press any key to stop...");
            Console.ReadKey();

            Server.Stop();
        }
    }
}