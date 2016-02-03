using System;
using Happer.Logging.NLogIntegration;

namespace Happer.TestHttpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            NLogLogger.Use();

            TestWithClientWebSocket.Connect().Wait();

            Console.WriteLine("Waiting...");
            Console.ReadKey();
        }
    }
}
