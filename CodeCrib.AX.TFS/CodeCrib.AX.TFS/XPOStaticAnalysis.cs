using CodeCrib.AX.Client;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.TFS
{    
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class XPOStaticAnalysisCheck : CodeActivity
    {
        [RequiredArgument]
        public InArgument<string> XpoFilename { get; set; }        

        protected override void Execute(CodeActivityContext context)
        {
            XPOStaticAnalysis staticAnalysis = new XPOStaticAnalysis();            
            staticAnalysis.analyzeXPO(XpoFilename.Get(context));

            Boolean failBuild = false;

            foreach (KeyValuePair<string, int> tempLabel in staticAnalysis.TemporaryLabelDictionary)
            {
                context.TrackBuildError(String.Format("{0} temporary labels found in {1}.", tempLabel.Value, tempLabel.Key));
            }

            if (staticAnalysis.DuplicateOriginDictionary.Count() > 0)
            {
                foreach (KeyValuePair<string, List<string>> origin in staticAnalysis.DuplicateOriginDictionary)
                {
                    context.TrackBuildError(String.Format("(fatal) Origin Id {0} is used on multiple objects: \n{1}", origin.Key, String.Join("\n", origin.Value.ToArray())));
                }
                failBuild = true;                
            }       
            
            if (failBuild)
            {
                throw new Exception("XPO validation failed.");
            }
        }
    }    
}
