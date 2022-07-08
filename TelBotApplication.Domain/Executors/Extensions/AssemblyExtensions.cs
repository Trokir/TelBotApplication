using System;
using System.Linq;
using System.Reflection;

namespace TelBotApplication.Domain.Executors.Extensions
{
    public static class AssemblyExtensions
    {

        public static Assembly GetAssemblyByName(this AppDomain domain, string assemblyName)
        {
            return domain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
        }
    }
}
