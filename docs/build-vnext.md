# Build Agent
## Identity
Configure the build agent to run as a service using a Windows Active Directory identity.  This identity is used to access resources during the build:
* Log on to the AX environment
* Start and stop the AOS service
* Prepare build databases in the SQL Server instance
* Access network shared resources such as database backup files

## PowerShell Module
Copy build output of the CodeCrib.AX.VSTS project to ```~\Documents\WindowsPowerShell\CodeCrib.AX.VSTS``` for the build agent identity.

# Build Definition
## Template
Import the build definition template ```template/AX2012 VSTS build.json```

## Variables
Each of these variables in the build definition is prefixed with CodeCrib.Build

Variable|Description|Examples
--|--|--
AdditionalModelPaths|Optional, PowerShell array of axmodel file paths to be imported during the build|```@('$(Build.SourcesDirectory)\ExtraModels\Model1.axmodel','$(Build.SourcesDirectory)\ExtraModels\Model2.axmodel')```
AOTCompilePaths|Optional, PowerShell array of AOT nodes to compile using the client prior to running axbuild.  AOT paths use the format provided from the development environment's right click menu: Add-Ins, Copy, Entire path|```@('\Data Dictionary\Tables\Table1','\Classes\Class1')```
AxBuildAlternateBinaryFolder|Optional, folder path passed as the ```altbin``` parameter to axbuild|
AxBuildWorkers|Optional, number of worker processes passed to axbuild|
AxUtilBinaryFolderPath|Optional, folder path of management utilities to use instead of the system installed version.  Applies to model export step and is useful to stamp the resulting axmodel file with a different version|
ConfigurationFile|Optional, client configuration file to use when running client activities|```$(Build.SourcesDirectory)\Configuration.axc```
DatabaseBackupFilePath|Backup file to restore as the business database|```\\fileserver\share\backups\database.bak```
DatabaseName|The working database to use during the build.  Will be overwritten if a backup is restored.|
DatabaseServerName|Database server, can include an instance name|```localhost```<br/>```localhost\INSTANCE```
IncludeXpoArtifact|Should the combined XPO be included in the build output?|```false```<br/>```true```
LayerCodes|PowerShell array of development layer access codes.  Required for models using layers other than ```USR``` or ```USP```|```@('var:VarLayerAccessCode','cus:CusLayerAccessCode')```
ModelDescription|Text applied to the description field of the resulting model|```$(Build.DefinitionName)```
ModelstoreDatabaseBackupFilePath|Backup file to restore as the model store database|```\\fileserver\share\backups\database_model.bak```
ModelVersion|Text applied to the version number of the resulting model.  Must fit the [.NET Framework assembly number format](https://docs.microsoft.com/en-us/dotnet/framework/app-domains/assembly-versioning#assembly-version-number).|```$(Build.BuildNumber)```
ReferencesFolder|Folder path containing .NET assembly files to make available for use during the build.|```$(Build.SourcesDirectory)\AdditionalAssemblies```

## Source Mappings
Map the model folder to the root local path (```$(Build.SourcesDirectory)```).  Optionally add mappings for additional assembly references and models.

### Example Mappings (TFVC)
Type|Server path|Local path under $(build.sourcesDirectory)
--|--|--
Map|$/ProjectName/BranchName/ModelFolderName|
Map|$/ProjectName/BranchName/Assemblies|AdditionalAssemblies
Map|$/ProjectName/BranchName/ExtraModels|ExtraModels

