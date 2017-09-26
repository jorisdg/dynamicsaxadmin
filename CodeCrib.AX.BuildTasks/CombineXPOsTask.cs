using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public class CombineXPOsTask : BuildTask
    {
        protected string Folder;
        protected bool Recursive;
        protected string CombinedXPOFile;
        protected bool IncludeSystemObjects;
        protected bool IncludeNonSystemObjects;

        public CombineXPOsTask(
            IBuildLogger buildLogger, 
            string folder, 
            bool recursive, 
            string combinedXPOFile, 
            bool includeSystemObjects, 
            bool includeNonSystemObjects) : base(buildLogger)
        {
            Folder = folder;
            Recursive = recursive;
            CombinedXPOFile = combinedXPOFile;
            IncludeSystemObjects = includeSystemObjects;
            IncludeNonSystemObjects = includeNonSystemObjects;
        }

        public override void Run()
        {
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(CombinedXPOFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(CombinedXPOFile));

            if (IncludeNonSystemObjects && IncludeSystemObjects)
                Client.CombineXPOs.Combine(Folder, Recursive, CombinedXPOFile, Client.CombineXPOs.IncludeObjects.AllObjects);
            else if (IncludeNonSystemObjects && !IncludeSystemObjects)
                Client.CombineXPOs.Combine(Folder, Recursive, CombinedXPOFile, Client.CombineXPOs.IncludeObjects.ExcludeSystemObjects);
            else if (!IncludeNonSystemObjects && IncludeSystemObjects)
                Client.CombineXPOs.Combine(Folder, Recursive, CombinedXPOFile, Client.CombineXPOs.IncludeObjects.SystemObjectsOnly);
            // else neither
        }
    }
}
