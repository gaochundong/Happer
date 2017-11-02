using System;
using Happer.Http;
using Happer.Metrics;

namespace Metrics
{
    public static class HapperMetrics
    {
        public static MetricsConfig WithHapper(this MetricsConfig metricsConfig, IPipelines pipelines)
        {
            return metricsConfig.WithHapper(pipelines, happer => happer.WithHapperMetrics(m => m.WithAllMetrics()).WithMetricsModule());
        }

        public static MetricsConfig WithHapper(this MetricsConfig metricsConfig, IPipelines pipelines, Action<HapperMetricsConfig> happerConfig)
        {
            var currentConfig = metricsConfig.WithConfigExtension((ctx, hs) => new HapperMetricsConfig(ctx, hs, pipelines), () => HapperMetricsConfig.Disabled);
            happerConfig(currentConfig);
            return metricsConfig;
        }
    }
}
