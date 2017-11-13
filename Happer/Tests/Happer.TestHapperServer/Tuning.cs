using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Happer.Hosting.Self;

namespace Happer.TestHapperServer
{
    /// <summary>
    /// Try to do something to optimize the performance.
    /// </summary>
    internal static class Tuning
    {
        public static void ConfigureThreadPool()
        {
            // To improve CPU utilization, increase the number of threads that the .NET thread pool expands by when
            // a burst of requests come in. We could do this by editing machine.config/system.web/processModel/minWorkerThreads,
            // but that seems too global a change, so we do it in code for just our AppPool. More info:
            // http://support.microsoft.com/kb/821268
            // http://blogs.msdn.com/b/tmarq/archive/2007/07/21/asp-net-thread-usage-on-iis-7-0-and-6-0.aspx
            // http://blogs.msdn.com/b/perfworld/archive/2010/01/13/how-can-i-improve-the-performance-of-asp-net-by-adjusting-the-clr-thread-throttling-properties.aspx

            int newMinWorkerThreadsPerLogicalProcessor = 8;
            int newMinWorkerThreads = newMinWorkerThreadsPerLogicalProcessor * Environment.ProcessorCount;

            int minWorkerThreads, minCompletionPortThreads;
            ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);
            ThreadPool.SetMinThreads(newMinWorkerThreads, minCompletionPortThreads);
        }

        public static void ShowThreadPoolSettings()
        {
            int minWorkerThreads, maxWorkerThreads;
            int minCompletionPortThreads, maxCompletionPortThreads;
            int availableWorkerThreads, availableCompletionPortThreads;
            ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);
            ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);
            ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availableCompletionPortThreads);

            Console.WriteLine("Current thread pool settings:");
            Console.WriteLine("   Worker thread: " + minWorkerThreads + " / " + maxWorkerThreads);
            Console.WriteLine("       IO thread: " + minCompletionPortThreads + " / " + maxCompletionPortThreads);
            Console.WriteLine("Available thread: " + availableWorkerThreads + " / " + availableCompletionPortThreads);
        }

        public static void ConfigureIgnoreWriteExceptions(SelfHost host)
        {
            var hostType = typeof(SelfHost);
            var hostFields = hostType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).ToList();
            var listenerField = hostFields.First(p => p.Name == "_listener");
            var listener = (HttpListener)listenerField.GetValue(host);

            // This doesn't seem to ignore all write exceptions, so in WriteResponse(), we still have a catch block.
            listener.IgnoreWriteExceptions = true;
        }

        public static void ConfigureRequestQueueLength(SelfHost host)
        {
            var hostType = typeof(SelfHost);
            var hostFields = hostType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).ToList();
            var listenerField = hostFields.First(p => p.Name == "_listener");
            var listener = (HttpListener)listenerField.GetValue(host);

            // Increase the HTTP.SYS backlog queue from the default of 1000 to 65535.
            // To verify that this works, run 'netsh http show servicestate'.
            // Check "Max requests" property for your process.
            // https://support.microsoft.com/en-us/help/820129/http-sys-registry-settings-for-windows
            // http://stackoverflow.com/questions/15417062/changing-http-sys-kernel-queue-limit-when-using-net-httplistener

            HttpSys.SetRequestQueueLength(listener, 65535);
        }
    }

    internal static class HttpSys
    {
        public static void SetRequestQueueLength(HttpListener listener, uint len)
        {
            var listenerType = typeof(HttpListener);
            var requestQueueHandleProperty =
                listenerType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                    .First(p => p.Name == "RequestQueueHandle");
            var requestQueueHandle = (CriticalHandle)requestQueueHandleProperty.GetValue(listener);

            var result = HttpSetRequestQueueProperty(requestQueueHandle,
                HTTP_SERVER_PROPERTY.HttpServerQueueLengthProperty,
                ref len, (uint)Marshal.SizeOf(len), 0, IntPtr.Zero);

            if (result != 0)
            {
                throw new HttpListenerException((int)result);
            }
        }

        internal enum HTTP_SERVER_PROPERTY
        {
            HttpServerAuthenticationProperty,
            HttpServerLoggingProperty,
            HttpServerQosProperty,
            HttpServerTimeoutsProperty,
            HttpServerQueueLengthProperty,
            HttpServerStateProperty,
            HttpServer503VerbosityProperty,
            HttpServerBindingProperty,
            HttpServerExtendedAuthenticationProperty,
            HttpServerListenEndpointProperty,
            HttpServerChannelBindProperty,
            HttpServerProtectionLevelProperty,
        }

        [DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern uint HttpSetRequestQueueProperty(
            CriticalHandle requestQueueHandle,
            HTTP_SERVER_PROPERTY serverProperty,
            ref uint pPropertyInfo,
            uint propertyInfoLength,
            uint reserved,
            IntPtr pReserved);
    }
}
