using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildRuntime
{
    public class AssemblyResolver
    {
        public static void SetUp()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(ResolveEventHandler);
        }

        static public Assembly ResolveEventHandler(object sender, ResolveEventArgs args)
        {
            string name = args.Name.Substring(0, args.Name.IndexOf(","));
            if (name.Equals("axutillib", StringComparison.CurrentCultureIgnoreCase))
            {
                //Build the path of the assembly from where it has to be loaded.				
                RegistryKey AXInstall = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Dynamics\6.0\Setup");
                string strTempAssmbPath = string.Format(@"{0}\ManagementUtilities\{1}.dll", AXInstall.GetValue("InstallDir"), name);

                return Assembly.LoadFrom(strTempAssmbPath);
            }

            return null;
        }
    }
}
