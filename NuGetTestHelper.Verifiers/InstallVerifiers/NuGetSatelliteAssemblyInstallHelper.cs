using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Versioning;
using System.ComponentModel.Composition;
using NuGet;
using System.IO;
using VSLangProj;
using NuGetTestHelper;

namespace NuGetPackageVerifiers
{
    [Export(typeof(IPackageVerifier))]
    [VerifierMetadata("SatellitePackage Verifier", NuGetTestHelper.Constants.InstallAction)]
    public class NuGetSatelliteAssemblyInstallHelper : IPackageVerifier
    {
        /// <summary>
        /// Validates that the reference corrresponding to the resource assembly is added to the project and the resources are being extracted.
        /// </summary>
        /// <param name="packageFullPath"></param>
        /// <param name="solutionPath"></param>
        public bool? Validate(string packageFullPath, VsProjectManager dteHelper)
        {
            ZipPackage zipPackage = new ZipPackage(packageFullPath);
            string solutionPath = dteHelper.SolutionPath;
            HasSuceeded = true;
            if (zipPackage.IsSatellitePackage())
            {
                string language = zipPackage.Language;
                List<IPackageFile> satelliteFiles = zipPackage.GetSatelliteFiles().ToList();
                foreach (IPackageFile satelliteFile in satelliteFiles)
                {
                    if (satelliteFile.Path.EndsWith(".dll") || satelliteFile.Path.EndsWith(".exe")) //exclude xml files
                    {
                        string satelliteAssembly = Path.GetFileNameWithoutExtension(satelliteFile.Path);
                        string baseAssemblyName = satelliteAssembly.Replace(".resources", "");  //base assembly name would be the same without ".resources"
                        Reference baseReference = (Reference) dteHelper.GetReferenceByName(baseAssemblyName);
                        if (baseReference == null)
                        {
                            HasSuceeded = false;
                            errorBuilder.AppendFormat(" No reference added for the ENU assembly {0} relative to the satellite assembly {1}.", baseAssemblyName, satelliteAssembly);
                            errorBuilder.AppendLine();
                        }
                        else
                        {
                            string expectedSatelliteAssemblyPath = (Path.Combine(Path.GetDirectoryName(baseReference.Path), language, satelliteAssembly + ".dll"));
                            if (File.Exists(expectedSatelliteAssemblyPath))
                            {
                                outputBuilder.AppendFormat("Reference added for ENU assembly {0} and the resources are extraced next to it @ {1}", baseAssemblyName, expectedSatelliteAssemblyPath);
                                outputBuilder.AppendLine();
                            }
                            else
                            {
                                HasSuceeded = false;
                                errorBuilder.AppendFormat("The satellite assembly is not extraced @ {0} for reference {1}", expectedSatelliteAssemblyPath, baseAssemblyName);
                                errorBuilder.AppendLine();
                            }
                        }
                    }
                }

            }
            else
            {
                return null;
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
