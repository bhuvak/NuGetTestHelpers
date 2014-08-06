using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using NuGet;
using System.Text.RegularExpressions;
using System.IO;


namespace TestAnalysisRules
{
    [Export(typeof(IPackageRule))]
    public class AuthenticodeSigningRule: IPackageRule
    {
        public IEnumerable<PackageIssue> Validate(IPackage package)
        {
            foreach (IPackageFile assemblyFile in package.GetFiles())
            {
                string extension = Path.GetExtension(assemblyFile.Path);
                string tempFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
             
                if (extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) || extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    bool signed = WinTrust.IsAuthentiCodeSigned(assemblyFile.Path);

                    if (!signed)
                    {
                        yield return new PackageIssue(
                            "Assembly not authenticode signed",
                              "The assembly '" + assemblyFile.Path + "' in this package is not authenticode signed.",
                            "Authenticode sign this assembly",
                            PackageIssueLevel.Error);
                    }
                }
            }
        }      
    }
}
