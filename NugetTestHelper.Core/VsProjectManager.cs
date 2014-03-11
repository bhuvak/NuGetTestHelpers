using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using System.IO;
using VSLangProj;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Win32;
using System.Diagnostics;


namespace NuGetTestHelper
{
    /// <summary>
    /// Provides Utility methods to create projects based on appropriate template and .Net framework using the Visual Studio DTE.
    /// This class is loosely coupled with other classes in the library to enable stand-alone usage of this class.
    /// </summary>
    public class VsProjectManager : IDisposable
    {
        #region PublicMethods
       /// <summary>
       /// Launches VS with the specific version and SKU.
       /// </summary>
       /// <param name="VsVersion"></param>
       /// <param name="VsSKU"></param>
        public void LaunchVS(VSVersion VsVersion, VSSKU VsSKU)
        {
            if (!IsSkuInstalled(VsVersion, vsSKU))
                throw new Exception(string.Format("The SKU with version {0} and SKU name {1} is not present in the machine. Please specify a different SKU or install it." ,VsVersion, vsSKU));
            this.vsVersion = VsVersion;
            this.vsSKU = VsSKU;
            LaunchVSInternal();
        }

        /// <summary>
        /// Kill all stale VS entries left behind after running tests..
        /// </summary>
        public void CloseAllSkus()
        {
            Console.WriteLine("Closing all instances Open instances of VS to avoid stale instances..");
            try
            {
                System.Diagnostics.Process[] allProcesses = System.Diagnostics.Process.GetProcesses();
                foreach (System.Diagnostics.Process proc in allProcesses)
                {
                    if (proc.ProcessName.Contains("devenv") || proc.ProcessName.Contains("VWDExpress") || proc.ProcessName.Contains("VSWinExpress"))
                    {
                       proc.Kill();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception while killing devenv.exe", e.Message);
            }
        }

        /// <summary>
        /// Launches the default VS SKU and version based on the installations in the current machine.
        /// </summary>
        public void LaunchDefaultVsSku()
        {
            GetDefaultSKU(out this.vsVersion, out this.vsSKU);
            LaunchVSInternal();
        }

        /// <summary>
        /// Creates a new project based on the given projectTemplateName, Language and framework in the specified path.
        /// </summary>
        /// <param name="projectTemplate"></param>
        /// <param name="framework"></param>
        /// <param name="solnPath"></param>
        public void CreateProject(string projectTemplateName,string projectLanguage,string projectTargetFramework, string projectName, string solnFullPath=null)
        {            
            try
            {
                if (solnFullPath == null)
                    solnFullPath = Path.Combine(Environment.CurrentDirectory, "Solution" + DateTime.Now.Ticks.ToString());
                this.SolutionPath = solnFullPath;
                Solution2 soln = dteObject.Solution as Solution2;                                
               //setting the project location is required due to Nuget bug # 2917 : http://nuget.codeplex.com/workitem/2917
                Properties prop = dteObject.Properties["Environment", "ProjectsAndSolution"];
                prop.Item("ProjectsLocation").Value = Path.GetFullPath(Path.GetDirectoryName(solnFullPath));
                string templatepath = soln.GetProjectTemplate(projectTemplateName, projectLanguage);
                soln.AddFromTemplate(templatepath + projectTargetFramework, solnFullPath, projectName);              
            }
            catch (Exception e)
            {             
                throw new Exception(string.Format("Unable to create project with template {0}. Make sure that the template is valid and the template file exists. Exception message : {1}", projectTemplateName.ToString(), e.Message));
            }           
        }

        /// <summary>
        /// Creates a new project based on the given full path to the project template.
        /// </summary>
        /// <param name="projectTemplate"></param>
        /// <param name="framework"></param>
        /// <param name="solnPath"></param>
        public void CreateProject(string projectTemplateFullPath, string projectTargetFramework, string projectName, string solnFullPath = null)
        {
            try
            {
                if (solnFullPath == null)
                    solnFullPath = Path.Combine(Environment.CurrentDirectory, "Solution" + DateTime.Now.Ticks.ToString());
                this.SolutionPath = solnFullPath;
                Solution2 soln = dteObject.Solution as Solution2;               
                //setting the project location is required due to Nuget bug # 2917 : http://nuget.codeplex.com/workitem/2917
                Properties prop = dteObject.Properties["Environment", "ProjectsAndSolution"];
                prop.Item("ProjectsLocation").Value = Path.GetFullPath(Path.GetDirectoryName(solnFullPath));
                soln.AddFromTemplate(projectTemplateFullPath + projectTargetFramework, solnFullPath, projectName);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Unable to create project with template {0}. Make sure that the template is valid and the template file exists. Exception message : {1}", projectTemplateFullPath.ToString(), e.Message));
            }
        }


        /// <summary>
        /// Opens an existing solution.
        /// </summary>
        /// <param name="solutionFullPath"></param>
        public void OpenExistingSolution(string solutionFullPath)
        {
            if (!File.Exists(solutionFullPath))
                throw new Exception(string.Format("The specified solution {0} doesn't exist", solutionFullPath));

            this.SolutionPath = Path.GetFullPath(Path.GetDirectoryName(solutionFullPath));
            try
            {
                dteObject.Solution.Open(solutionFullPath);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Unable to open solution : {0}. Exception message :{1}", solutionFullPath, e.Message));
            }
        }              

        /// <summary>
        /// Returns the list of reference assemblies present in the current active project.
        /// </summary>
        /// <returns></returns>
        public  Dictionary<string,string> GetReferences(string projectName=null)
        {
            Dictionary<string,string> referenceNames = new Dictionary<string,string>();      
            Project currentProject = null;
            //if project name is not specified get the current active project.
            if (string.IsNullOrEmpty(projectName))
            {
                currentProject = GetActiveProject();
            }
            else
            {
                currentProject = GetProjectByName(projectName);
            }
            if (currentProject == null)
                return null;
            currentProject.Save();

            VSProject vsProj = currentProject.Object as VSProject;
            if (vsProj == null)
                return null;

            foreach (Reference reference in vsProj.References)
            {               
                referenceNames.Add(reference.Name, reference.Version);
            }
            return referenceNames;
        }

        /// <summary>
        /// Returns the list of reference assemblies present in the current active project.
        /// </summary>
        /// <returns></returns>
        public Reference GetReferenceByName(string referenceName,string projectName=null)
        {           
            Project currentProject = null;
            //if project name is not specified get the current active project.
            if (string.IsNullOrEmpty(projectName))
            {
                currentProject = GetActiveProject();
            }
            else
            {
                currentProject = GetProjectByName(projectName);
            }
            if (currentProject == null)
                return null;
            currentProject.Save();

            //var vsRef = currentProject.Object.References.Item(referenceName);
            //return vsRef;
            //Console.WriteLine("Embeded interop : " + vsRef.EmbedInteropTypes);

            VSProject vsProj = currentProject.Object as VSProject;
            if (vsProj == null)
                return null;
            return vsProj.References.Find(referenceName);        
        }       

        /// <summary>
        /// Returns the .net framework of the current active project of the solution.
        /// </summary>
        /// <returns></returns>
        public string GetProjectFramework()
        {
            return dteObject.Solution.Projects.Item(1).Properties.Item("TargetFrameworkMoniker").Value;
        }

        /// <summary>
        /// Returns the caption of the current window that is active inside VS.
        /// </summary>
        /// <returns></returns>
        public string GetActiveWindowName()
        {
            return dteObject.ActiveWindow.Caption.ToString();
        }      

        /// <summary>
        /// Builds the solution and returns the build status.
        /// </summary>
        /// <param name="buildOutput"></param>
        /// <returns></returns>
        public int GetSolutionBuildStatus(out string buildOutput)
        {
            dteObject.Solution.SolutionBuild.Build(true);
            //Make sure that the build completes.
            while (dteObject.Solution.SolutionBuild.BuildState.Equals(vsBuildState.vsBuildStateInProgress))
                System.Threading.Thread.Sleep(1 * 1000);
            buildOutput = GetTextFromOutputPane("Build");
            return dteObject.Solution.SolutionBuild.LastBuildInfo;
        }

        /// <summary>
        /// Disposes the current DTE instance.
        /// </summary>
        public void CloseSolution()
        {
            if (dteObject != null)
            dteObject.ExecuteCommand("File.Exit");
        }

        /// <summary>
        /// Disposes the current DTE instance.
        /// </summary>
        public void Dispose()
        {
            if(dteObject != null)
             dteObject.Quit();            
        }

        #endregion PublicMethods

        #region PrivateMethods    
 
        
        [DllImport("User32.dll")]
        private static extern Int32 SetForegroundWindow(int hWnd);

        [DllImport("user32.dll")]
        private static extern int FindWindow(string lpClassName, string lpWindowName);


        private void LaunchVSInternal()
        {
            Type visualStudioType = Type.GetTypeFromProgID(GetProgIDForVSVersion(this.vsVersion, this.vsSKU));
            dteObject = Activator.CreateInstance(visualStudioType) as DTE2;
            System.Threading.Thread.Sleep(10 * 1000);
            dteObject.MainWindow.Visible = true;
            System.Threading.Thread.Sleep(10 * 1000);
            dteObject.MainWindow.Activate();
            dteObject.MainWindow.WindowState = vsWindowState.vsWindowStateMaximize;
            //Invoke SetForeGround to bring the VS window on top. Though it is not required, just setting it as foreground to be on safer side.
            SetForegroundWindow(FindWindow(null, dteObject.MainWindow.Caption));
        }    

        /// <summary>
        /// Installs the given package on the current project.
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="version"></param>
        /// <param name="path"></param>
        internal bool InstallPackage(string packageId, string version, int timeOut = 10 * 60 * 1000, bool update=false)
        {
            //Add enough sleeps to let the solution and package manager console to initialize before installing the package.
            System.Threading.Thread.Sleep(10 * 1000);
            SafeExecuteCommand("View.PackageManagerConsole");
            System.Threading.Thread.Sleep(10 * 1000);
            dteObject.ActiveWindow.Activate();
            //clear existing content from the console.
            SafeExecuteCommand("View.PackageManagerConsole clear-Host");           
            //Invoke the install script. Enclosing quotes are required in case if the path contains spaces.
            string installCommand = @"& " + @"""" + Path.Combine(Environment.CurrentDirectory, @"Install.ps1") + @"""" + @" " + @"""" + packageId + @"""" + " " + @"""" + update.ToString() + @"""";// +" -Version " + version + " -pre";
            SafeExecuteCommand("View.PackageManagerConsole " + installCommand);
            //Wait till package installation completes or the timeout exceeds.
            int waitTime = 0;
            while ( !File.Exists(GetResultFile(packageId)) && !File.Exists(GetResultFile(packageId,"Fail.txt")) && (waitTime < timeOut) )
            {
                //Check for the results file which the install package script would create on success or failure.
                //This is required as DTE.ExecuteCommand is asynchronous and there is no way to raise an event when the operation completes.
                System.Threading.Thread.Sleep(30 * 1000);
                waitTime += 30 * 1000;
            }
            SafeExecuteCommand("View.PackageManagerConsole clear-Host");
            System.Threading.Thread.Sleep(3 * 1000);
            SafeExecuteCommand("File.SaveAll");
            return IsPackageInstalled(packageId);
        }


        private bool IsPackageInstalled(string packageId)
        {
            //Checks the pass.txt file that would have been created by the installpackage powershell script.
            if (File.Exists(GetResultFile(packageId)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private string GetResultFile(string packageId, string resultFile = "Pass.txt")
        {
            return Path.Combine(this.SolutionPath, packageId + resultFile);
        }

        public Project GetActiveProject()
        {
            Array activeSolutionProjects = dteObject.ActiveSolutionProjects as Array;
            if (activeSolutionProjects != null && activeSolutionProjects.Length > 0)
            {
                return activeSolutionProjects.GetValue(0) as Project;
            }
            else
            {
                return null;
            }
        }

        private Project GetProjectByName(string projectName)
        {
            var item = dteObject.Solution.Projects.GetEnumerator();
            while (item.MoveNext())
            {
                var project = item.Current as EnvDTE.Project;
                if (project.Name.Equals(projectName))
                {
                    return project;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the text from the specified pane of the VS OutPut Window.
        /// </summary>
        /// <param name="paneName"></param>
        /// <returns></returns>
        private string GetTextFromOutputPane(string paneName)
        {
            dteObject.ToolWindows.OutputWindow.Parent.AutoHides = false;
            OutputWindowPane outputPane = dteObject.ToolWindows.OutputWindow.OutputWindowPanes.Item(paneName);
            outputPane.Activate();
            outputPane.TextDocument.Selection.StartOfDocument(false);
            outputPane.TextDocument.Selection.EndOfDocument(true);
            return dteObject.ToolWindows.OutputWindow.ActivePane.TextDocument.Selection.Text;

        }

        /// <summary>
        /// Returns the trace logs for package install command.
        /// </summary>
        /// <returns></returns>
        internal string GetPackageInstallationOutput()
        {
            return GetTextFromOutputPane("Package Manager");
        }

        private void SafeExecuteCommand(string command)
        {
            System.Threading.Thread.Sleep(1 * 1000);
            int retryCount = 0;
            bool commandExecuted = false;
            while (commandExecuted == false && retryCount < 5)
            {
                retryCount++;
                try
                {
                    dteObject.ExecuteCommand(command);
                    commandExecuted = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    System.Threading.Thread.Sleep(5 * 1000);
                }
            }
        }

        private string GetProgIDForVSVersion(VSVersion vsVersion,VSSKU vsSKU)
        {
            string version = GetVersionString(vsVersion);
            string Sku = GetSkuString(vsSKU);

            return Sku + ".DTE." + version;
           
        }

        private string GetSkuString(VSSKU vsSKU)
        {
            switch (vsSKU)
            {
                case VSSKU.VSU:
                    return VSUSkUString;
                case VSSKU.VWDExpress:
                    return VWDExpressSkUString;
                case VSSKU.WDExpress:
                    return WDExpressSkUString;
                case VSSKU.Win8Express:
                    return Win8ExpressSkUString;
                default:
                    return VSUSkUString;
            }
        }

        private string GetVersionString(VSVersion vsVersion)
        {
            switch (vsVersion)
            {
                case VSVersion.VS2013:
                    return VS2013VersionString;
                case VSVersion.VS2012:
                    return VS2012VersionString;
                case VSVersion.VS2010:
                    return VS2010VersionString;
                default:
                    return VS2012VersionString;
            }
        }       
        
        private bool IsSkuInstalled(VSVersion version, VSSKU sku)
        {
            string[] subkeys = Registry.LocalMachine.OpenSubKey(GetSkuInstallRegKeyPath(version,sku)).GetSubKeyNames();
            return (subkeys != null && subkeys.Length > 0);           
        }

        private string GetSkuInstallRegKeyPath(VSVersion version, VSSKU sku)
        {
          return Path.Combine( @"Software\Microsoft", GetSkuString(sku) , GetVersionString(version), @"Setup\VS");
        }

        private void GetDefaultSKU(out VSVersion defaultVersion, out VSSKU defaultSku)
        {
            foreach (var version in Enum.GetValues(typeof(VSVersion)))
            {
                foreach (var sku in Enum.GetValues(typeof(VSSKU)))
                {
                    if(IsSkuInstalled( (VSVersion)version, (VSSKU)sku))
                    {
                        defaultVersion = (VSVersion) version;
                        defaultSku =  (VSSKU) sku;
                        return;
                    }
                }

            }
            throw new Exception("No VS SKU installed in this machine. Please make sure to install any SKU of VS to continue");
        }

        #endregion PrivateMethods

        #region PrivateVariables
             
        #region VSSKUs
        private const string VSUSkUString= "VisualStudio";
        private const string WDExpressSkUString = "wdexpress";
        private const string VWDExpressSkUString = "vwdexpress";
        private const string Win8ExpressSkUString = "vswinexpress";
        private const string VS2013VersionString = "12.0";
        private const string VS2012VersionString = "11.0";
        private const string VS2010VersionString = "10.0";
        private const string DTEString = "DTE";
    
        #endregion VSSKUs

        private VSVersion vsVersion = VSVersion.VS2012;
        private VSSKU vsSKU = VSSKU.VSU;
        private DTE2 dteObject;

        public DTE2 DteObject
        {
            get { return dteObject; }
            private set { dteObject = value; }
        }
        private string solutionPath;

        public string SolutionPath
        {
            get { return solutionPath; }
           private set { solutionPath = value; }
        }

        #endregion PrivateVariables

    }

    /// <summary>
    /// Represents the version of VS to be used for package validation scenario.
    /// </summary>
    public enum VSVersion
    {
        VS2010,
        VS2012,
        VS2013
    }

     /// <summary>
    /// Represents the VS SKU.
    /// </summary>
    public enum VSSKU
    {
        VSU,
        VWDExpress,
        WDExpress,
        Win8Express
    }
 		
}
