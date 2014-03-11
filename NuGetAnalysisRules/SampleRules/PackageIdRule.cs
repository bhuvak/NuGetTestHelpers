using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using NuGet;

namespace TestAnalysisRules
{
   [Export(typeof(IPackageRule))] 
    public class PackageIdRule : IPackageRule
    {
        //Checks if the package Id starts with a specified prefix.
        public IEnumerable<PackageIssue> Validate(IPackage package)
        {
            if (!package.Id.StartsWith("Microsoft.", StringComparison.Ordinal))
            {
                yield return new PackageIssue(
                    "Package Id is not valid",
                    "The Id of this package doesn't start with 'Microsoft.AspNet.*'",
                    "Rename the Id attribute so that it starts with 'Microsoft.AspNet.*'",
                    PackageIssueLevel.Error);
            }
        }
    }
}