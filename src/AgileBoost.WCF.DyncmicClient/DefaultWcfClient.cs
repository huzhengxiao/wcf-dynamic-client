using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using AgileBoost.WCF.DynamicClient;
using AgileBoost.WCF.DyncmicClient.Utility;

namespace AgileBoost.WCF.DyncmicClient
{
    public class DefaultWcfClient : AbstractDyncmicWcfClient
    {
        private object _client;
        private Type _cfType; 
        private object _channelFactory;
        private Type _serviceType;
        private MethodInfo[] _methods;
        private string _url;

        public DefaultWcfClient(string url, string bindingType) : base(url)
        {
            this._url = url;
            WcfProxyClientAssembly assembly = new WcfProxyClientAssembly(url);
            _serviceType = assembly.GetWcfProxyClientType();

            Init(url,bindingType);
        }

        private void Init(string url,string bindingType)
        {
            var address = new EndpointAddress(url);
            Binding binding = this.CreateBinding(bindingType);


            _cfType = typeof(ChannelFactory<>);
            _cfType = _cfType.MakeGenericType(_serviceType);
            var parameters = new List<object>();
            parameters.Add(binding);
            parameters.Add(address);
            _channelFactory = Activator.CreateInstance(_cfType, BindingFlags.Default, null, parameters.ToArray(), null, null);
            _client = _cfType.InvokeMember("CreateChannel", BindingFlags.Default | BindingFlags.InvokeMethod, null, _channelFactory,
                null);


            _methods = _serviceType.GetMethods();
        }


        public override void Upgrade()
        {
            WcfProxyClientAssembly assembly = new WcfProxyClientAssembly(_url);
            assembly.UpgradeInterface();
        }


        private Binding CreateBinding(string binding)
        {
            Binding bindinginstance = null;
            if (binding.ToLower() == "basichttpbinding")
            {
                BasicHttpBinding ws = new BasicHttpBinding();
                ws.MaxReceivedMessageSize = 65535000;
                bindinginstance = ws;
            }
            else if (binding.ToLower() == "netnamedpipebinding")
            {
                NetNamedPipeBinding ws = new NetNamedPipeBinding();
                ws.MaxReceivedMessageSize = 65535000;
                bindinginstance = ws;
            }
            else if (binding.ToLower() == "netpeertcpbinding")
            {
                NetPeerTcpBinding ws = new NetPeerTcpBinding();
                ws.MaxReceivedMessageSize = 65535000;
                bindinginstance = ws;
            }
            else if (binding.ToLower() == "nettcpbinding")
            {
                NetTcpBinding ws = new NetTcpBinding();
                ws.MaxReceivedMessageSize = 65535000;
                ws.Security.Mode = SecurityMode.None;
                bindinginstance = ws;
            }
            else if (binding.ToLower() == "wsdualhttpbinding")
            {
                WSDualHttpBinding ws = new WSDualHttpBinding();
                ws.MaxReceivedMessageSize = 65535000;

                bindinginstance = ws;
            }
            else if (binding.ToLower() == "webhttpbinding")
            {
//                WebHttpBinding ws = new WebHttpBinding();
//                ws.MaxReceivedMessageSize = 65535000;
//                bindinginstance = ws;
            }
            else if (binding.ToLower() == "wsfederationhttpbinding")
            {
                WSFederationHttpBinding ws = new WSFederationHttpBinding();
                ws.MaxReceivedMessageSize = 65535000;
                bindinginstance = ws;
            }
            else if (binding.ToLower() == "wshttpbinding")
            {
                WSHttpBinding ws = new WSHttpBinding(SecurityMode.None);
                ws.MaxReceivedMessageSize = 65535000;
                ws.Security.Message.ClientCredentialType = System.ServiceModel.MessageCredentialType.Windows;
                ws.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                bindinginstance = ws;
            }
            return bindinginstance;
        }


        public override object Invoke(string method, params object[] parameters)
        {
            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentNullException("method");
            }

            if (!_methods.Any(s => s.Name == method))
            {
                method = _methods.FirstOrDefault(s => s.Name.ToLower() == method.ToLower())?.Name ?? method;
            }

            return _serviceType.InvokeMember(method, BindingFlags.Default | BindingFlags.InvokeMethod, null, _client, parameters);

        }

        public override void Dispose()
        {
            _cfType.InvokeMember("Close", BindingFlags.Default | BindingFlags.InvokeMethod, null, _channelFactory, null);

            _cfType = null;
            _client = null;
        }
    }
}