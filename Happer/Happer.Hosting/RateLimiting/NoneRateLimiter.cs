using System;
using System.Threading;
using System.Threading.Tasks;

namespace Happer.Hosting
{
    public class NoneRateLimiter : IRateLimiter
    {
        public static readonly NoneRateLimiter None = new NoneRateLimiter();

        public NoneRateLimiter()
        {
        }

        public int CurrentCount { get { return 1; } }

        #region Wait

        public void Wait()
        {
        }

        public void Wait(CancellationToken cancellationToken)
        {
        }

        public bool Wait(TimeSpan timeout)
        {
            return true;
        }

        public bool Wait(int millisecondsTimeout)
        {
            return true;
        }

        public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return true;
        }

        public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            return true;
        }

        public async Task WaitAsync()
        {
            await Task.CompletedTask;
        }

        public async Task WaitAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        public async Task<bool> WaitAsync(TimeSpan timeout)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> WaitAsync(int millisecondsTimeout)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            return await Task.FromResult(true);
        }

        #endregion

        #region Release

        public int Release()
        {
            return 1;
        }

        public int Release(int releaseCount)
        {
            return 1;
        }

        #endregion
    }
}
