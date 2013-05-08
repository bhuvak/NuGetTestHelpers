using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;
using System.IO;
using System.Runtime.Versioning;
using System.ComponentModel.Composition;
using NuGetTestHelper;

namespace NuGetPackageVerifiers
{
    [Export(typeof(IPackageVerifier))]
    [VerifierMetadata("Contents, Verifier",NuGetTestHelper.Constants.InstallAction)]
    public class NuGetContentsInstallVerifier : IPackageVerifier
    {
        public bool? Validate(string packageFullPath, VsProjectManager dteHelper)
        {
         
            string solutionPath = dteHelper.SolutionPath;      
            HasSuceeded = true;
            
            ZipPackage zipPackage = new ZipPackage(packageFullPath);
            List<IPackageFile> files = zipPackage.GetContentFiles().ToList();         
            if (files == null || files.Count == 0)
            {               
                outputBuilder.AppendFormat(" No content files are present in the nupkg file.Skipping content validation");
                return null;
            }

            //Get compatible items for the current project.
            //if no compatible items, then check if there are any files with directly under contents and get them.
            IEnumerable<IPackageFile> compatibleItems;
            VersionUtility.TryGetCompatibleItems(new FrameworkName(dteHelper.GetProjectFramework()), files, out compatibleItems);
            if (compatibleItems == null || compatibleItems.ToList().Count == 0)
                VersionUtility.TryGetCompatibleItems(null, files, out compatibleItems);
            if (compatibleItems == null || compatibleItems.ToList().Count == 0)
            {             
                outputBuilder.AppendFormat(" The package doesnt have a content folder matching the current project's target framework : {0}", dteHelper.GetProjectFramework());
                return null;
            }

            //exclude the .transform files as they are treated separeately.
            List<IPackageFile> contentFiles = new List<IPackageFile>();
            foreach(IPackageFile file in compatibleItems)
            {
                if (!file.Path.EndsWith(".transform"))
                    contentFiles.Add(file);                
            }

            if (contentFiles == null || contentFiles.Count == 0)
            {              
                outputBuilder.AppendFormat(" No content files are present in the nupkg file.Skipping content validation");
                return null;
            }

            foreach (IPackageFile file in contentFiles)
            {                             
                string filePath = file.Path.Remove(0, @"Content\".Length);
                if (file.Path.EndsWith(".pp"))
                    filePath = filePath.Remove(filePath.Length - 3, 3);
                filePath = Path.Combine(solutionPath, filePath);
                if (File.Exists(filePath))
                {         
                    outputBuilder.AppendFormat("Content file : {0} added properly !!", file.Path);
                    outputBuilder.AppendLine();
                }
                else
                {
                    HasSuceeded = false;
                    errorBuilder.AppendFormat("Content file : {0} not added properly. Check the solution @ {1} for more details", file.Path, solutionPath);
                    errorBuilder.AppendLine();
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
