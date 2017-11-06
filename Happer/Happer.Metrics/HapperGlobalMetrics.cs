using Happer.Http;
using Happer.Http.Routing;
using Metrics;
using Metrics.Utils;

namespace Happer.Metrics
{
    public class HapperGlobalMetrics
    {
        private const string RequestStartTimeKeyName = "RequestStartTimeKey";
        private const string RequestStartTimeKey = "__Metrics.RequestStartTime__";

        private static MetricsContext _globalMetricsContext = Metric.Context("Happer");
        public static MetricsContext GlobalMetricsContext { get { return _globalMetricsContext; } }

        private readonly MetricsContext _context;
        private readonly IPipelines _pipelines;

        public HapperGlobalMetrics(MetricsContext context, IPipelines pipelines)
        {
            _context = context;
            _pipelines = pipelines;
        }

        public HapperGlobalMetrics WithAllMetrics()
        {
            return this.WithRequestTimer()
                .WithErrorsMeter()
                .WithActiveRequestCounter()
                .WithPostPutAndPatchRequestSizeHistogram()
                .WithTimerForEachRequest();
        }

        public HapperGlobalMetrics WithRequestTimer(string metricName = "Requests")
        {
            var requestTimer = _context.Timer(metricName, Unit.Requests);

            _pipelines.BeforeRequest.AddItemToStartOfPipeline(ctx =>
            {
                ctx.Items[RequestStartTimeKey] = requestTimer.StartRecording();
                return null;
            });

            _pipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                object timer;
                if (ctx.Items.TryGetValue(RequestStartTimeKey, out timer))
                {
                    if (timer is long)
                    {
                        var startTime = (long)timer;
                        var endTime = requestTimer.EndRecording();
                        requestTimer.Record(endTime - startTime, TimeUnit.Nanoseconds);
                    }
                    ctx.Items.Remove(RequestStartTimeKey);
                }
            });

            return this;
        }

        public HapperGlobalMetrics WithErrorsMeter(string metricName = "Errors")
        {
            var errorMeter = _context.Meter(metricName, Unit.Errors, TimeUnit.Seconds);

            _pipelines.OnError.AddItemToStartOfPipeline((ctx, ex) =>
            {
                errorMeter.Mark();
                return null;
            });

            return this;
        }

        public HapperGlobalMetrics WithActiveRequestCounter(string metricName = "Active Requests")
        {
            var counter = _context.Counter(metricName, Unit.Custom("Active Requests"));

            _pipelines.BeforeRequest.AddItemToStartOfPipeline(ctx =>
            {
                counter.Increment();
                return null;
            });

            _pipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                counter.Decrement();
            });

            return this;
        }

        public HapperGlobalMetrics WithPostPutAndPatchRequestSizeHistogram(string metricName = "Post, Put & Patch Request Size")
        {
            var histogram = _context.Histogram(metricName, Unit.Bytes);

            _pipelines.BeforeRequest.AddItemToStartOfPipeline(ctx =>
            {
                var method = ctx.Request.Method.ToUpper();
                if (method == "POST" || method == "PUT" || method == "PATCH")
                {
                    histogram.Update(ctx.Request.Headers.ContentLength);
                }
                return null;
            });

            return this;
        }

        public HapperGlobalMetrics WithTimerForEachRequest()
        {
            _pipelines.BeforeRequest.AddItemToStartOfPipeline(ctx =>
            {
                ctx.Items[RequestStartTimeKeyName] = Clock.Default.Nanoseconds;
                return null;
            });

            _pipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                if (ctx.ResolvedRoute != null && !(ctx.ResolvedRoute is NotFoundRoute))
                {
                    var name = string.Format("{0} {1}", ctx.ResolvedRoute.Description.Method, ctx.ResolvedRoute.Description.Path);
                    var startTime = (long)ctx.Items[RequestStartTimeKeyName];
                    var elapsed = Clock.Default.Nanoseconds - startTime;
                    _context.Timer(name, Unit.Requests).Record(elapsed, TimeUnit.Nanoseconds);
                }
            });

            return this;
        }
    }
}
