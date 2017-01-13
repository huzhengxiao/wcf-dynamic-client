using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel.Description;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CSharp;


namespace AgileBoost.WCF.DyncmicClient.Utility
{
    internal class WcfProxyClientAssembly
    {
        private WcfUrlAddressInfo _wcfUrlAddressInfo;
        private static object _locker = new object();
        private static object _upgradeLocker = new object();
        private static IDictionary<string,Type> _cacheDictionary = new ConcurrentDictionary<string, Type>();
        private string _version;

        public WcfProxyClientAssembly(string url)
        {
            this._wcfUrlAddressInfo = new WcfUrlAddressInfo(url);
        }

        

        public Type GetWcfProxyClientType()
        {
            if (_cacheDictionary.ContainsKey(_wcfUrlAddressInfo.WcfUrl))
            {
                return _cacheDictionary[_wcfUrlAddressInfo.WcfUrl];
            }


            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly assembly = null;
            if (!assemblies.Any(s => s.FullName.IndexOf(_wcfUrlAddressInfo.AssemblyName)==0))
            {
                lock (_locker)
                {
                    var dllFullPath = this.GetDllFullPath(false);
                    if (!File.Exists(dllFullPath))
                    {
                        // dot not find compiled dll file,
                        // then ready to compiled it.
                        string csharpCodeFilePath = this.GetCSharpFileFullPath(false);
                        if (!File.Exists(csharpCodeFilePath))
                        {
                            this.GenerateCShareFile(csharpCodeFilePath);
                        }
                        this.CompileCSharpFile(csharpCodeFilePath,false);
                        assembly = Assembly.LoadFile(this.GetDllFullPath(false));
                        
                    }
                    else
                    {
                        if (
                            !AppDomain.CurrentDomain.GetAssemblies()
                                .Any(s => s.FullName.IndexOf(_wcfUrlAddressInfo.AssemblyName)==0))
                        {
                            assembly = Assembly.LoadFile(dllFullPath);
                        }
                    }
                }
                

            }
            else
            {
                assembly = assemblies.Where(s => s.FullName.IndexOf(_wcfUrlAddressInfo.AssemblyName)==0).Select(s=>s).OrderByDescending(s=>s.FullName).FirstOrDefault();
                
            }
            lock (_locker)
            {
                if (_cacheDictionary.ContainsKey(this._wcfUrlAddressInfo.WcfUrl))
                {
                    return _cacheDictionary[this._wcfUrlAddressInfo.WcfUrl];
                }
                else
                {
                    Type type = assembly.GetTypes().FirstOrDefault(s => s.IsInterface);
                    this._wcfUrlAddressInfo.SetServiceFullName(type.FullName);
                    _cacheDictionary.Add(this._wcfUrlAddressInfo.WcfUrl,type);
                    return type;
                }
                
            }
            
        }


        private void CompileCSharpFile(string filePath,bool updateMode)
        {
            CSharpCodeProvider csc = new CSharpCodeProvider();
            ICodeCompiler icc = csc.CreateCompiler();
            this.Version = this.GetVersionFromFilePath(filePath);

            //设定编译参数
            CompilerParameters cplist = new CompilerParameters();
            
            cplist.GenerateExecutable = false;
            cplist.GenerateInMemory = true;
            cplist.ReferencedAssemblies.Add("System.dll");
            cplist.ReferencedAssemblies.Add("System.Core.dll");
            cplist.ReferencedAssemblies.Add("System.XML.dll");
            cplist.ReferencedAssemblies.Add("System.Data.dll");
            cplist.ReferencedAssemblies.Add("System.ServiceModel.dll");
            cplist.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            cplist.ReferencedAssemblies.Add("System.Runtime.Serialization.dll");


            cplist.GenerateExecutable = false;
            cplist.GenerateInMemory = false;
            cplist.OutputAssembly = $"{this._wcfUrlAddressInfo.AssemblyName}.{this.Version}.dll";
            cplist.CompilerOptions = "/optimize";



           var result = icc.CompileAssemblyFromFile(cplist, filePath);
            if (result.Errors.HasErrors)
            {
                foreach (CompilerError error in result.Errors)
                {
                    Console.WriteLine(error.ErrorText);
                }
            }

            else
            {
                string dllFolder = this.GetDllFileSaveFolder();
                if (!Directory.Exists(dllFolder))
                {
                    Directory.CreateDirectory(dllFolder);
                }
                File.Copy(result.PathToAssembly, this.GetDllFullPath(updateMode), true);
                File.Delete(result.PathToAssembly);
            }

           
        }

        private void GenerateCShareFile(string filePath)
        {

            MetadataExchangeClient client = new MetadataExchangeClient(new Uri(this.GetWsdlAddress()), MetadataExchangeClientMode.HttpGet);
            MetadataSet metadata = client.GetMetadata();
            WsdlImporter importer = new WsdlImporter(metadata);
            CodeCompileUnit ccu = new CodeCompileUnit();
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            ServiceContractGenerator generator = new ServiceContractGenerator(ccu);
            foreach (ContractDescription description in importer.ImportAllContracts())
            {
                generator.GenerateServiceContractType(description);
            }
            StringWriter writer = new StringWriter();
            provider.GenerateCodeFromCompileUnit(ccu, writer, null);
            string code = writer.ToString();
            string folder = $"{AppDomain.CurrentDomain.BaseDirectory}{WcfConfig.CSharpCodeFolder}\\{this._wcfUrlAddressInfo.AssemblyName}\\";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            using (var fs = File.Open(filePath, FileMode.Create, FileAccess.Write))
            {
                using (var stream = new StreamWriter(fs,Encoding.UTF8))
                {
                    // write assembly info 
                    
                    stream.WriteLine("using System.Reflection;");
                    stream.WriteLine("using System.Runtime.CompilerServices;");
                    stream.WriteLine("using System.Runtime.InteropServices;");
                    stream.WriteLine("");
                    stream.WriteLine("// 有关程序集的一般信息由以下");
                    stream.WriteLine("// 控制。更改这些特性值可修改");
                    stream.WriteLine("// 与程序集关联的信息。");
                    stream.WriteLine($"[assembly: AssemblyTitle(\"WcfProxyClient_{this._wcfUrlAddressInfo.AssemblyName}_{this.Version}\")]");
                    stream.WriteLine("[assembly: AssemblyDescription(\"WcfProxyClient\")]");
                    stream.WriteLine("[assembly: AssemblyConfiguration(\"\")]");
                    stream.WriteLine("[assembly: AssemblyCompany(\"AgileBoost\")]");
                    stream.WriteLine("[assembly: AssemblyProduct(\"WcfProxyClient\")]");
                    stream.WriteLine("[assembly: AssemblyCopyright(\"Copyright © 2017\")]");
                    stream.WriteLine("[assembly: AssemblyTrademark(\"\")]");
                    stream.WriteLine("[assembly: AssemblyCulture(\"\")]");
                    stream.WriteLine("");
                    stream.WriteLine("//将 ComVisible 设置为 false 将使此程序集中的类型");
                    stream.WriteLine("//对 COM 组件不可见。  如果需要从 COM 访问此程序集中的类型，");
                    stream.WriteLine("//请将此类型的 ComVisible 特性设置为 true。");
                    stream.WriteLine("[assembly: ComVisible(false)]");
                    stream.WriteLine("");
                    stream.WriteLine("// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID");
                    stream.WriteLine("[assembly: Guid(\""+Guid.NewGuid().ToString()+"\")]");
                    stream.WriteLine("");
                    stream.WriteLine("// 程序集的版本信息由下列四个值组成: ");
                    stream.WriteLine("//");
                    stream.WriteLine("//      主版本");
                    stream.WriteLine("//      次版本");
                    stream.WriteLine("//      生成号");
                    stream.WriteLine("//      修订号");
                    stream.WriteLine("//");
                    stream.WriteLine("// 您可以指定所有值，也可以通过使用“*”来使用");
                    stream.WriteLine("// 方法是按如下所示使用“*”: :");
                    stream.WriteLine("// [assembly: AssemblyVersion(\"1.0.*\")]");
                    stream.WriteLine($"[assembly: AssemblyVersion(\"{this.Version}\")]");
                    stream.WriteLine($"[assembly: AssemblyFileVersion(\"{this.Version}\")]");
                    stream.WriteLine("");
                    stream.WriteLine(""); 

                    stream.WriteLine($"namespace {this._wcfUrlAddressInfo.Namespace}");
                    stream.WriteLine("{");
                    stream.Write(code);
                    stream.WriteLine("}");
                }
            } 

        }

        private string GetVersionFromFilePath(string filePath)
        {
            string expression = @"\d{4}\.\d{4}\.\d+\.\d+";
            var match = Regex.Match(filePath, expression);
            if (match != null)
            {
                return match.Value;
            }
            else
            {
                return "";
            }
        }

        private string Version
        {
            get
            {
                if (string.IsNullOrEmpty(_version))
                {
                    _version = DateTime.Now.ToString("yyMM.ddHH.MM.ss");
                }
                return _version;
            }
            set { _version = value; }
        }

        private string GetCSharpFileFullPath(bool updateMode)
        {
            string folder = this.GetCSharpCodeFileSaveFolder();
            if (!updateMode && Directory.Exists(folder))
            {
                var dir = new DirectoryInfo(folder);
                var files = dir.GetFiles();
                if (files?.Any()??false)
                {
                    var file = files.OrderByDescending(s => s.CreationTime).FirstOrDefault();
                    return file.FullName;
                }
                else
                {
                    return $"{folder}{this._wcfUrlAddressInfo.ServiceName}.{this.Version}.cs";
                }
                
            }
            else
            {
                string path = $"{folder}{this._wcfUrlAddressInfo.ServiceName}.{this.Version}.cs";
                return path;
            }
        }


        private string GetCSharpCodeFileSaveFolder()
        {
            return  $@"{AppDomain.CurrentDomain.BaseDirectory}{WcfConfig.CSharpCodeFolder}\{this._wcfUrlAddressInfo.AssemblyName}\";
        }

        /// <summary>
        /// 升级接口
        /// </summary>
        public void UpgradeInterface()
        {
             
            this.Version = DateTime.Now.ToString("yyMM.ddHH.mm.ss");

            lock (_upgradeLocker)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                // 如果已经生成，则跳出
                if (assemblies.Any(s=>s.FullName==$"{_wcfUrlAddressInfo.AssemblyName}.{this.Version}"))
                    return;

                var dllFullPath = this.GetDllFullPath(true);
                if(File.Exists(dllFullPath))
                    return;
                string csharpCodeFilePath = this.GetCSharpFileFullPath(true);
                if (!File.Exists(csharpCodeFilePath))
                {
                    this.GenerateCShareFile(csharpCodeFilePath);
                }
                this.CompileCSharpFile(csharpCodeFilePath,true);

                var assembly = Assembly.LoadFile(this.GetDllFullPath(true));
                Type type = assembly.GetTypes().FirstOrDefault(s => s.IsInterface);
                this._wcfUrlAddressInfo.SetServiceFullName(type.FullName);

                if (_cacheDictionary.ContainsKey(this._wcfUrlAddressInfo.WcfUrl))
                {
                    _cacheDictionary[this._wcfUrlAddressInfo.WcfUrl] = type;
                }
                else
                {
                    _cacheDictionary.Add(this._wcfUrlAddressInfo.WcfUrl,type);
                }
            }

            
        }


        private string GetWsdlAddress()
        {
            return this._wcfUrlAddressInfo.WcfUrl + "?wsdl";
        }

        private string GetDllFileSaveFolder()
        {
            return $@"{AppDomain.CurrentDomain.BaseDirectory}{WcfConfig.DllFolder}\{this._wcfUrlAddressInfo.AssemblyName}\";
        }

        private string GetDllFullPath(bool updateMode)
        {
            string folder = this.GetDllFileSaveFolder();

            if (!updateMode && Directory.Exists(folder))
            {
                var dir = new DirectoryInfo(folder);
                var files = dir.GetFiles();
                if (files?.Any() ?? false)
                {
                    var file = files.OrderByDescending(s => s.CreationTime).FirstOrDefault();
                    return file.FullName;
                }
                else
                {
                    return $"{folder}{this._wcfUrlAddressInfo.AssemblyName}.{this.Version}.dll";
                }

            }
            else
            {
                string path = $"{folder}{this._wcfUrlAddressInfo.AssemblyName}.{this.Version}.dll";
                return path;
            }
        }
    }
}

