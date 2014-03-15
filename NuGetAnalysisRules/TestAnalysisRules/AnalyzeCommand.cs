using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using NuGet;
using NuGet.Commands;
using System.Threading;

namespace TestAnalysisRules
{

    [Command(
        "analyze", 
        "Analyze the specify package to detect potential issues.",
        MinArgs = 1, 
        MaxArgs = 1)]
    public class AnalyzeCommand : Command
    {
        [ImportMany(typeof(IPackageRule))]
        public IEnumerable<IPackageRule> Rules { get; set; }

        public AnalyzeCommand()
        {
        }

        public override void ExecuteCommand()
        {
            // First argument should be the package
          // System.Threading.Thread.Sleep(30 * 1000);
            string packagePath = Arguments[0];

            if (!File.Exists(packagePath))
            {
                Console.WriteError("The specified package path does not exist.");
                return;
            }

            IPackage zipPackage;
            try
            {
                zipPackage = new ZipPackage(packagePath);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to load the specified package.");
                Console.WriteLine("More information:");
                Console.WriteLine(exception.Message);
                return;
            }

            AnalyzePackage(zipPackage);
        }

        internal void AnalyzePackage(IPackage package)
        {
            IEnumerable<IPackageRule> packageRules = Rules;
            if (!String.IsNullOrEmpty(package.Version.SpecialVersion))
            {
                // If a package contains a special token, we'll warn users if it does not strictly follow semver guidelines.
                packageRules = packageRules.Concat(new [] { new StrictSemanticVersionValidationRule() });
            }

            IList<PackageIssue> issues = package.Validate(packageRules)
                                                .OrderBy(p => p.Title, StringComparer.CurrentCulture)
                                                .ToList();

            if (issues.Count > 0)
            {
                Console.WriteLine();
              
                int warningCount = 0;
                int errorCount = 0;
                if (issues.Any(item => item.Level.Equals(PackageIssueLevel.Warning)))
                    warningCount = issues.Where(item => item.Level.Equals(PackageIssueLevel.Warning)).ToList().Count;
                if (issues.Any(item => item.Level.Equals(PackageIssueLevel.Error)))
                    errorCount = issues.Where(item => item.Level.Equals(PackageIssueLevel.Error)).ToList().Count;
                if (errorCount > 0 && warningCount > 0)
                {
                    Console.WriteLine("ERROR: {0} errors and {1} warnings found with package '{2}'.", errorCount.ToString(), warningCount.ToString(), package.Id);
                }
                else if(errorCount > 0)
                {
                    Console.WriteLine("ERROR: {0} errors found with package '{1}'.",  errorCount.ToString(), package.Id);
                }
                else
                {
                    Console.WriteWarning("{0} warnings found with package '{1}'.",  warningCount.ToString(), package.Id);
                }   
                
                foreach (var issue in issues)
                {
                    PrintPackageIssue(issue);
                }
            }
            else
            {
                Console.WriteLine("No issues found with package '{0}'.", package.Id);
            }
        }

        private void PrintPackageIssue(PackageIssue issue)
        {
            Console.WriteLine();
            Console.WriteWarning(
                prependWarningText: false,
                value: "Issue: {0}",
                args: issue.Title);

            Console.WriteWarning(
                prependWarningText: false,
                value: "Level: {0}",
                args: issue.Level.ToString());

            Console.WriteWarning(
                prependWarningText: false,
                value: "Description: {0}",
                args: issue.Description);

            if (!String.IsNullOrEmpty(issue.Solution))
            {
                Console.WriteWarning(
                    prependWarningText: false,
                    value: "Solution: {0}",
                    args: issue.Solution);
            }
        }
    }
}