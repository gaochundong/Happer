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
                Console.WriteLine("[{1}] Delay starts Thread[{0}].",
                    Thread.CurrentThread.ManagedThreadId,
                    DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff"));
                Thread.Sleep(TimeSpan.FromSeconds(3));
                Console.WriteLine("[{1}] Delay ends Thread[{0}].",
                    Thread.CurrentThread.ManagedThreadId,
                    DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff"));
                return DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff");
            });
        }

        private static string GetEmbeddedResourceData(string fileName)
        {
            var assem = System.Reflection.Assembly.GetExecutingAssembly();
            string filePath = assem.FullName.Split(',')[0] + ".Content." + fileName;
            using (var stream = assem.GetManifestResourceStream(filePath))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
