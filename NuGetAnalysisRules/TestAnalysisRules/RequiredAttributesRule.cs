using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using NuGet;

namespace TestAnalysisRules
{
    [Export(typeof(IPackageRule))] 
    public class RequiredAttributesRule : IPackageRule
    {
        public IEnumerable<PackageIssue> Validate(IPackage package)
        {

            if (String.IsNullOrEmpty(package.Copyright))
            {
                yield return new PackageIssue(
                    "Copyright attribute is missing",
                    "This package doesn't have the Copyright attribute set.",
                    "Set the Copyright attribute.",
                    PackageIssueLevel.Error);
            }

            if (package.LicenseUrl == null)
            {
                yield return new PackageIssue(
                    "License Url is not set",
                    "This package doesn't have the License Url set.",
                    "Set the LicenseUrl attribute to appropriately.",
                    PackageIssueLevel.Error);
            }

            if (package.IconUrl == null)
            {
                yield return new PackageIssue(
                    "Icon Url is not set",
                    "This package doesn't have the Icon Url set.",
                    "Set the Icon Url attribute to appropriately.",
                    PackageIssueLevel.Error);
            }        

            if (String.IsNullOrEmpty(package.Tags))
            {
                yield return new PackageIssue(
                    "Tags attribute is missing",
                    "This package doesn't have the Tags attribute set.",
                    "Set the Tags attribute.",
                    PackageIssueLevel.Error);
            }

            if (String.IsNullOrEmpty(package.Title))
            {
                yield return new PackageIssue(
                    "Title attribute is missing",
                    "This package doesn't have the Title attribute set.",
                    "Set the Title attribute.",
                    PackageIssueLevel.Error);
            }

            if (String.IsNullOrEmpty(package.Summary))
            {
                yield return new PackageIssue(
                    "Summary attribute is missing",
                    "This package doesn't have the Summary attribute set.",
                    "Set the Summary attribute.",
                    PackageIssueLevel.Error);
            }

            if (package.ProjectUrl == null)
            {
                yield return new PackageIssue(
                    "Project Url attribute is missing",
                    "This package doesn't have the Project Url attribute set.",
                    "Set the Project Url attribute.",
                    PackageIssueLevel.Error);
            }

            if (!package.RequireLicenseAcceptance)
            {
                yield return new PackageIssue(
                    "Require License Acceptance is not set to true",
                    "This package doesn't have the Require License Acceptance attribute set to true.",
                    "Set the Require License Acceptance attribute to true.",
                    PackageIssueLevel.Error);
            }
        }
    }
}