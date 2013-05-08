using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;
using System.IO;
using System.IO.Compression;
using Ionic.Zip;

namespace NuGetTestHelper
{
    public class NugetDependencyUtility
    {
        /// <summary>
        /// Given a package, this method would return the list of outdated dependencies and the version of the latest dependency present in Nuget official site.
        /// </summary>
        /// <param name="packageFullPath"></param>
        /// <returns></returns>
        public static void GetUpdatesForPackageDependencies(string packageFullPath, out List<Tuple<string,string,string>> minorUpdate, out List<Tuple<string,string,string>> majorUpdate)
        {           
            minorUpdate = new List<Tuple<string, string, string>>();
            majorUpdate = new List<Tuple<string, string, string>>();

            ZipPackage zipPackage = new ZipPackage(packageFullPath);      
            List<PackageDependency> dependencies = new List<PackageDependency>();
            foreach (PackageDependencySet dependencySet in zipPackage.DependencySets)
                dependencies.AddRange(dependencySet.Dependencies);
            IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("https://nuget.org/api/v2/") as IPackageRepository;
            foreach (PackageDependency dependent in dependencies)
            {
                //Check if a later version if available for any of the external dependencies.
                SemanticVersion packageversion = (dependent.VersionSpec.MaxVersion!=null) ? dependent.VersionSpec.MaxVersion : new SemanticVersion(dependent.VersionSpec.ToString());              
                List<IPackage> packages = repo.FindPackagesById(dependent.Id).ToList();
                packages =   packages.Where(item => item.IsListed()).ToList();
                SemanticVersion version = packages.Max(item => item.Version);                   
               if (packageversion < version)
                {
                    if((packageversion.Version.Major.Equals(version.Version.Major)))
                            minorUpdate.Add(new Tuple<string,string,string>(dependent.Id,packageversion.ToString(),version.ToString()));
                    else
                        majorUpdate.Add(new Tuple<string,string,string>(dependent.Id,packageversion.ToString(),version.ToString()));
                }                                  
            }           
        }  

        /// <summary>
        /// Given a packages, updates all its dependencies to point to the latest major and minor version of the dependencies available in the NuGet official source.
        /// </summary>
        /// <param name="PackageFullPath"></param>
        /// <returns></returns>
        public static string GetPackageWithUpdatedDependencies(string PackageFullPath)
        {
            string nuspecFilePath = new ZipPackage(PackageFullPath).Id + ".nuspec";
            List<Tuple<string, string, string>> minorUpdate = new List<Tuple<string, string, string>>();
            List<Tuple<string, string, string>> majorUpdate = new List<Tuple<string, string, string>>();
            GetUpdatesForPackageDependencies(PackageFullPath, out minorUpdate, out majorUpdate);
            using (ZipFile zipFile = new ZipFile(PackageFullPath))
            {              
                ZipEntry entry = zipFile.Entries.Where( item => item.FileName.Contains(".nuspec")).ToList()[0];
                entry.Extract( Environment.CurrentDirectory,ExtractExistingFileAction.OverwriteSilently);
                string extractedfile = Path.Combine(Environment.CurrentDirectory,nuspecFilePath);
                string newFile = "Updated" + nuspecFilePath;

                StreamReader sr = new StreamReader(extractedfile);
                StreamWriter sw = new StreamWriter(newFile);
                while(!sr.EndOfStream)
                {
                    string nextline = sr.ReadLine();
                    if(nextline.Contains("dependency"))
                    {                       
                        int startindex = nextline.IndexOf(@"""");
                        int endindex = nextline.IndexOf(@"""", startindex+1) - 1;
                        string currentDependency = nextline.Substring(startindex + 1, endindex-startindex );
                        if(majorUpdate.Any( item => item.Item1.Equals(currentDependency)))
                        {
                            continue;
                        }
                        if (minorUpdate.Any(item => item.Item1.Equals(currentDependency)))
                        {
                            continue;
                        }
                        
                    }
                    sw.WriteLine(nextline);
                }
                sw.Flush();
                sw.Close();
                sr.Close();
               

                foreach (Tuple<string, string,string> keyValuePair in majorUpdate)
                {                   
                    NuSpecUtility.UpdateDependency( newFile, keyValuePair.Item1, keyValuePair.Item2,keyValuePair.Item3);
                }

                foreach (Tuple<string, string, string> keyValuePair in minorUpdate)
                {
                    NuSpecUtility.UpdateDependency(newFile, keyValuePair.Item1, keyValuePair.Item2, keyValuePair.Item3);
                }           

                zipFile.RemoveEntry(entry);                
                File.Replace(newFile, extractedfile, "backup", true);
                zipFile.AddFile(extractedfile,".");
                string newfileName = "Updated" + new ZipPackage(PackageFullPath).Id + ".nupkg";
                zipFile.Save(newfileName);
                return newfileName;               
            }
       }
         
    }  

}
