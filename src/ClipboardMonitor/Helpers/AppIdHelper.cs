using System.Linq;
using System.Reflection;

namespace ClipboardMonitor.Helpers
{
    internal static class AppIdHelper
    {

        internal static string EnsureAppId()
        {
            var asm = Assembly.GetExecutingAssembly();
            var name = asm.GetName().Name;                  // e.g. "ClipboardMonitor"
            var companyAttr = asm.GetCustomAttribute<AssemblyCompanyAttribute>();
            var final = string.Concat((from c in companyAttr.Company.Split(' ')
                                       select c[0]).ToList());

            var appId = $"{final}.{name}";
            NativeMethods.SetCurrentProcessExplicitAppUserModelID(appId);
            return appId;
        }
    }
}
