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
    public class DependencyAnalysisRule : IPackageRule
    {
        //checks if the latest version of dependencies are being used.
        public IEnumerable<PackageIssue> Validate(IPackage package)
        {

            string sourceUrl = "https://nuget.org/api/v2";            
            List<PackageDependency> dependencies = new List<PackageDependency>();
            foreach (PackageDependencySet dependencySet in package.DependencySets)
                dependencies.AddRange(dependencySet.Dependencies);
            IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository(sourceUrl) as IPackageRepository;
                   
            foreach (PackageDependency dependent in dependencies)
            {
                //Check if a later version if available for any of the external dependencies.
                SemanticVersion packageversion = (dependent.VersionSpec.MaxVersion != null) ? dependent.VersionSpec.MaxVersion : new SemanticVersion(dependent.VersionSpec.ToString());
                List<IPackage> packages = repo.FindPackagesById(dependent.Id).ToList();
                packages = packages.Where(item => item.IsListed()).ToList();
                SemanticVersion version = packages.Max(item => item.Version);
                    if (packageversion < version)
                    {
                        if ((packageversion.Version.Major.Equals(version.Version.Major)))
                        {
                            yield return new PackageIssue(
                     "Package Dependency not pointing to latest minor version.",
                     string.Format("The dependency {2} of the package is not pointing to the latest version available in Nuget.org. Package Version : {0} , Live version : {1}", packageversion, version.ToString(), dependent.Id),
                     "Update the package to latest version if needed.",
                     PackageIssueLevel.Warning);                         
                        }
                        else
                        {
                            yield return new PackageIssue(
                        "Package Dependency not pointing to latest major version.",
                        string.Format("The dependency {2} of the package is not pointing to the latest version available in Nuget.org. Package Version : {0} , Live version : {1}", packageversion, version.ToString(), dependent.Id),
                        "Update the package to latest version if needed.",
                        PackageIssueLevel.Error);                         
                        }
                    }
            }
        }
    }
}
