using System;
using System.Threading;
using System.Threading.Tasks;

namespace Happer.Http.Routing
{
    public abstract class Route
    {
        protected Route(RouteDescription description)
        {
            this.Description = description;
        }

        protected Route(string name, string method, string path, Func<Context, bool> condition)
            : this(new RouteDescription(name, method, path, condition))
        {
        }

        public RouteDescription Description { get; private set; }

        public abstract Task<object> Invoke(DynamicDictionary parameters, CancellationToken cancellationToken);
    }

    public class Route<T> : Route
    {
        public Route(RouteDescription description, Func<object, CancellationToken, Task<T>> action)
            : base(description)
        {
            this.Action = action;
        }

        public Route(string name, string method, string path, Func<Context, bool> condition, Func<object, CancellationToken, Task<T>> action)
            : this(new RouteDescription(name, method, path, condition), action)
        {
        }

        public Route(string method, string path, Func<Context, bool> condition, Func<object, CancellationToken, Task<T>> action)
            : this(string.Empty, method, path, condition, action)
        {
        }

        public Func<object, CancellationToken, Task<T>> Action { get; set; }

        public override Task<object> Invoke(DynamicDictionary parameters, CancellationToken cancellationToken)
        {
            var task = this.Action.Invoke(parameters, cancellationToken);

            var tcs = new TaskCompletionSource<object>();

            task.ContinueWith(t => tcs.SetResult(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
            task.ContinueWith(t => tcs.SetException(t.Exception.InnerExceptions), TaskContinuationOptions.OnlyOnFaulted);
            task.ContinueWith(t => tcs.SetCanceled(), TaskContinuationOptions.OnlyOnCanceled);

            return tcs.Task;
        }
    }
}
