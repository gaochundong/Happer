using System;
using Nancy.Hosting.Self;

namespace Happer.TestNancyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = @"http://localhost:3203/";
            using (var host = new NancyHost(
                new HostConfiguration()
                {
                    UrlReservations = new UrlReservations() { CreateAutomatically = true }
                },
                new Uri(uri)))
            {
                host.Start();

                Console.WriteLine("Nancy is listening - navigate to {0}.", uri);
                Console.WriteLine("Press enter to stop...");
                Console.ReadKey();
            }

            Console.WriteLine("Stopped. Good bye!");
        }
    }
}
