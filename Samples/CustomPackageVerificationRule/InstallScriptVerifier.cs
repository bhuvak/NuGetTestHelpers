using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Versioning;
using System.ComponentModel.Composition;
using NuGetTestHelper;
using VSLangProj;

namespace CustomPackageVerificationRule
{
    [Export(typeof(IPackageVerifier))]
    [VerifierMetadata("InstallScriptVerifier for Nuget.VisualStudio", NuGetTestHelper.Constants.InstallAction)]
    public class InstallScriptVerifier : IPackageVerifier
    {
        bool? IPackageVerifier.Validate(string packageFullPath, VsProjectManager dteHelper)
        {
            var reference = dteHelper.GetActiveProject().Object.References.Item("Nuget.VisualStudio");
            if (reference == null)
            {
                errorBuilder.Append("Reference to Nuget.visualStudio not added to the project.");
                return false;
            }
            if (reference.EmbedInteropTypes == true)
            {
                outputBuilder.Append("Reference to Nuget.VisualStudio has EmbedInterOpTypes set to true properly");
                return true;
            }
            else 
            {
                errorBuilder.Append("Reference to Nuget.VisualStudio doesn't have EmbedInterOpTypes set to True as expected.");
                return false;
            }             

        }

        #region Properties

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

        #endregion Properties
    }
}
