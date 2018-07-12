# Dynamics AX Admin

This project and the utilities within will help you automate multiple facets of your Microsoft Dynamics AX 2012 installation. Each project comes with a class library that contains all the functionality, and PowerShell cmdlets supporting pipeline as well as WPF sample apps that wrap that functionality. Use PowerShell, the GUI, or use the class libraries in your own projects.

- The **Setup** utilities help you with silent mode installations, by managing the parameter files and passing them to the setup utility of Dynamics AX 2012 
- The **Config** utilities help you manage client and server configurations both in configuration files and registry settings. 
- The **Manage** utilities help you manage the object server service (stop/start) and manage the model store 
- The **Client** utilities help you automate client commands, such as compiling, synchronizing, generating CIL, AutoRun scripts, and much more 
- The **SQL** utilities help you restore an AX database and reset the global guid 
- The **AXBuild** utilities help you automate the axbuild commands (AX 2012 CU7 or higher) 
- The **TFS** custom XAML workflow activities wrap all the above utilities into custom workflow activities for use in automated builds in TFS

## VSTS Builds
[Usage for VSTS builds](docs/build-vnext.md)