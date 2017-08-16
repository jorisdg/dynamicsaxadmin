//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeCrib.AX.Client.Commands
{
    public class Command
    {
        public bool Minimize { get; set; }
        public bool LazyClassLoading { get; set; }
        public bool LazyTableLoading { get; set; }
        public bool NoCompileOnImport { get; set; }
        public bool NoModalBoxes { get; set; }

        public bool Development { get; set; }

        public string Layer { get; set; }
        public string LayerCode { get; set; }
        public string Model { get; set; }
        public string ModelPublisher { get; set; }
        public string ConfigurationFile { get; set; }

        virtual public List<string> Parameters()
        {
            List<string> parameters = new List<string>();

            if (!string.IsNullOrEmpty(ConfigurationFile))
                parameters.Add(string.Format("\"{0}\"", ConfigurationFile));

            if (Minimize)
                parameters.Add("-MINIMIZE");
            if (LazyClassLoading)
                parameters.Add("-LAZYCLASSLOADING");
            if (LazyTableLoading)
                parameters.Add("-LAZYTABLELOADING");
            if (NoCompileOnImport)
                parameters.Add("-NOCOMPILEONIMPORT");
            if (NoModalBoxes)
                parameters.Add("-INTERNAL=NoModalBoxes");

            if (Development)
                parameters.Add("-Development");

            if (!string.IsNullOrEmpty(Layer))
                parameters.Add(string.Format("-aol={0}", Layer));
            if (!string.IsNullOrEmpty(LayerCode))
                parameters.Add(string.Format("-aolcode={0}", LayerCode));
            if (!string.IsNullOrEmpty(Model))
            {
                if (!string.IsNullOrEmpty(ModelPublisher))
                    parameters.Add(string.Format("\"-Model=({0},{1})\"", Model, ModelPublisher));
                else
                    parameters.Add(string.Format("\"-Model={0}\"", Model));
            }

            return parameters;
        }
    }

    public class ImportXPO : Command
    {
        public string Filename { get; set; }

        public override List<string> Parameters()
        {
            List<string> parameters = base.Parameters();

            parameters.Add(string.Format("\"-AOTIMPORTFILE={0}\"", Filename));

            return parameters;
        }
    }

    public class ImportLabelFile : Command
    {
        public string Filename { get; set; }

        public override List<string> Parameters()
        {
            List<string> parameters = base.Parameters();

            parameters.Add(string.Format("\"-StartupCmd=aldimport_{0}\"", Filename));

            return parameters;
        }
    }

    public class Synchronize : Command
    {
        public override List<string> Parameters()
        {
            List<string> parameters = base.Parameters();

            parameters.Add("-StartupCmd=Synchronize");

            return parameters;
        }
    }

    public class Compile : Command
    {
        public bool UpdateCrossReference { get; set; }

        public override List<string> Parameters()
        {
            List<string> parameters = base.Parameters();

            parameters.Add(string.Format("-StartupCmd=CompileAll{0}", UpdateCrossReference ? "_+" : ""));

            return parameters;
        }
    }

    public class GenerateCIL : Command
    {
        public override List<string> Parameters()
        {
            List<string> parameters = base.Parameters();

            parameters.Add("-StartupCmd=CompileIL");

            return parameters;
        }
    }

    public class RunTestProject : Command
    {
        public string ProjectName { get; set; }

        public override List<string> Parameters()
        {
            List<string> parameters = base.Parameters();

            parameters.Add(string.Format("-StartupCmd=RunTestProject_{0}", ProjectName));

            return parameters;
        }
    }

    public class XMLDocumentation : Command
    {
        public string Filename { get; set; }

        public override List<string> Parameters()
        {
            List<string> parameters = base.Parameters();

            parameters.Add(string.Format("\"-StartupCmd=xmldocumentation_{0}\"", Filename));

            return parameters;
        }
    }

    public class XMLReflection : Command
    {
        public string Filename { get; set; }

        public override List<string> Parameters()
        {
            List<string> parameters = base.Parameters();

            parameters.Add(string.Format("\"-StartupCmd=xmlreflection_{0}\"", Filename));

            return parameters;
        }
    }
    
    public class CheckBestPractices : Command
    {
        public override List<string> Parameters()
        {
            List<string> parameters = base.Parameters();

            parameters.Add(string.Format("-StartupCmd=checkbestpractices"));

            return parameters;
        }
    }

    public class AutoRun : Command
    {
        public string Filename { get; set; }

        public override List<string> Parameters()
        {
            List<string> parameters = base.Parameters();

            parameters.Add(string.Format("\"-StartupCmd=AutoRun_{0}\"", Filename));

            return parameters;
        }
    }

    public class StartupCommand : Command
    {
        public string Command { get; set; }

        public override List<string> Parameters()
        {
            List<string> parameters = base.Parameters();

            parameters.Add(string.Format("\"-StartupCmd={0}\"", Command));

            return parameters;
        }
    }
}
