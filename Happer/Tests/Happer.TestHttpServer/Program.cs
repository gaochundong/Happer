﻿using System;
using Happer.Hosting.Self;
using Happer.Http;
using Happer.Metrics;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Metrics;

namespace Happer.TestHttpServer
{
    class Program
    {
        private static ILog _log;

        static Program()
        {
            NLogLogger.Use();
            _log = Logger.Get<Program>();
        }

        static void Main(string[] args)
        {
            var pipelines = new Pipelines();

            Metric.Config
                .WithAllCounters()
                //.WithReporting(r => r.WithConsoleReport(TimeSpan.FromSeconds(30))) // optional -- display to console
                //.WithReporting(r => r.WithCSVReports(@"C:\metrics\csv", TimeSpan.FromSeconds(30)))
                //.WithReporting(r => r.WithTextFileReport(@"C:\metrics\text\metrics.txt", TimeSpan.FromSeconds(30)))
                //.WithHttpEndpoint("http://localhost:3201/metrics/")                // optional -- listen http port
                .WithHapper(pipelines);

            var container = new ModuleContainer();

            // http://localhost:3202/
            // http://localhost:3202/ping
            // http://localhost:3202/hello
            // http://localhost:3202/text
            // http://localhost:3202/time
            container.AddModule(new TestModule());

            // http://localhost:3202/plaintext
            container.AddModule(new BenchmarkModule());

            // http://localhost:3202/metrics
            // http://localhost:3202/metrics/ping
            // http://localhost:3202/metrics/text
            // http://localhost:3202/metrics/health
            // http://localhost:3202/metrics/json
            // http://localhost:3202/metrics/v1/health
            // http://localhost:3202/metrics/v1/json
            // http://localhost:3202/metrics/v2/json
            container.AddModule(new MetricsModule());

            var bootstrapper = new Bootstrapper();
            var engine = bootstrapper.BootWith(container, pipelines);

            string uri = "http://localhost:3202/";
            var host = new SelfHost(engine, new Uri(uri));

            host.Start();
            _log.WarnFormat("Server is listening on [{0}].", uri);

            Console.ReadKey();

            host.Stop();
            _log.WarnFormat("Server is stopped.");

            Console.ReadKey();
        }
    }
}
