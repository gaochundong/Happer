using System;
using Happer.Http;
using Metrics;
using Metrics.Reports;

namespace Happer.Metrics
{
    public class HapperMetricsConfig
    {
        public static readonly HapperMetricsConfig Disabled = new HapperMetricsConfig();

        private readonly MetricsContext _metricsContext;
        private readonly Func<HealthStatus> _healthStatus;
        private readonly IPipelines _pipelines;

        private readonly bool _isDiabled;

        public HapperMetricsConfig(MetricsContext metricsContext, Func<HealthStatus> healthStatus, IPipelines pipelines)
        {
            _metricsContext = metricsContext;
            _healthStatus = healthStatus;
            _pipelines = pipelines;
        }

        private HapperMetricsConfig()
        {
            _isDiabled = true;
        }

        public HapperMetricsConfig WithHapperMetrics(Action<HapperGlobalMetrics> config, string context = "Happer")
        {
            if (_isDiabled)
            {
                return this;
            }

            var globalMetrics = new HapperGlobalMetrics(_metricsContext.Context(context), _pipelines);
            config(globalMetrics);
            return this;
        }

        public HapperMetricsConfig WithMetricsModule(string metricsPath = "/metrics")
        {
            if (_isDiabled)
            {
                return this;
            }

            return WithMetricsModule(m => { }, c => { }, metricsPath);
        }

        public HapperMetricsConfig WithMetricsModule(Action<MetricsEndpointReports> config, string metricsPath = "/metrics")
        {
            if (_isDiabled)
            {
                return this;
            }

            return WithMetricsModule(m => { }, config, metricsPath);
        }

        public HapperMetricsConfig WithMetricsModule(Action<Module> moduleConfig, Action<MetricsEndpointReports> config, string metricsPath = "/metrics")
        {
            if (_isDiabled)
            {
                return this;
            }

            var reportsConfig = new MetricsEndpointReports(_metricsContext.DataProvider, _healthStatus);
            config(reportsConfig);
            MetricsModule.Configure(moduleConfig, reportsConfig, metricsPath);
            return this;
        }
    }
}
