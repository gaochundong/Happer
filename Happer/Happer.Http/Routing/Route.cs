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

        public override async Task<object> Invoke(DynamicDictionary parameters, CancellationToken cancellationToken)
        {
            return await this.Action.Invoke(parameters, cancellationToken).ConfigureAwait(false);
        }
    }
}
