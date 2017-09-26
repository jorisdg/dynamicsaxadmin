using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    [Obsolete("Tasks marshal by value through [Serializable] attribute", true)]
    public class ModelStoreTaskParameters
    {
        public IBuildLogger BuildLogger { get; set; }

        public int TimeoutMinutes { get; set; }

        public string ConfigurationFile { get; set; }
        
        public string ModelManifest { get; set; }

        public string VersionOverride { get; set; }

        public string DescriptionOverride { get; set; }

        public bool SetNoInstallMode { get; set; }
    }
}
