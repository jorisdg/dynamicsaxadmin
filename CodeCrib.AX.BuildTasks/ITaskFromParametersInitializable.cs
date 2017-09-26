using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Obsolete("Tasks marshal by value through [Serializable] attribute", true)]
    public interface ITaskFromParametersInitializable
    {
        void InitializeFromParameters(ModelStoreTaskParameters parameters);
    }
}
