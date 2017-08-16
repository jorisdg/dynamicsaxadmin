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

namespace CodeCrib.AX.Setup
{
    public class Setup
    {
        /// <summary>
        /// Path and filename of Dynamics AX setup executable
        /// </summary>
        public string ExecutablePath
        {
            get;
            set;
        }

        /// <summary>
        /// Path and filename of setup parameters file
        /// </summary>
        public string ParametersFile
        {
            get;
            set;
        }

        /// <summary>
        /// Run Dynamics AX Setup as specified in ExecutablePath property, and pass in list of parameters.
        /// This will save the parameters as a file in the %temp% folder of the current user.
        /// This method will wait for the setup to exit.
        /// </summary>
        /// <param name="parameters">List of parameters to use for setup</param>
        /// <returns>Exit code of the Dynamics AX Setup program</returns>
        public int Run(List<Parameter> parameters)
        {
            string filename = string.Format(@"{0}\{1}.txt", Environment.GetEnvironmentVariable("TEMP"), Guid.NewGuid().ToString());
            ParameterFile.Save(parameters, filename);

            return Run(filename);
        }

        /// <summary>
        /// Run Dynamics AX Setup as specified in ExecutablePath property, and pass parameters file specified in the ParametersFile property.
        /// This method will wait for the setup to exit.
        /// </summary>
        /// <returns>Exit code of the Dynamics AX Setup program</returns>
        public int Run()
        {
            return Run(ParametersFile);
        }

        /// <summary>
        /// Run Dynamics AX Setup as specified in ExecutablePath property
        /// This method will wait for the setup to exit.
        /// </summary>
        /// <param name="filename">Path and filename of parameters file to pass to setup</param>
        /// <returns>Exit code of the Dynamics AX Setup program</returns>
        public int Run(string filename)
        {
            Process axSetup = Process.Start(ExecutablePath, string.Format("Parmfile=\"{0}\"", filename));

            axSetup.WaitForExit();

            return axSetup.ExitCode;
        }
    }
}
