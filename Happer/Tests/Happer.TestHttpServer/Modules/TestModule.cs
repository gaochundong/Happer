using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Happer.Http;
using Logrila.Logging;

namespace Happer.TestHttpServer
{
    public class TestModule : Module
    {
        private static ILog _log = Logger.Get<TestModule>();

        public TestModule()
        {
            _log.WarnFormat("Initializing {0}.", typeof(TestModule).FullName);

            ShowThreadPoolSettings();

            Get("/thread", x =>
            {
                Print("Thread[{0}]", Thread.CurrentThread.GetDescription());

                return DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff");
            });
            Get("/sleep", x =>
            {
                Print("Sleep starts Thread[{0}].",
                    Thread.CurrentThread.GetDescription());

                Thread.Sleep(TimeSpan.FromSeconds(8));

                Print("--------> Sleep ends Thread[{0}].",
                    Thread.CurrentThread.GetDescription());

                return DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff");
            });
            Get("/delay", async x =>
            {
                Print("Delay starts Thread[{0}].",
                    Thread.CurrentThread.GetDescription());

                await Task.Delay(TimeSpan.FromSeconds(8));

                Print("--------> Delay ends Thread[{0}].",
                    Thread.CurrentThread.GetDescription());

                return DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff");
            });
            Get("/async", async _ => await Task.FromResult("Hello, World!"));

            Get("/exception", x =>
            {
                throw new InvalidOperationException("I want to throw an exception.");
            });

            Get("/while", x =>
            {
                while (true) { }
            });
            Get("/while/{count}", parameters =>
            {
                int count = (int)parameters.count;

                Print("While starts Thread[{0}].",
                    Thread.CurrentThread.GetDescription());

                while (count > 0) { count--; }

                Print("--------> While ends Thread[{0}].",
                    Thread.CurrentThread.GetDescription());

                return DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff");
            });

            Get("/big/{count}", parameters =>
            {
                return new string('x', (int)parameters.count);
            });
        }

        static void Print(string format, params object[] args)
        {
            Console.WriteLine(string.Format("{0}|{1}",
                DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff"),
                string.Format(format, args)));
        }

        static void ShowThreadPoolSettings()
        {
            int minWorkerThreads, maxWorkerThreads;
            int minCompletionPortThreads, maxCompletionPortThreads;
            int availableWorkerThreads, availableCompletionPortThreads;
            ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);
            ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);
            ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availableCompletionPortThreads);

            _log.DebugFormat("Current thread pool settings:");
            _log.DebugFormat("   Worker thread: " + minWorkerThreads + " / " + maxWorkerThreads);
            _log.DebugFormat("       IO thread: " + minCompletionPortThreads + " / " + maxCompletionPortThreads);
            _log.DebugFormat("Available thread: " + availableWorkerThreads + " / " + availableCompletionPortThreads);
        }
    }

    public static class ThreadExtensions
    {
        public static uint GetUnmanagedThreadId(this Thread context)
        {
            return GetCurrentThreadId();
        }

        public static string GetDescription(this Thread context)
        {
            return string.Format(CultureInfo.InvariantCulture, "ManagedThreadId[{0,6}], UnmanagedThreadId[{1,6}]",
              context.ManagedThreadId,
              context.GetUnmanagedThreadId());
        }

        [DllImport("kernel32.dll")]
        internal static extern uint GetCurrentThreadId();
    }
}
