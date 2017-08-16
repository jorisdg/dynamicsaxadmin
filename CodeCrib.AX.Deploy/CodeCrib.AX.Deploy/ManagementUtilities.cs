using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace CodeCrib.AX.Deploy
{
    public class ManagementUtilities
    {
        public static string InstallDir()
        {
            RegistryKey AXInstall = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Dynamics\6.0\Setup");
            return System.IO.Path.Combine(AXInstall.GetValue("InstallDir").ToString(), "ManagementUtilities");
        }
    }
}
