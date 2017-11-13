using System;
using Happer.Hosting.Self;

namespace Happer.TestHapperServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new ModuleContainer();

            // http://localhost:3202/plaintext
            container.AddModule(new PlainModule());

            var bootstrapper = new Bootstrapper();
            var engine = bootstrapper.BootWith(container);

            var uri = @"http://localhost:3202/";
            var host = new SelfHost(engine, new Uri(uri));

            host.Start();
            Console.WriteLine("Happer is listening - navigate to {0}.", uri);
            Console.WriteLine("Press enter to stop...");
            Console.ReadKey();

            host.Stop();
            Console.WriteLine("Server is stopped.");
        }
    }
}
