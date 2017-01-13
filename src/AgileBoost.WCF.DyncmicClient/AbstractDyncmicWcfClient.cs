using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileBoost.WCF.DynamicClient
{
    public abstract class AbstractDyncmicWcfClient:IDisposable
    {

        protected string _url;

        public AbstractDyncmicWcfClient(string url)
        {
            this._url = url;
        }

        /// <summary>
        /// 升级接口,调用此接口以后，需要重新获取一个新实例来进行操作，方可使用最新版本，否则还是使用的旧版本功能。
        /// </summary>
        public abstract void Upgrade();

        public abstract object Invoke(string method, params object[] parameters);

        public abstract void Dispose();
    }
}
