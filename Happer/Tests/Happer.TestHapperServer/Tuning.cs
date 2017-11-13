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
        public static void TuneAll(SelfHost host)
        {
            ConfigureThreadPool();
            ConfigureIgnoreWriteExceptions(host);
            ConfigureRequestQueueLength(host);
        }

        public static void ConfigureThreadPool()
        {
            // The CLR ThreadPool injects new threads at a rate of about 2 per second.
            // To improve CPU utilization, increase the number of threads that the .NET thread pool expands by when
            // a burst of requests come in. We could do this by editing machine.config/system.web/processModel/minWorkerThreads,
            // but that seems too global a change, so we do it in code for just our AppPool. More info:
            // http://support.microsoft.com/kb/821268
            // https://blogs.msdn.microsoft.com/tmarq/2007/07/20/asp-net-thread-usage-on-iis-7-5-iis-7-0-and-iis-6-0/
            // https://blogs.msdn.microsoft.com/perfworld/2010/01/13/how-can-i-improve-the-performance-of-asp-net-by-adjusting-the-clr-thread-throttling-properties/
            // https://msdn.microsoft.com/en-us/library/system.threading.threadpool.setminthreads(v=vs.110).aspx

            int newMinWorkerThreadsPerLogicalProcessor = 3;
            int newMinWorkerThreads = newMinWorkerThreadsPerLogicalProcessor * Environment.ProcessorCount + 32;

            int minWorkerThreads, minCompletionPortThreads;
            ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);

            if (ThreadPool.SetMinThreads(newMinWorkerThreads, minCompletionPortThreads))
            {
                ShowThreadPoolSettings();
            }
        }

        public static void ShowThreadPoolSettings()
        {
            int minWorkerThreads, maxWorkerThreads;
            int minCompletionPortThreads, maxCompletionPortThreads;

            ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);
            ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);

            Console.WriteLine("Current thread pool settings:");
            Console.WriteLine("   Worker threads: " + minWorkerThreads + " / " + maxWorkerThreads);
            Console.WriteLine("       IO threads: " + minCompletionPortThreads + " / " + maxCompletionPortThreads);
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

        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364639(v=vs.85).aspx
        internal enum HTTP_SERVER_PROPERTY
        {
            HttpServerAuthenticationProperty,
            HttpServerLoggingProperty,
            HttpServerQosProperty,
            HttpServerTimeoutsProperty,
            HttpServerQueueLengthProperty, // The connections property limits the number of requests in the request queue. This is a ULONG.
            HttpServerStateProperty,
            HttpServer503VerbosityProperty,
            HttpServerBindingProperty,
            HttpServerExtendedAuthenticationProperty,
            HttpServerListenEndpointProperty,
            HttpServerChannelBindProperty,
            HttpServerProtectionLevelProperty,
        }

        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364501(v=vs.85).aspx
        [DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern uint HttpSetRequestQueueProperty(
            CriticalHandle requestQueueHandle,   // _In_       HANDLE               Handle,
            HTTP_SERVER_PROPERTY serverProperty, // _In_       HTTP_SERVER_PROPERTY Property,
            ref uint pPropertyInfo,              // _In_       PVOID                pPropertyInformation,
            uint propertyInfoLength,             // _In_       ULONG                PropertyInformationLength,
            uint reserved,                       // _Reserved_ ULONG                Reserved,
            IntPtr pReserved);                   // _Reserved_ PVOID                pReserved
    }
}
