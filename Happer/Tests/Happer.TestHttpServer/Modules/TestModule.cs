using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Happer.Http;
using Happer.Metrics;
using Logrila.Logging;

namespace Happer.TestHttpServer
{
    public class TestModule : Module
    {
        private static ILog _log = Logger.Get<TestModule>();

        public TestModule()
        {
            _log.DebugFormat("Initializing the test module.");

            // ---------------------- metrics ----------------------
            this.MetricForRequestTimeAndResponseSize(typeof(TestModule).Name, "Get", "/");
            this.MetricForRequestTimeAndResponseSize(typeof(TestModule).Name, "Get", "/ping");
            this.MetricForRequestTimeAndResponseSize(typeof(TestModule).Name, "Get", "/hello");
            this.MetricForRequestTimeAndResponseSize(typeof(TestModule).Name, "Get", "/text");
            this.MetricForRequestTimeAndResponseSize(typeof(TestModule).Name, "Get", "/time");
            this.MetricForRequestTimeAndResponseSize(typeof(TestModule).Name, "Get", "/redirect");
            this.MetricForRequestTimeAndResponseSize(typeof(TestModule).Name, "Get", "/user/{name}");
            // ---------------------- metrics ----------------------

            Get("/", x => { return "Hello, World!"; });
            Get("/ping", x => { return "pong"; });
            Get("/hello", x => { Print("Hello, World!"); return "Hello, World!"; });
            Get("/text", x => { return "Text = " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"); });
            Get("/time", x => { return "Time = " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"); });

            Get("/redirect", _ => this.Response.AsRedirect("~/text"));
            Get("/user/{name}", parameters => { return (string)parameters.name; });

            Post("/post", x => { return "POST = " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"); });
            Post("/post-something", x =>
            {
                var body = new StreamReader(this.Request.Body).ReadToEnd();
                return body;
            });

            Get("/json", x =>
            {
                var model = new TestModel() { This = new TestModel() };
                return this.Response.AsJson(model);
            });

            Get("/xml", x =>
            {
                var model = new TestModel() { This = new TestModel() };
                return this.Response.AsXml(model);
            });

            Get("/html", x =>
            {
                string html =
                    @"
                    <html>
                    <head>
                      <title>Hi there</title>
                    </head>
                    <body>
                        This is a page, a simple page.
                    </body>
                    </html>
                    ";
                return this.Response.AsHtml(html);
            });

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
            Get("/delay", x =>
            {
                Print("Delay starts Thread[{0}].",
                    Thread.CurrentThread.GetDescription());

                Task.Delay(TimeSpan.FromSeconds(1)).Wait();

                Print("--------> Delay ends Thread[{0}].",
                    Thread.CurrentThread.GetDescription());

                return DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff");
            });
            Get("/exception", x =>
            {
                throw new InvalidOperationException("I want to throw an exception.");
            });
            Get("/while", x =>
            {
                while (true) { }
            });
        }

        static void Print(string format, params object[] args)
        {
            Console.WriteLine(string.Format("{0}|{1}",
                DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff"),
                string.Format(format, args)));
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
