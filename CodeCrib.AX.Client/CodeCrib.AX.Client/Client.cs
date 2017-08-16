//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Microsoft.Win32;

namespace CodeCrib.AX.Client
{
    public class Client
    {
        public const string regKey = @"Software\Microsoft\Dynamics\6.0\Setup\Components";

        public string ExecutablePath { get; set; }

        public static string GetInstallDir32()
        {
            RegistryKey componentsKey = Registry.CurrentUser.OpenSubKey(regKey, false);
            String installDir32 = componentsKey.GetValue("InstallDir32", null) as String;

            return System.Environment.ExpandEnvironmentVariables(installDir32);
        }

        public Client()
        {
            ExecutablePath = string.Format(@"{0}\Client\bin\ax32.exe", GetInstallDir32());
        }

        public Client(string executablePath)
        {
            ExecutablePath = executablePath;
        }

        public static void ExecuteCommand(Commands.Command command, int timeOutMinutes)
        {
            Client client = new Client();

            client.Execute(command.Parameters(), timeOutMinutes);
        }

        public static Process StartCommand(Commands.Command command)
        {
            Client client = new Client();

            return client.Start(command.Parameters());
        }

        public static void ExecuteCommand(string clientPath, Commands.Command command, int timeOutMinutes)
        {
            Client client = new Client(clientPath);

            client.Execute(command.Parameters(), timeOutMinutes);
        }

        public static Process StartCommand(string clientPath, Commands.Command command)
        {
            Client client = new Client(clientPath);

            return client.Start(command.Parameters());
        }

        public void Execute(Commands.Command command, int timeOutMinutes)
        {
            Execute(command.Parameters(), timeOutMinutes);
        }

        public Process Start(Commands.Command command)
        {
            return Start(command.Parameters());
        }

        public void Execute(List<string> parameterList, int timeOutMinutes)
        {
            Process process = Start(parameterList);

            if (timeOutMinutes > 0)
            {
                if (!process.WaitForExit((int)new TimeSpan(0, timeOutMinutes, 0).TotalMilliseconds))
                {
                    // Process is still running after the timeout has elapsed.

                    Exception outerException = null;

                    try
                    {
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        // Error trying to kill the process
                        // Don't throw the error just yet...
                        outerException = ex;
                    }

                    if (outerException != null)
                    {
                        throw new TimeoutException(String.Format("Client time out of {0} minutes exceeded, additionally an exception was encountered while trying to kill the client process (see innerexception)", timeOutMinutes), outerException);
                    }
                    else
                    {
                        throw new TimeoutException(String.Format("Client time out of {0} minutes exceeded", timeOutMinutes));
                    }
                }
            }
            else
            {
                process.WaitForExit();
            }
        }

        public Process Start(List<string> parameterList)
        {
            string parameterString = String.Join(" ", parameterList.ToArray());

            ProcessStartInfo processStartInfo = new ProcessStartInfo(ExecutablePath, parameterString);
            processStartInfo.WindowStyle = ProcessWindowStyle.Minimized;

            return Process.Start(processStartInfo);
        }
    }
}
