using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Happer.Http.Routing;

namespace Happer.Http
{
    public abstract class Module
    {
        private readonly List<Route> _routes = new List<Route>();

        protected Module()
            : this(string.Empty)
        {
        }

        protected Module(string modulePath)
        {
            this.ModulePath = modulePath;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerable<Route> Routes
        {
            get { return _routes.AsReadOnly(); }
        }

        public string ModulePath { get; protected set; }

        public string GetModuleName()
        {
            var typeName = this.GetType().Name;

            var offset = typeName.LastIndexOf("Module", StringComparison.Ordinal);

            if (offset <= 0)
            {
                return typeName;
            }

            return typeName.Substring(0, offset);
        }

        public Context Context { get; set; }

        public Request Request
        {
            get { return this.Context.Request; }
            set { this.Context.Request = value; }
        }

        public ResponseFormatter Response { get; set; }

        protected void AddRoute<T>(string method, string path, Func<dynamic, CancellationToken, Task<T>> action, Func<Context, bool> condition, string name)
        {
            _routes.Add(new Route<T>(name == null ? string.Empty : name, method, this.GetFullPath(path), condition, action));
        }

        private string GetFullPath(string path)
        {
            var relativePath = (path ?? string.Empty).Trim('/');
            var parentPath = (this.ModulePath ?? string.Empty).Trim('/');

            if (string.IsNullOrEmpty(parentPath))
            {
                return string.Concat("/", relativePath);
            }

            if (string.IsNullOrEmpty(relativePath))
            {
                return string.Concat("/", parentPath);
            }

            return string.Concat("/", parentPath, "/", relativePath);
        }

        #region HTTP Methods

        public virtual void Delete(string path, Func<dynamic, object> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Delete<object>(path, action, condition, name);
        }

        public virtual void Delete<T>(string path, Func<dynamic, T> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Delete(path, args => Task.FromResult(action((DynamicDictionary)args)), condition, name);
        }

        public virtual void Delete(string path, Func<dynamic, Task<object>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Delete<object>(path, action, condition, name);
        }

        public virtual void Delete<T>(string path, Func<dynamic, Task<T>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Delete(path, (args, ct) => action((DynamicDictionary)args), condition, name);
        }

        public virtual void Delete(string path, Func<dynamic, CancellationToken, Task<object>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Delete<object>(path, action, condition, name);
        }

        public virtual void Delete<T>(string path, Func<dynamic, CancellationToken, Task<T>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.AddRoute("DELETE", path, action, condition, name);
        }

        public virtual void Get(string path, Func<dynamic, object> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Get<object>(path, action, condition, name);
        }

        public virtual void Get<T>(string path, Func<dynamic, T> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Get(path, args => Task.FromResult(action((DynamicDictionary)args)), condition, name);
        }

        public virtual void Get(string path, Func<dynamic, Task<object>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Get<object>(path, action, condition, name);
        }

        public virtual void Get<T>(string path, Func<dynamic, Task<T>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Get(path, (args, ct) => action((DynamicDictionary)args), condition, name);
        }

        public virtual void Get(string path, Func<dynamic, CancellationToken, Task<object>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Get<object>(path, action, condition, name);
        }

        public virtual void Get<T>(string path, Func<dynamic, CancellationToken, Task<T>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.AddRoute("GET", path, action, condition, name);
        }

        public virtual void Head(string path, Func<dynamic, object> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Head<object>(path, action, condition, name);
        }

        public virtual void Head<T>(string path, Func<dynamic, T> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Head(path, args => Task.FromResult(action((DynamicDictionary)args)), condition, name);
        }

        public virtual void Head(string path, Func<dynamic, Task<object>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Head<object>(path, action, condition, name);
        }

        public virtual void Head<T>(string path, Func<dynamic, Task<T>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Head(path, (args, ct) => action((DynamicDictionary)args), condition, name);
        }

        public virtual void Head(string path, Func<dynamic, CancellationToken, Task<object>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Head<object>(path, action, condition, name);
        }

        public virtual void Head<T>(string path, Func<dynamic, CancellationToken, Task<T>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.AddRoute("HEAD", path, action, condition, name);
        }

        public virtual void Options(string path, Func<dynamic, object> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Options<object>(path, action, condition, name);
        }

        public virtual void Options<T>(string path, Func<dynamic, T> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Options(path, args => Task.FromResult(action((DynamicDictionary)args)), condition, name);
        }

        public virtual void Options(string path, Func<dynamic, Task<object>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Options<object>(path, action, condition, name);
        }

        public virtual void Options<T>(string path, Func<dynamic, Task<T>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Options(path, (args, ct) => action((DynamicDictionary)args), condition, name);
        }

        public virtual void Options(string path, Func<dynamic, CancellationToken, Task<object>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Options<object>(path, action, condition, name);
        }

        public virtual void Options<T>(string path, Func<dynamic, CancellationToken, Task<T>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.AddRoute("OPTIONS", path, action, condition, name);
        }

        public virtual void Patch(string path, Func<dynamic, object> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Patch<object>(path, action, condition, name);
        }

        public virtual void Patch<T>(string path, Func<dynamic, T> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Patch(path, args => Task.FromResult(action((DynamicDictionary)args)), condition, name);
        }

        public virtual void Patch(string path, Func<dynamic, Task<object>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Patch<object>(path, action, condition, name);
        }

        public virtual void Patch<T>(string path, Func<dynamic, Task<T>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Patch(path, (args, ct) => action((DynamicDictionary)args), condition, name);
        }

        public virtual void Patch(string path, Func<dynamic, CancellationToken, Task<object>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Patch<object>(path, action, condition, name);
        }

        public virtual void Patch<T>(string path, Func<dynamic, CancellationToken, Task<T>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.AddRoute("PATCH", path, action, condition, name);
        }

        public virtual void Post(string path, Func<dynamic, object> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Post<object>(path, action, condition, name);
        }

        public virtual void Post<T>(string path, Func<dynamic, T> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Post(path, args => Task.FromResult(action((DynamicDictionary)args)), condition, name);
        }

        public virtual void Post(string path, Func<dynamic, Task<object>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Post<object>(path, action, condition, name);
        }

        public virtual void Post<T>(string path, Func<dynamic, Task<T>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Post(path, (args, ct) => action((DynamicDictionary)args), condition, name);
        }

        public virtual void Post(string path, Func<dynamic, CancellationToken, Task<object>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Post<object>(path, action, condition, name);
        }

        public virtual void Post<T>(string path, Func<dynamic, CancellationToken, Task<T>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.AddRoute("POST", path, action, condition, name);
        }

        public virtual void Put(string path, Func<dynamic, object> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Put<object>(path, action, condition, name);
        }

        public virtual void Put<T>(string path, Func<dynamic, T> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Put(path, args => Task.FromResult(action((DynamicDictionary)args)), condition, name);
        }

        public virtual void Put(string path, Func<dynamic, Task<object>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Put<object>(path, action, condition, name);
        }

        public virtual void Put<T>(string path, Func<dynamic, Task<T>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Put(path, (args, ct) => action((DynamicDictionary)args), condition, name);
        }

        public virtual void Put(string path, Func<dynamic, CancellationToken, Task<object>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.Put<object>(path, action, condition, name);
        }

        public virtual void Put<T>(string path, Func<dynamic, CancellationToken, Task<T>> action, Func<Context, bool> condition = null, string name = null)
        {
            this.AddRoute("PUT", path, action, condition, name);
        }

        #endregion
    }
}
