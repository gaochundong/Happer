using System;
using Happer.Http;
using Happer.Metrics;

namespace Metrics
{
    public static class HapperMetrics
    {
        public static MetricsConfig WithHapper(this MetricsConfig metricsConfig, IPipelines pipelines)
        {
            return metricsConfig
                .WithHapper(pipelines, hpc => hpc.WithHapperMetrics(m => m.WithAllMetrics()).WithMetricsModule());
        }

        public static MetricsConfig WithHapper(this MetricsConfig metricsConfig, IPipelines pipelines, Action<HapperMetricsConfig> configHapper)
        {
            var currentConfig = metricsConfig
                .WithConfigExtension(
                    (ctx, hs) => new HapperMetricsConfig(ctx, hs, pipelines), 
                    () => HapperMetricsConfig.Disabled);
            configHapper(currentConfig);
            return metricsConfig;
        }
    }
}
