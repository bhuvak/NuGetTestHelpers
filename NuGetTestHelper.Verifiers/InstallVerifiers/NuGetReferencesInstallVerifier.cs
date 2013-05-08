using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Versioning;
using System.ComponentModel.Composition;
using NuGet;
using NuGetTestHelper;
using System.IO;


namespace NuGetPackageVerifiers
{
    [Export(typeof(IPackageVerifier))]
    [VerifierMetadata("References Verifier", NuGetTestHelper.Constants.InstallAction)]
    public class NuGetReferencesInstallVerifier : IPackageVerifier
    {
        /// <summary>
        /// Validates the expected references to be added by the given package and the actual references being added matches.
        /// </summary>
        /// <param name="packageFullPath"></param>
        /// <param name="solutionPath"></param>
        public bool? Validate(string packageFullPath, VsProjectManager dteHelper)
        {
            ZipPackage zipPackage = new ZipPackage(packageFullPath);
            if (zipPackage.IsSatellitePackage())
                return null;

            string solutionPath = dteHelper.SolutionPath;          
            HasSuceeded = true;
        
            Dictionary<string,string> referencesInProject = dteHelper.GetReferences();
            
            string packageId = zipPackage.Id;
            List<IPackageFile> files = zipPackage.GetLibFiles().ToList();
            if( files == null || files.Count == 0 )
            {
                outputBuilder.AppendFormat(" No Lib files are present in the nupkg file.Skipping reference validation.");
                return null;
            }
        
            //Get compatible items for the current project.
            //if no compatible items, then check if there are any files with directly under Lib and get them.
            IEnumerable<IPackageFile> compatibleItems;
            VersionUtility.TryGetCompatibleItems(new FrameworkName(dteHelper.GetProjectFramework()), files, out compatibleItems);
            if (compatibleItems == null || compatibleItems.ToList().Count == 0)
                VersionUtility.TryGetCompatibleItems(null, files, out compatibleItems);
            if (compatibleItems == null || compatibleItems.ToList().Count == 0)
            {               
                outputBuilder.AppendFormat(" The package doesnt have a Lib folder matching the current project's target framework : {0}", dteHelper.GetProjectFramework());
                return null;
            }

            //Check that the compatible lib files are added as references.
            foreach (IPackageFile file in compatibleItems)
            {
                if (file.Path.EndsWith(".dll") || file.Path.EndsWith(".exe")) //exclude xml files
                {
                    string referenceName = Path.GetFileNameWithoutExtension((file.Path));
                    if (!referencesInProject.Keys.Contains(referenceName, StringComparer.OrdinalIgnoreCase))
                    {                  
                        HasSuceeded = false;
                        errorBuilder.AppendFormat("The reference {0} is not added to project as part of installating package {1}.Check the Solution @ {2} for details", referenceName, packageId, solutionPath);
                        errorBuilder.AppendLine();
                    }
                    else
                    {                                     
                        outputBuilder.AppendFormat("Reference Added properly for Lib : {0} !!", file.Path);
                        outputBuilder.AppendLine();
                    }
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
