using System;
using System.Reflection;

namespace Happer.Http.Responses
{
    public class ResponseNegotiator
    {
        public ResponseNegotiator()
        {
        }

        public Response NegotiateResponse(dynamic routeResult, Context context)
        {
            Response response;
            if (TryCastResultToResponse(routeResult, out response))
            {
                return response;
            }

            throw new NotSupportedException("Negotiate response failed.");
        }

        private static bool TryCastResultToResponse(dynamic routeResult, out Response response)
        {
            var targetType = routeResult.GetType();
            var responseType = typeof(Response);

            var methods = responseType.GetMethods(BindingFlags.Public | BindingFlags.Static);

            foreach (var method in methods)
            {
                if (!method.Name.Equals("op_Implicit", StringComparison.Ordinal))
                {
                    continue;
                }

                if (method.ReturnType != responseType)
                {
                    continue;
                }

                var parameters = method.GetParameters();

                if (parameters.Length != 1)
                {
                    continue;
                }

                if (parameters[0].ParameterType != targetType)
                {
                    continue;
                }

                response = (Response)routeResult;
                return true;
            }

            response = null;
            return false;
        }
    }
}
