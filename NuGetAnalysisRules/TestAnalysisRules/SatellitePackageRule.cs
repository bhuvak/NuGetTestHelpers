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
               
                if (package.Summary.Contains("{") || package.Title.Contains("{") || package.Description.Contains("{") )
                {
                    yield return new PackageIssue(
                        "Package summary is not localized",
                        "The summary of the package is not localized'",
                        "Update the summary with a proper localized string",
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
