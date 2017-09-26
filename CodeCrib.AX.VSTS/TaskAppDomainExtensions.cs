using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    public static class TaskAppDomainExtensions
    {
        public static void RunInAppDomain<TTask>(this TTask task) where TTask:BuildTask,new()
        {
            using (var taskWrapper = AppDomainWrapper<TTask>.Create(task))
            {
                taskWrapper.Facade.Run();
            }
        }
    }
}
