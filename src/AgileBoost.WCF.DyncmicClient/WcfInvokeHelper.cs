using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileBoost.WCF.DyncmicClient
{
    public static class WcfInvokeHelper
    {

        public static object Invoke(string url, string methodName, params object[] parameters)
        {
            using (var client = new DefaultWcfClient(url, "basichttpbinding"))
            {
                return client.Invoke(methodName, parameters);
            }
        }
    }
}
