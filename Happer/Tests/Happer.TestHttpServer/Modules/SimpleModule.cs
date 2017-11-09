using System;
using System.IO;
using Happer.Http;
using Happer.Metrics;
using Logrila.Logging;

namespace Happer.TestHttpServer
{
    public class SimpleModule : Module
    {
        private static ILog _log = Logger.Get<SimpleModule>();

        public SimpleModule()
        {
            _log.WarnFormat("Initializing {0}.", typeof(SimpleModule).FullName);

            Get("/", x => { return "Hello, World!"; });
            Get("/ping", x => { return "pong"; });
            Get("/hello", x => { return "Hello, World!"; });
            Get("/text", x => { return "Text = " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"); });
            Get("/time", x => { return "Time = " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"); });

            Get("/print", _ => { Print("Hello, World!"); return "Hello, World!"; });
            Get("/user/{name}", parameters => { return (string)parameters.name; });
            Get("/redirect", _ => this.Response.AsRedirect("~/text"));

            Post("/post", _ => { return "POST = " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"); });
            Post("/post-something", _ =>
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

            // ---------------------- metrics ----------------------
            //this.MetricForRequestTimeAndResponseSize(typeof(TestModule).Name, "Get", "/");
            //this.MetricForRequestTimeAndResponseSize(typeof(TestModule).Name, "Get", "/ping");
            //this.MetricForRequestTimeAndResponseSize(typeof(TestModule).Name, "Get", "/hello");
            //this.MetricForRequestTimeAndResponseSize(typeof(TestModule).Name, "Get", "/text");
            //this.MetricForRequestTimeAndResponseSize(typeof(TestModule).Name, "Get", "/time");
            //this.MetricForRequestTimeAndResponseSize(typeof(TestModule).Name, "Get", "/user/{name}");
            this.MetricForAllRequests();
        }

        static void Print(string format, params object[] args)
        {
            Console.WriteLine(string.Format("{0}|{1}",
                DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff"),
                string.Format(format, args)));
        }
    }
}
