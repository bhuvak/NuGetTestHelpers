using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using NuGet;

namespace NuGetTestHelper
{
    /// <summary>
    /// This class encapsulates helper methods to create nuspec file, update a given nuspec file and to create a package.
    /// </summary>
    public class NugetProcessUtility
    {       
        /// <summary>
        /// Given the full path to the package analyzes the package and returns the analysis warnings/errors.
        /// </summary>
        /// <param name="PackageFullPath"></param>
        /// <param name="destinationPath"></param>
        public static string AnalyzeNugetPackage(string PackageFullPath)
        {
            string standardOutput = string.Empty;
            string standardError = string.Empty;
            InvokeNugetProcess(string.Join(string.Empty, new string[] { AnalyzeCommandString, @"""", PackageFullPath, @"""" }), out standardError, out standardOutput,null);
            return standardOutput;         
        }             

        #region PrivateMethods

        private static int InvokeNugetProcess(string arguments, out string standardError, out string standardOutput,string WorkingDir=null)
        {
            Process nugetProcess = new Process();
            ProcessStartInfo nugetProcessStartInfo = new ProcessStartInfo(Path.Combine(@"C:\Nuget", NugetExePath));
            nugetProcessStartInfo.Arguments = arguments;
            nugetProcessStartInfo.RedirectStandardError = true;
            nugetProcessStartInfo.RedirectStandardOutput = true;
            nugetProcessStartInfo.RedirectStandardInput = true;
            nugetProcessStartInfo.UseShellExecute = false;
            nugetProcess.StartInfo = nugetProcessStartInfo;
            nugetProcess.StartInfo.WorkingDirectory = WorkingDir;
            nugetProcess.Start();            standardError = nugetProcess.StandardError.ReadToEnd();
            standardOutput = nugetProcess.StandardOutput.ReadToEnd();            
            nugetProcess.WaitForExit();
            return nugetProcess.ExitCode;           
        }

        #endregion PrivateMethods

        #region PrivateMemebers
        internal static string AnalyzeCommandString = " analyze ";
        internal static string SpecCommandString = " spec -f ";
        internal static string PackCommandString = " pack ";
        internal static string UpdateCommandString = " update ";
        internal static string InstallCommandString = " install ";
        internal static string OutputDirectorySwitchString = " -OutputDirectory ";
        internal static string PreReleaseSwitchString = " -Prerelease ";
        internal static string SourceSwitchString = " -Source ";
        internal static string ExcludeVersionSwitchString = " -ExcludeVersion ";
        internal static string NugetExePath = @"NuGet.exe";            
        #endregion PrivateMemebers

    }


   
}
