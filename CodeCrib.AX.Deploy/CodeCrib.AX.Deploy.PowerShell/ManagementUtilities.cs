using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace CodeCrib.AX.Deploy.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "ManagementFolder")]
    public class Get_ManagementFolder : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            this.WriteObject(ManagementUtilities.InstallDir());
        }
    }
}
