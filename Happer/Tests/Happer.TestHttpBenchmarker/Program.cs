using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Happer.TestHttpBenchmarker
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new HttpClient() { BaseAddress = new Uri(@"http://localhost:3202") };

            int concurrencyLevel = 4;
            int totalRequestCount = 10000;
            string documentPath = @"/plaintext";

            Console.WriteLine(string.Format("{0}Begin testing...{0}", Environment.NewLine));

            var tasks = new List<Task>(totalRequestCount);
            var watch = Stopwatch.StartNew();
            Parallel.For(0, totalRequestCount,
                new ParallelOptions() { MaxDegreeOfParallelism = concurrencyLevel },
                i =>
                {
                    tasks.Add(client.GetStringAsync(documentPath));
                });
            Task.WaitAll(tasks.ToArray());
            watch.Stop();

            Console.WriteLine(string.Format("          Server Uri: {0}",
                client.BaseAddress));
            Console.WriteLine(string.Format("       Document Path: {0}",
                documentPath));
            Console.WriteLine(string.Format("   Concurrency Level: {0}",
                concurrencyLevel));
            Console.WriteLine(string.Format("   Complete requests: {0}",
                totalRequestCount));
            Console.WriteLine(string.Format("Time taken for tests: {0:####0.###} seconds",
                watch.Elapsed.TotalMilliseconds / 1000));
            Console.WriteLine(string.Format("  Request per second: {0:####0.###} [#/sec] (mean)",
                totalRequestCount / (watch.Elapsed.TotalMilliseconds / 1000)));
            Console.WriteLine(string.Format("    Time per request: {0:####0.###} [ms] (mean)",
                watch.Elapsed.TotalMilliseconds / totalRequestCount));

            Console.WriteLine(string.Format("{0}End testing...{0}", Environment.NewLine));

            Console.ReadKey();
        }
    }
}
