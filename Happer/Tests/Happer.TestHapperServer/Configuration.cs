using System;
using System.Threading;

namespace Happer.TestHapperServer
{
    public class Configuration
    {
        public static void ConfigureThreadPool()
        {
            var workerThreads = 20 * Environment.ProcessorCount;
            ThreadPool.SetMinThreads(workerThreads, workerThreads);
            ThreadPool.SetMaxThreads(workerThreads, workerThreads);
        }

        public static void ShowThreadPoolSettings()
        {
            int minWorkerThreads, maxWorkerThreads;
            int minCompletionPortThreads, maxCompletionPortThreads;
            int availableWorkerThreads, availableCompletionPortThreads;
            ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);
            ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);
            ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availableCompletionPortThreads);

            Console.WriteLine("Current thread pool settings:");
            Console.WriteLine("   Worker thread: " + minWorkerThreads + " / " + maxWorkerThreads);
            Console.WriteLine("       IO thread: " + minCompletionPortThreads + " / " + maxCompletionPortThreads);
            Console.WriteLine("Available thread: " + availableWorkerThreads + " / " + availableCompletionPortThreads);
        }
    }
}
