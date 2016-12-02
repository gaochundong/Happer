using System;
using System.IO;
using System.Threading;
using Happer.Http;

namespace Happer.TestHttpServer
{
    public class TestModule : Module
    {
        public TestModule()
        {
            Get("/", x => { return "Hello, World!"; });
            Get("/redirect", _ => this.Response.AsRedirect("~/text"));
            Get("/text", x => { return "Text = " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"); });
            Get("/time", x => { return "Time = " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"); });
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


            Get("/delay", x =>
            {
                Console.WriteLine("[{0}] Delay starts Thread[{1}].",
                    DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff"),
                    Thread.CurrentThread.ManagedThreadId);

                Thread.Sleep(TimeSpan.FromSeconds(3));

                Console.WriteLine("[{0}] Delay ends Thread[{1}].",
                    DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff"),
                    Thread.CurrentThread.ManagedThreadId);

                return DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff");
            });
        }
    }
}
