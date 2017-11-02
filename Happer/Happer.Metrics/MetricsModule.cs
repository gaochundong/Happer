using System;
using System.IO;
using System.Linq;
using System.Net;
using Happer.Http;
using Metrics.Endpoints;
using Metrics.Reports;

namespace Happer.Metrics
{
    public class MetricsModule : Module
    {
        private struct ModuleConfig
        {
            public readonly string ModulePath;
            public readonly Action<Module> ModuleConfigAction;
            public readonly HapperMetricsEndpointHandler Handler;

            public ModuleConfig(Action<Module> moduleConfig, HapperMetricsEndpointHandler handler, string metricsPath)
            {
                this.ModuleConfigAction = moduleConfig;
                this.ModulePath = metricsPath;
                this.Handler = handler;
            }
        }

        private static ModuleConfig Config;

        private static readonly object[] noCacheHeaders = {
            new { Header = "Cache-Control", Value = "no-cache, no-store, must-revalidate" },
            new { Header = "Pragma", Value = "no-cache" },
            new { Header = "Expires", Value = "0" }
        };

        internal static void Configure(Action<Module> moduleConfig, MetricsEndpointReports reports, string metricsPath)
        {
            var handler = new HapperMetricsEndpointHandler(reports.Endpoints);
            MetricsModule.Config = new ModuleConfig(moduleConfig, handler, metricsPath);
        }

        public MetricsModule()
            : base(MetricsModule.Config.ModulePath ?? "/")
        {
            if (string.IsNullOrEmpty(MetricsModule.Config.ModulePath))
            {
                return;
            }

            MetricsModule.Config.ModuleConfigAction?.Invoke(this);

            Get("/", _ =>
            {
                if (!this.Request.Url.Path.EndsWith("/"))
                {
                    return Response.AsRedirect(this.Request.Url.ToString() + "/");
                }
                var gzip = AcceptsGzip();
                var response = Response.FromStream(FlotWebApp.GetAppStream(!gzip), "text/html");
                if (gzip)
                {
                    response.WithHeader("Content-Encoding", "gzip");
                }
                return response;
            });

            Get("/{path*}", p =>
            {
                var path = (string)p.path;
                var endpointResponse = MetricsModule.Config.Handler.Process(path, this.Request);
                return endpointResponse != null ? GetResponse(endpointResponse) : HttpStatusCode.NotFound;
            });
        }

        private static Response GetResponse(MetricsEndpointResponse endpointResponse)
        {
            var response = new Response();
            response.StatusCode = (HttpStatusCode)endpointResponse.StatusCode;
            response.ContentType = endpointResponse.ContentType;
            response.Contents = stream =>
            {
                using (var writer = new StreamWriter(stream, endpointResponse.Encoding))
                {
                    writer.Write(endpointResponse.Content);
                }
            };
            return response.WithHeaders(noCacheHeaders);
        }

        private bool AcceptsGzip()
        {
            return this.Request.Headers.AcceptEncoding.Any(e => e.Equals("gzip", StringComparison.OrdinalIgnoreCase));
        }
    }
}
