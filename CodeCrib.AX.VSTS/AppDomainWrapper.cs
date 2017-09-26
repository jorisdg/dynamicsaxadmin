using CodeCrib.AX.BuildRuntime;
using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    internal class AppDomainWrapper<TTask> : IDisposable where TTask : BuildTask, new()
    {
        private AppDomain appDomain;
        private WrappedBuildTask<TTask> facade;

        protected AppDomainWrapper()
        {
            Type facadeType = typeof(WrappedBuildTask<TTask>);
            AppDomainSetup setupInformation = new AppDomainSetup();
            setupInformation.ApplicationBase = Path.GetDirectoryName(facadeType.Assembly.Location);
            appDomain = AppDomain.CreateDomain(string.Format("Build_{0}", Guid.NewGuid()), null, setupInformation);
            appDomain.AssemblyResolve += AssemblyResolver.ResolveEventHandler;
            facade = (WrappedBuildTask<TTask>)appDomain.CreateInstanceFromAndUnwrap(facadeType.Assembly.Location, facadeType.FullName);
        }

        public WrappedBuildTask<TTask> Facade
        {
            get
            {
                return facade;
            }
        }

        public static void Run(IBuildLogger buildLogger, ModelStoreTaskParameters parameters)
        {
            using (AppDomainWrapper<TTask> wrapper = new AppDomainWrapper<TTask>())
            {
                parameters.BuildLogger = VSTSBuildLogger.CreateDefault();
                wrapper.Facade.RunWithParameters(parameters);
            }
        }

        public static AppDomainWrapper<TTask> Create()
        {
            return new AppDomainWrapper<TTask>();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (appDomain != null)
                    {
                        AppDomain.Unload(appDomain);
                        appDomain = null;
                    }
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AppDomainWrapper() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
