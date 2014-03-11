using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using NuGet;

namespace TestAnalysisRules
{
    [Export(typeof(IPackageRule))]
    public class PowerShellScriptIsSignedRule : IPackageRule
    {
        private static readonly string[] PowerShellExtensions = new string[] { ".ps1", ".psm1", ".psd1", ".ps1xml" };

        public IEnumerable<PackageIssue> Validate(IPackage package)
        {
            foreach (var packageFile in package.GetFiles())
            {
                string extension = Path.GetExtension(packageFile.Path);
                if (PowerShellExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    bool isSigned = VerifySigned(packageFile);
                    if (!isSigned)
                    {
                        yield return new PackageIssue(
                            "PowerShell script is not signed.",
                            "The PowerShell script '" + packageFile.Path + "' is not signed.",
                            "Sign this file.", 
                            PackageIssueLevel.Error);
                    }
                }
            }
        }

        private bool VerifySigned(IPackageFile packageFile)
        {
            const string SignatureStartBlock = "# SIG # Begin signature block";
            const string SignatureEndBlock = "# SIG # End signature block";

            using (Stream stream = packageFile.GetStream())
            {
                var streamReader = new StreamReader(stream);
                string fullText = streamReader.ReadToEnd();

                return fullText.IndexOf(SignatureStartBlock, StringComparison.OrdinalIgnoreCase) > -1 &&
                       fullText.IndexOf(SignatureEndBlock, StringComparison.OrdinalIgnoreCase) > -1;
            }
        }
    }
}