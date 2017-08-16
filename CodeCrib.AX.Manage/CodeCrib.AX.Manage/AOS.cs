//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.ServiceProcess;

namespace CodeCrib.AX.Manage
{
    public class AOS
    {
        uint serviceNumber;
        ServiceController service = null;

        public AOS(uint serviceNumber)
        {
            this.serviceNumber = serviceNumber;
            service = new ServiceController(string.Format("AOS60${0}", serviceNumber.ToString("D2")));
        }

        public Boolean IsRunning
        {
            get
            {
                if (service.Status == ServiceControllerStatus.Running)
                    return true;

                return false;
            }
        }

        public string Status
        {
            get
            {
                return service.Status.ToString();
            }
        }

        public void Start(int timeOutMinutes = 0)
        {
            if (service.Status == ServiceControllerStatus.Running)
                return;

            if (service.Status != ServiceControllerStatus.StartPending)
                service.Start();

            if (timeOutMinutes > 0)
                service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, timeOutMinutes, 0));
            else
                service.WaitForStatus(ServiceControllerStatus.Running);
        }

        public void Stop(int timeOutMinutes = 0)
        {
            if (service.Status == ServiceControllerStatus.Stopped)
                return;

            if (service.Status != ServiceControllerStatus.StopPending)
                service.Stop();

            if (timeOutMinutes > 0)
                service.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, timeOutMinutes, 0));
            else
                service.WaitForStatus(ServiceControllerStatus.Stopped);
        }
    }
}
