//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace CodeCrib.AX.Setup.PowerShell
{
    /// <summary>
    /// Opens a Microsoft Dynamics AX setup parameter file and returns a list of parameters
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Parameters", DefaultParameterSetName = "parameterfile")]
    public class Get_Parameters : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to file", Mandatory = true, ParameterSetName = "parameterfile")]
        [Parameter(ParameterSetName = "logfile")]
        public string filename;

        [Parameter(HelpMessage = "Flag to extract parameters from a log file", Mandatory = true, ParameterSetName = "logfile")]
        public SwitchParameter logfile;

        protected override void ProcessRecord()
        {
            switch (this.ParameterSetName)
            {
                case "parameterfile":
                    this.WriteObject(CodeCrib.AX.Setup.ParameterFile.Open(filename));
                    break;
                case "logfile":
                    this.WriteObject(CodeCrib.AX.Setup.LogParser.LogExtract(filename));
                    break;
            }
        }
    }

    /// <summary>
    /// Takes a list of parameters over pipeline and set a parameter to a specific value,
    /// or creates a new parameter in the list if it doesn't exist;
    /// Optionally update environment variables in the values with actual values from the system
    /// and returns the list of updated parameters
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "Parameter")]
    public class Set_Parameter : PSCmdlet
    {
        [Parameter(ValueFromPipeline = true, Mandatory = true)]
        public List<Parameter> parameters;

        [Parameter(Mandatory = true, HelpMessage = "Parameter to be updated or added", ParameterSetName = "updateparameter")]
        public string parameter;

        [Parameter(Mandatory = true, HelpMessage = "New value for parameter", ParameterSetName = "updateparameter")]
        public string value;

        [Parameter(Mandatory = true, HelpMessage = "Replace environment variables with actual values", ParameterSetName = "replaceenvvar")]
        public SwitchParameter EnvironmentVariables;

        List<Parameter> pipedData;

        protected override void BeginProcessing()
        {
            pipedData = new List<Parameter>();
        }

        protected override void ProcessRecord()
        {
            pipedData.AddRange(parameters);
        }

        protected override void EndProcessing()
        {
            switch (this.ParameterSetName)
            {
                case "updateparameter":
                    Parameter updateParameter = pipedData.FirstOrDefault(x => x.Name == parameter);

                    if (updateParameter != null)
                    {
                        updateParameter.Value = value;
                    }
                    else
                    {
                        updateParameter = new Parameter() { Name = parameter, Value = value, Enabled = true };
                    }
                    break;
                case "replaceenvvar":
                    foreach (Parameter parameter in pipedData)
                    {
                        parameter.Value = System.Environment.ExpandEnvironmentVariables(parameter.Value);
                    }
                    break;
            }

            this.WriteObject(pipedData);
        }
    }

    /// <summary>
    /// Launches the Microsoft Dynamics AX setup using a list of parameters or setup parameters file
    /// If list of parameters is used, the list is saved to a temporary file in %temp%
    /// </summary>
    [Cmdlet(VerbsLifecycle.Start, "AxSetup")]
    public class Start_AxSetup : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to Microsoft Dynamics AX setup executable", Mandatory = true, ParameterSetName = "paramfile")]
        [Parameter(Mandatory = true, ParameterSetName = "paramlist")]
        public string setupPath;

        [Parameter(HelpMessage = "List of parameters", ValueFromPipeline = true, Mandatory = true, ParameterSetName = "paramlist")]
        public List<Parameter> parameters;

        [Parameter(HelpMessage = "Path to file with parameters", Mandatory = true, ParameterSetName = "paramfile")]
        public string filename;

        List<Parameter> pipedData;

        protected override void BeginProcessing()
        {
            pipedData = new List<Parameter>();
        }

        protected override void ProcessRecord()
        {
            if (parameters != null)
            {
                pipedData.AddRange(parameters);
            }
        }

        protected override void EndProcessing()
        {
            CodeCrib.AX.Setup.Setup setup = new CodeCrib.AX.Setup.Setup();
            setup.ExecutablePath = setupPath;

            switch (this.ParameterSetName)
            {
                case "paramlist":
                    setup.Run(pipedData);
                    break;
                case "paramfile":
                    setup.Run(filename);
                    break;
            }
        }
    }
}
