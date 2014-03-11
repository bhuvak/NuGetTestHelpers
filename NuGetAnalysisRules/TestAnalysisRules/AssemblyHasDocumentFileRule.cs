using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using NuGet;

namespace TestAnalysisRules
{
    [Export(typeof(IPackageRule))]
    public class AssemblyHasDocumentFileRule : IPackageRule
    {
        /// <summary>
        /// Validates if XML documentation file is present for all the reference assemblies.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public IEnumerable<PackageIssue> Validate(IPackage package)
        {
            if (!package.IsSatellitePackage())
            {

                var libXmlFiles = package.GetLibFiles()
                                         .Select(file => file.Path)
                                         .Where(path => path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

                var libXmlFileSet = new HashSet<string>(libXmlFiles, StringComparer.OrdinalIgnoreCase);

                foreach (IPackageFile assemblyFile in package.GetLibFiles())
                {
                    string filePath = assemblyFile.Path;
                    if (filePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        string documentPath = Path.ChangeExtension(filePath, ".xml");
                        if (!libXmlFileSet.Contains(documentPath))
                        {
                            yield return new PackageIssue(
                                "Assembly lacks XML document file.",
                                "The assembly '" + filePath + "' doesn't have a corresponding XML document file.",
                                "Add an XML document file at '" + documentPath + "'.",
                                PackageIssueLevel.Warning);
                        }
                    }
                }
            }
        }
    }
}