using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using NuGet;

namespace TestAnalysisRules
{
    [Export(typeof(IPackageRule))]
    public class AuthorIsMicrosoftRule : IPackageRule
    {
        public IEnumerable<PackageIssue> Validate(IPackage package)
        {
            if (package.Authors != null && package.Authors.All(author => !author.Equals("Microsoft", StringComparison.Ordinal)))
            {
                yield return new PackageIssue(
                    "Authors attribute is invalid.",
                    "The Authors attribute of this package doesn't contain 'Microsoft'.",
                    "Add 'Microsoft' as one of the Authors.",
                    PackageIssueLevel.Error);
            }

            if (package.Owners != null && package.Owners.All(owner => !owner.Equals("Microsoft", StringComparison.Ordinal)))
            {
                yield return new PackageIssue(
                    "Owners attribute is invalid.",
                    "The Owners attribute of this package doesn't contain 'Microsoft'.",
                    "Add 'Microsoft' as one of the Owners.",
                    PackageIssueLevel.Error);
            }
        }
    }
}