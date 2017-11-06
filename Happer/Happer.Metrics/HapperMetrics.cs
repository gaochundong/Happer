using System;
using Happer.Http;
using Metrics;

namespace Happer.Metrics
{
    public static class HapperMetrics
    {
        public static IPipelines WithMetrics(this IPipelines pipelines)
        {
            Metric.Config.WithAllCounters().WithHapper(pipelines);
            return pipelines;
        }

        public static MetricsConfig WithHapper(this MetricsConfig metricsConfig, IPipelines pipelines)
        {
            return metricsConfig
                .WithHapper(pipelines, hpc =>
                    hpc.WithHapperMetrics(m => m.WithAllMetrics()).WithMetricsModule());
        }

        public static MetricsConfig WithHapper(this MetricsConfig metricsConfig, IPipelines pipelines, Action<HapperMetricsConfig> configHapper)
        {
            var happerConfig = metricsConfig
                .WithConfigExtension(
                    (ctx, hs) => new HapperMetricsConfig(ctx, hs, pipelines),
                    () => HapperMetricsConfig.Disabled);
            configHapper(happerConfig);
            return metricsConfig;
        }
    }
}
