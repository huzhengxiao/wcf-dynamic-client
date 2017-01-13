# wcf-dynamic-client
wcf的动态代理客户端，主要功能就是为了动态调用wcf接口。

## 基本功能介绍 
这个库是为了解决有多个wcf地址要被调用的情况，却又不想为每个wcf都手动生成一个代理客户端的代码(使用svcutil.exe)。 
只需要使用`WcfInvokeHelper.Invoke(string url, string methodName, params object[] parameters)`即可调用目标接口。 

## 细节相关说明 
主要使用了动态编译功能。 
根据提供的url去生成客户端代理类（基本上和用svcutil.exe生成的一样），并在生成的代码中增加了程序集信息，和我们新建的项目里，都会有Assembly.cs的代码一样。 
然后将其动态编译，将加载到当前应用程序域中，然后获取服务的接口声明，然后使用反射，创建ChannelFactory的实例，然后调用CreateChannel来创建客户端，以供调用。 
 
 增加的程序集版本是为了可以动态升级调用，而不用重启应用程序。升级是需要通过调用Upgrade()方法来实现的。
 升级是重新获取一次wsdl的信息，然后重新生成新代码（代码中标记了新的版本信息）并编译，然后将其加载到应用程序域中，下次调用时，就调用最新版本的程序集来构建客户端实际。
 对之前的调用不影响。
 
 
 ## 使用方法
 
 ### 直接调用静态方法即可。
 ```c#
 /// 这个在每次操作完成以后，会自动关闭通道，也即 client.Close();
 WcfInvokeHelper.Invoke(string url, string methodName, params object[] parameters);
 
 ```
 
 
 ### 使用实例方法调用
 ```c#
 using(var client = new DefaultWcfClient(url))
 {
     client.Invoke(string methodName,params object[] parameters);
     client.Invoke(string methodName,params object[] parameters);
     client.Invoke(string methodName,params object[] parameters);
     // 使用完成之后，会自动释放资源，也即调用client.Close()关闭通道。
 }
 ```
