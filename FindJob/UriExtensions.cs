using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FindJob
{
    public static class UriExtensions
    {
        public static string UnlimitedCode = "0";

        public static string AddQueryParameters(this string uri, params (string key, string value)[] parameters)
        {
            var builder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(builder.Query);

            foreach (var (key, value) in parameters)
            {
                query[key] = value;
            }

            builder.Query = query.ToString();
            return builder.Uri.AbsoluteUri;
        }
        // Appends a parameter to a query string if the value is not equal to the 'unlimited' code.
        public static string appendParam(this string uri, string paramName, string paramValue)
        {
            return uri + (!string.IsNullOrEmpty(paramValue) && paramValue != UnlimitedCode
                ? $"&{paramName}={paramValue}"
                : string.Empty);
        }

        // Appends a list of parameters to a query string if the list is not empty and its first element is not the 'unlimited' code.
        public static string appendListParam(this string uri, string paramName, List<string> paramValues)
        {
            paramValues?.RemoveAll(v => v == UnlimitedCode);
            return uri + (paramValues != null && paramValues.Count > 0 && paramValues[0] != UnlimitedCode
                ? $"&{paramName}={string.Join(",", paramValues)}"
                : string.Empty);
        }
    }
}
