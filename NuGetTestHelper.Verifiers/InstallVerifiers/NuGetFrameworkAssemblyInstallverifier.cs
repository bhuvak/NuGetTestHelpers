using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;
using NuGetTestHelper;
using System.ComponentModel.Composition;
using System.Runtime.Versioning;



namespace NuGetPackageVerifiers
{
    [Export(typeof(IPackageVerifier))]
    [VerifierMetadata("FramworkAssembly Verifier", NuGetTestHelper.Constants.InstallAction)]
    public class NuGetFrameworkAssemblyInstallverifier : IPackageVerifier
    {
        public bool? Validate(string packageFullPath, VsProjectManager dteHelper)
        {          
          
            string solutionPath = dteHelper.SolutionPath;
            HasSuceeded = true;       

            Dictionary<string, string> referencesInProject = dteHelper.GetReferences();
            ZipPackage zipPackage = new ZipPackage(packageFullPath);
            List<FrameworkAssemblyReference> frameworkAssemblies = zipPackage.FrameworkAssemblies.ToList();
            if (frameworkAssemblies == null || frameworkAssemblies.Count == 0 )
            {            
                outputBuilder.Append("No Framework assemblies present in the package. Skipping the verification.");
                return null;
            }
                       
            //Get only thr framework assemblies which applies to the current project.
            frameworkAssemblies = frameworkAssemblies.Where(item => item.SupportedFrameworks.Contains(new FrameworkName(dteHelper.GetProjectFramework()))).ToList();

            foreach (FrameworkAssemblyReference frameworkassembly in frameworkAssemblies)
            {
                if (!referencesInProject.Keys.Contains(frameworkassembly.AssemblyName))
                {
                    HasSuceeded = false;
                    errorBuilder.AppendFormat(" Reference to the Framework Assembly {0} not added to the project. Check the solution @ {1} for more details.", frameworkassembly.AssemblyName, solutionPath);
                    errorBuilder.AppendLine();
                }
                else
                {           
                    outputBuilder.AppendFormat(" Reference to the Framework Assembly {0} added properly.", frameworkassembly.AssemblyName);
                    outputBuilder.AppendLine();
                }
            }
            return HasSuceeded;
        }

        public bool? HasSuceeded { get; set; }
        public string Output
        {
            get
            {
                return this.outputBuilder.ToString();
            }

        }

        public string Error
        {
            get
            {
                return this.errorBuilder.ToString();
            }

        }
        
        private StringBuilder outputBuilder = new StringBuilder();
        private StringBuilder errorBuilder = new StringBuilder();
    }
    
}
