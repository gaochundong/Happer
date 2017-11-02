using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics
{
    public class HapperMetrics
    {
        //public static MetricsConfig WithNancy(this MetricsConfig config, IPipelines nancyPipelines)
        //{
        //    return config.WithNancy(nancyPipelines, nancy => nancy
        //        .WithNancyMetrics(m => m.WithAllMetrics())
        //        .WithMetricsModule()
        //    );
        //}

        //public static MetricsConfig WithNancy(this MetricsConfig config, IPipelines nancyPipelines,
        //    Action<NancyMetricsConfig> nancyConfig)
        //{
        //    var currentConfig = config.WithConfigExtension((ctx, hs) => new NancyMetricsConfig(ctx, hs, nancyPipelines), () => NancyMetricsConfig.Disabled);
        //    nancyConfig(currentConfig);
        //    return config;
        //}
    }
}
