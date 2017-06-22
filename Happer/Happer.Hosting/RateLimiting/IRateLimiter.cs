using System;
using System.Threading;
using System.Threading.Tasks;

namespace Happer.Hosting
{
    public interface IRateLimiter
    {
        int CurrentCount { get; }

        void Wait();
        void Wait(CancellationToken cancellationToken);
        bool Wait(TimeSpan timeout);
        bool Wait(TimeSpan timeout, CancellationToken cancellationToken);
        bool Wait(int millisecondsTimeout);
        bool Wait(int millisecondsTimeout, CancellationToken cancellationToken);

        Task WaitAsync();
        Task WaitAsync(CancellationToken cancellationToken);

        Task<bool> WaitAsync(TimeSpan timeout);
        Task<bool> WaitAsync(int millisecondsTimeout);
        Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken);
        Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken);

        int Release();
    }
}
