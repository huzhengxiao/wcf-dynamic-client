using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AgileBoost.WCF.DyncmicClient.Utility
{
    class WcfUrlAddressInfo
    {

        private string _wcfUrl;

        public WcfUrlAddressInfo(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw  new ArgumentNullException("url cannot be null");
            }
            this._wcfUrl = url;
            
            this.Parse();
        }

        public string WcfUrl { get { return this._wcfUrl; } }

        public string ServiceName { get; private set; }

        public string Namespace { get; private set; }

         


        public string ServiceFullName { get;private set; }

        public string AssemblyName { get;private set; }

         

        public void SetServiceFullName(string fullName)
        {
            this.ServiceFullName = fullName;
        }

        private void Parse()
        {
            
            Uri uri = new Uri(this._wcfUrl);

            this.ServiceName = GetServiceName();
            this.Namespace = $"{uri.Host.Replace(".","_")}_{uri.Port}_{this.ServiceName}"; 
            this.AssemblyName = this.Namespace; 
        }



        private string GetServiceName()
        {
            string regExpression1 = $@"\w+(\.svc)";
            string regExpression2 = @"\w+\/mex";
            string result = "";

            if (Regex.IsMatch(this._wcfUrl, regExpression1))
            {
                var match = Regex.Match(this._wcfUrl, regExpression1);
                result = match?.Value.Replace(".svc", "");
            }
            else if (Regex.IsMatch(this._wcfUrl, regExpression2))
            {
                var match = Regex.Match(this._wcfUrl, regExpression2);
                return result = match?.Value.Replace("/mex", "");
            }
            return result;;
        }

    }
}
