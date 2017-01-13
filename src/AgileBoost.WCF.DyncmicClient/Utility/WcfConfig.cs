using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileBoost.WCF.DyncmicClient.Utility
{
    internal static class WcfConfig
    {

        public static string DllFolder
        {
            get { return "WcfDlls"; }
        }

        public static string CSharpCodeFolder
        {
            get { return "WcfProxyClientCodes"; }
        }
    }
}
