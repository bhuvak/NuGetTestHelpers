using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using NuGet;
using System.Text.RegularExpressions;


namespace TestAnalysisRules
{
    [Export(typeof(IPackageRule))]
    public class SatellitePackageRule : IPackageRule
    {
        public IEnumerable<PackageIssue> Validate(IPackage package)
        {
            if (package.IsSatellitePackage())
            {
                var comparer = StringComparer.Create(new System.Globalization.CultureInfo(package.Language), true);              
               
                if (package.Summary.Contains("{"))
                {
                    yield return new PackageIssue(
                        "Package summary is not localized correctly",
                        "The summary of the package is not localized correctly'",
                        "Update the summary with a proper localized string",
                        PackageIssueLevel.Error);
                }

                if (package.Title.Contains("{"))
                {
                    yield return new PackageIssue(
                        "Package title is not localized correctly",
                        "The title of the package is not localized correctly'",
                        "Update the title with a proper localized string",
                        PackageIssueLevel.Error);
                }

                if (package.Description.Contains("{"))
                {
                    yield return new PackageIssue(
                        "Package description is not localized",
                        "The description of the package is not localized correctly'",
                        "Update the description with a proper localized string",
                        PackageIssueLevel.Error);
                }
            }
        }

        private bool IsNonEnglish(string input)
        {
            return !(Regex.IsMatch(@"[^A-Za-z0-9'\.&@:?!()$#^]", input));
        }
    }
}
