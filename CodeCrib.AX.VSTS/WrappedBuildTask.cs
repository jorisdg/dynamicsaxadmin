using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    internal class WrappedBuildTask<TTask> : MarshalByRefObject where TTask:BuildTask,new()
    {
        protected TTask task;

        public TTask Task
        {
            get
            {
                return task;
            }
            set
            {
                task = value;
            }
        }

        public WrappedBuildTask()
        {
            task = new TTask();
        }

        public void RunWithParameters(ModelStoreTaskParameters parameters)
        {
            if (task is ITaskFromParametersInitializable iTask)
            {
                iTask.InitializeFromParameters(parameters);
            }
            task.Run();
        }

        public void Run()
        {
            task.Run();
        }
    }
}
