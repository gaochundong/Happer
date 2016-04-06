using System;

namespace Happer.Rpc
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class RpcMethodAttribute : Attribute
    {
        public RpcMethodAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName { get; private set; }
    }
}
