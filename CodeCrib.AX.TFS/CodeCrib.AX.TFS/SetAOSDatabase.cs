//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;

namespace CodeCrib.AX.TFS
{
    public class SetAOSDatabase : CodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }
        [RequiredArgument]
        public InArgument<string> Database { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string configurationFile = ConfigurationFile.Get(context);
            string database = Database.Get(context);

            var serverConfig = CodeCrib.AX.Deploy.Configs.GetServerConfig(configurationFile);

            serverConfig.Database = database;

            CodeCrib.AX.Config.Server.SaveConfigToRegistry(serverConfig.AOSNumberOrigin, serverConfig);
        }
    }
}
