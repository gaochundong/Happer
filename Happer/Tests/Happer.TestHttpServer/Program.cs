using System;
using System.Diagnostics;
using Happer.Hosting.Self;
using Happer.Http;
using Happer.Metrics;
using Logrila.Logging.NLogIntegration;
using Metrics;

namespace Happer.TestHttpServer
{
    class Program
    {
        static Program()
        {
            NLogLogger.Use();
        }

        static void Main(string[] args)
        {
            var pipelines = new Pipelines();

            Metric.Config
                //.WithHttpEndpoint("http://localhost:3201/") // optional -- listen port
                .WithAllCounters()
                .WithReporting(r => r.WithConsoleReport(TimeSpan.FromSeconds(30)))
                .WithHapper(pipelines);

            var container = new ModuleContainer();
            container.AddModule(new TestModule());
            container.AddModule(new MetricsModule()); // optional -- enable /metrics/text

            var bootstrapper = new Bootstrapper();
            var engine = bootstrapper.BootWith(container, pipelines);

            string uri = "http://localhost:3202/";
            var host = new SelfHost(engine, new Uri(uri));
            host.Start();
            Console.WriteLine("Server is listening on [{0}].", uri);

            //AutoNavigateTo(uri);

            Console.ReadKey();
            Console.WriteLine("Stopped. Goodbye!");
            host.Stop();
            Console.ReadKey();
        }

        private static void AutoNavigateTo(string uri)
        {
            try
            {
                Process.Start(uri);
            }
            catch (Exception) { }
        }
    }
}
