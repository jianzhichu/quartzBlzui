using Autofac;
using System;
using System.Linq;
using System.Reflection;

namespace QM.Utility

{
    public class CustomAutofacModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder containerBuilder)
        {


            //程序集注入
            Assembly serviceAss = Assembly.Load("QM.Service");
            Type[] sertypes = serviceAss.GetTypes().Where(p => p.Name.EndsWith("Service")).ToArray();
            containerBuilder.RegisterTypes(sertypes).AsImplementedInterfaces().PropertiesAutowired();
            Assembly interfaceAss = Assembly.Load("QM.Interface");
            Type[] interfacetypes = interfaceAss.GetTypes().Where(p => p.Name.EndsWith("Service")).ToArray();
            containerBuilder.RegisterTypes(interfacetypes).AsImplementedInterfaces().PropertiesAutowired();

        }

    }
}
