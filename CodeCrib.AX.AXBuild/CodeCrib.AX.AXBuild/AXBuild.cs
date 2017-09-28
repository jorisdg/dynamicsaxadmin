//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeCrib.AX.AXBuild
{
    public class AXBuild
    {
        public string ExecutablePath { get; set; }

        public static AXBuild NewServerBinPath(string serverBinPath)
        {
            return new AXBuild(Path.Combine(new[] { serverBinPath, "AxBuild.exe" }));
        }

        public AXBuild(string executablePath)
        {
            ExecutablePath = executablePath;
        }

        public static void ExecuteCommand(string serverBinPath, Commands.Command command, int timeOutMinutes)
        {
            AXBuild axBuild = NewServerBinPath(serverBinPath);

            axBuild.Execute(command.Parameters(), timeOutMinutes);
        }

        public static Process StartCommand(string serverBinPath, Commands.Command command)
        {
            AXBuild axBuild = NewServerBinPath(serverBinPath);

            return axBuild.Start(command.Parameters());
        }

        public static Process CreateCommand(string serverBinPath, Commands.Command command)
        {
            AXBuild axBuild = NewServerBinPath(serverBinPath);

            return axBuild.Create(command.Parameters());
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
                        // TODO - How can the timeout handler account for the worker processes?
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
                        throw new TimeoutException(string.Format("AXBuild time out of {0} minutes exceeded, additionally an exception was encountered while trying to kill the AXBuild process (see innerexception)", timeOutMinutes), outerException);
                    }
                    else
                    {
                        throw new TimeoutException(string.Format("AXBuild time out of {0} minutes exceeded", timeOutMinutes));
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
            Process process = Create(parameterList);
            process.Start();

            return process;
        }

        public Process Create(List<string> parameterList)
        {
            Process process = new Process();

            string parameterString = string.Join(" ", parameterList.ToArray());

            process.StartInfo.FileName = ExecutablePath;
            process.StartInfo.Arguments = parameterString;

            return process;
        }
    }
}
