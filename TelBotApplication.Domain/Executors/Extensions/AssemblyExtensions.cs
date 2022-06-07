using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
