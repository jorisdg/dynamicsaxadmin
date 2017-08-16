using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace CodeCrib.AX.Manage.PowerShell
{
    [Cmdlet(VerbsLifecycle.Start, "AOS")]
    public class Start_AOS : PSCmdlet
    {
        [Parameter(HelpMessage = "AOS Number", Mandatory = true)]
        public uint aosNumber;

        [Parameter(HelpMessage = "Timeout (in minutes)", Mandatory = false)]
        public int timeOutMinutes;

        protected override void ProcessRecord()
        {
            CodeCrib.AX.Manage.AOS aos = new AOS(aosNumber);

            aos.Start(timeOutMinutes);
        }
    }

    [Cmdlet(VerbsLifecycle.Stop, "AOS")]
    public class Stop_AOS : PSCmdlet
    {
        [Parameter(HelpMessage = "AOS Number", Mandatory = true)]
        public uint aosNumber;

        [Parameter(HelpMessage = "Timeout (in minutes)", Mandatory = false)]
        public int timeOutMinutes;

        protected override void ProcessRecord()
        {
            CodeCrib.AX.Manage.AOS aos = new AOS(aosNumber);

            aos.Stop(timeOutMinutes);
        }
    }

    [Cmdlet(VerbsCommon.Get, "AOSStatus")]
    public class Get_AOSStatus : PSCmdlet
    {
        [Parameter(HelpMessage = "AOS Number", Mandatory = true)]
        public uint aosNumber;

        protected override void ProcessRecord()
        {
            CodeCrib.AX.Manage.AOS aos = new AOS(aosNumber);
            
            this.WriteObject(aos.Status);
        }
    }
}
