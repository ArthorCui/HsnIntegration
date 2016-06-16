function Generate-Version-Info
{
param(
	[string]$version = $(throw "version is a required parameter."),
	[string]$file = $(throw "file is a required parameter.")
)
  $asmInfo = "using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion(""1.0.*"")]
[assembly: AssemblyVersion(""$version"")]
[assembly: AssemblyFileVersion(""$version"")]
"
	$dir = [System.IO.Path]::GetDirectoryName($file)
	if ([System.IO.Directory]::Exists($dir) -eq $false)
	{
		Write-Host "Creating directory $dir"
		[System.IO.Directory]::CreateDirectory($dir)
	}
	Write-Host "Generating assembly info file: $file"
	Write-Output $asmInfo > $file
}

function Generate-Assembly-Info
{
param(
	[string]$version = $(throw "version is a required parameter."),
	[string]$file = $(throw "file is a required parameter."),
	[string]$assemblyTitle,
	[string]$assemblyDescription,
	[string]$assemblyConfiguration,
	[string]$assemblyCompany,
	[string]$assemblyProduct,
	[string]$assemblyCopyright,
	[string]$assemblyTrademark,
	[string]$assemblyCulture,
	[string]$comVisible = "false",
	[string]$guid
)
  $asmInfo = "using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(""$assemblyTitle"")]
[assembly: AssemblyDescription(""$assemblyDescription"")]
[assembly: AssemblyConfiguration(""$assemblyConfiguration"")]
[assembly: AssemblyCompany(""$assemblyCompany"")]
[assembly: AssemblyProduct(""$assemblyProduct"")]
[assembly: AssemblyCopyright(""$assemblyCopyright"")]
[assembly: AssemblyTrademark(""$assemblyTrademark"")]
[assembly: AssemblyCulture(""$assemblyCulture"")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible($comVisible)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid(""$guid"")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion(""1.0.*"")]
[assembly: AssemblyVersion(""$version"")]
[assembly: AssemblyFileVersion(""$version"")]
"
	$dir = [System.IO.Path]::GetDirectoryName($file)
	if ([System.IO.Directory]::Exists($dir) -eq $false)
	{
		Write-Host "Creating directory $dir"
		[System.IO.Directory]::CreateDirectory($dir)
	}
	Write-Host "Generating assembly info file: $file"
	Write-Output $asmInfo > $file
}