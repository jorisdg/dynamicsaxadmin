using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    public interface ITaskFromParametersInitializable
    {
        void InitializeFromParameters(ModelStoreTaskParameters parameters);
    }
}
