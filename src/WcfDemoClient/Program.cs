using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using AgileBoost.Bosch.WCF.DyncmicClient;
using BoschWcfDemoClient.Entities;
using BoschWcfDemoClient.Entities.Demo;
using BoschWcfDemoClient.Entities.Demo.Xixi;
using BoschWcfDemoClient.ServiceReference1;

namespace BoschWcfDemoClient
{
    class Program
    {
        static void Main(string[] args)
        {

            TestUpgrade();

            TestWcf1();
            TestWcf2();
            TestWcf1();

            //            var list = new List<Product>();
            //
            //            Console.WriteLine(list.GetType());
            //
            //            CreateListByDynamic();


            Console.WriteLine("please press anykey to continue...");
            Console.ReadLine();
        }


        static void TestUpgrade()
        {
            string url = "http://localhost:37566/Service1.svc";
            using (var client = new DefaultWcfClient(url, "basichttpbinding"))
            {
                Console.WriteLine(client.Invoke("GetData", 10));
            }
            using (var client = new DefaultWcfClient(url, "basichttpbinding"))
            {
                Console.WriteLine("Upgrade");
                client.Upgrade();
            }
            using (var client = new DefaultWcfClient(url, "basichttpbinding"))
            {
                Console.WriteLine(client.Invoke("GetData", 10));
            }
        }

        static void TestWcf1()
        {
            string url = "http://localhost:37566/Service1.svc";
            string method = "GetData";
            var value = 10;
            var result = WcfInvokeHelper.Invoke(url, method, value);
            Console.WriteLine($"wcf:method:{method},value:{value},result:{result}");
        }


        static void TestWcf2()
        {
            
            string url = "http://localhost:38626/Service1.svc";
            string method = "GetData";
            var value = 130;
            var result = WcfInvokeHelper.Invoke(url, method, value);
            Console.WriteLine($"wcf:method:{method},value:{value},result:{result}");
        }


        private static void TestWcfByServiceReference()
        {
            string url = "http://localhost:37566/Service1.svc";
            var address = new EndpointAddress(url);
            ChannelFactory<IService1> cf = new ChannelFactory<IService1>(new BasicHttpBinding(),address);
            
        }

        private static void CreateListByDynamic()
        {
            Type productType = typeof(Product);
            Type listType = typeof(List<>);
            listType = listType.MakeGenericType(productType);
            var result = Activator.CreateInstance(listType);

            Console.WriteLine(result.GetType());

            MyList demo  = new MyList();
            demo.Name = "";
        }
    }
}
