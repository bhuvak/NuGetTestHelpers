using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGetTestHelper;
using NuGetPackageVerifiers;
using NuGet;
using System.IO;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace NuGetPackageValidator
{
     public enum TestMode
        {
            Install,
            Analyze,
            All
        };
    public class Program
    {
        public static TestMode currentMode = TestMode.All;
        private static string packageId = string.Empty;
        private static string packageVersion = string.Empty;
        private static string packagePath = string.Empty;
        private static string projName = string.Empty;
        private static string TestRunPath = string.Empty;
        private static List<Tuple<string, string, string>> resultsDict = new List<Tuple<string, string, string>>();
        private static int errorCount = 0;
        private static int warningCount = 0;
        private static VSVersion vsVersion = VSVersion.VS2012;
        private static VSSKU vsSKU = VSSKU.VSU;       
        public static void Main(string[] args)
        {
           
            if(args.Length > 0)
            {
               AppSettingsHelper.PackageFullPathKeyValue =  args[0];               
            }           
            Initialize();
            
            if(args.Length > 1)
            {
               string[] testmodevalues =  args[1].Split(new char[] { ':' });
               if(Enum.TryParse<TestMode>(testmodevalues[1], out currentMode))
                   currentMode= (TestMode)Enum.Parse(typeof(TestMode),testmodevalues[1]);
            }

            if(currentMode == TestMode.All)
            {
                InstallPackage(packagePath);
                StaticAnalysis();                
            }
            if(currentMode == TestMode.Analyze)
            {
                StaticAnalysis();
            }
            if(currentMode == TestMode.Install)
            {
                InstallPackage(packagePath);
            }

            DumpLogs();
        }

        public static void DumpLogs()
        {      

            HTMLLogger logger = new HTMLLogger(Path.Combine(TestRunPath, packageId + ".htm"));
            logger.WriteSummary(errorCount, warningCount);
            logger.WriteTitle(" {0} {1}", packageId, packageVersion);
            logger.WriteTestCaseResultTableHeader(new string[] { "Scenario", "Result", "Details" });
            foreach (Tuple<string, string, string> result in resultsDict)
                logger.WriteTestCaseResult(result.Item1, result.Item2, result.Item3.Replace("<<<<", ""));
            logger.WriteEnd();         
            logger.Dispose();
        }

        private static void Initialize()
        {
            //extract package id and version.
            packagePath = AppSettingsHelper.PackageFullPathKeyValue;
          
            ZipPackage zipPackage = new ZipPackage(packagePath);
            packageId = zipPackage.Id;
            packageVersion = zipPackage.Version.ToString();

            //set package sources.
            string PackageSource = Path.GetDirectoryName(packagePath);
            if (string.IsNullOrEmpty(PackageSource))
                PackageSource = Environment.CurrentDirectory;
            IList<KeyValuePair<string, string>> sources = new List<KeyValuePair<string, string>>();
            sources.Add(new KeyValuePair<string, string>("TestSource", PackageSource));
            //If additional sources are specified in the app.config file, add them too.
            if (!string.IsNullOrEmpty(AppSettingsHelper.PackageSourceKeyValue))
            {
                string[] additionalPackageSources = AppSettingsHelper.PackageSourceKeyValue.Split(new char[] { ',', ';' });
                int i = 1;
                foreach (string additionalSource in additionalPackageSources)
                {
                    sources.Add(new KeyValuePair<string, string>("AdditionalSources_" + i.ToString(), additionalSource));
                    i++;
                }
            }
            NugetSettingsUtility.SetPackageSources(sources);
            NugetSettingsUtility.SetActivePackageSource("TestSource", PackageSource);

            //Set test run directory.
            if (string.IsNullOrEmpty(AppSettingsHelper.TestResultsPathKeyValue))
                TestRunPath = Path.Combine(Environment.CurrentDirectory, packageId);
            else
                TestRunPath = AppSettingsHelper.TestResultsPathKeyValue;
            //Create root level test run Dir
            if (!Directory.Exists(TestRunPath))
            {
                Directory.CreateDirectory(TestRunPath);
            }

            //initialize other values.
            projName = DateTime.Now.Ticks.ToString();
            vsVersion = (VSVersion)Enum.Parse(typeof(VSVersion), AppSettingsHelper.VSVersionKeyValue, true);
            vsSKU = (VSSKU)Enum.Parse(typeof(VSSKU), AppSettingsHelper.VSSKUKeyValue, true);

        }

        private static void InstallPackage(string packagePath, bool updateAll = false)
        {
            using (NuGetPackageTestHelper nugetHelper = new NuGetPackageTestHelper())
            {
                //nugetHelper.vsProjectManager.CloseAllSkus();
                //Launch appropriate version of VS and SKU
                nugetHelper.vsProjectManager.LaunchVS(vsVersion, vsSKU);
                //Create project with desired template and framework.
                if (string.IsNullOrEmpty(AppSettingsHelper.ProjectTemplateFullPathKeyValue))
                {
                    nugetHelper.vsProjectManager.CreateProject(AppSettingsHelper.ProjectTemplateNameKeyValue, AppSettingsHelper.ProjectTemplateLanguageKeyValue, ProjectTargetFrameworks.Net45, projName, Path.Combine(TestRunPath, DateTime.Now.Ticks.ToString()));
                }
                else
                {
                    nugetHelper.vsProjectManager.CreateProject(AppSettingsHelper.ProjectTemplateFullPathKeyValue, ProjectTargetFrameworks.Net45, projName, Path.Combine(TestRunPath, DateTime.Now.Ticks.ToString()));
                }
                string packageInstallOutput = string.Empty;
                //Install package and get output.
                bool installPassed = nugetHelper.nuGetPackageManager.InstallPackage(packagePath, out packageInstallOutput, updateAll);
                Console.WriteLine("Output from package installation :{0} {1}", Environment.NewLine, packageInstallOutput);

                //Get output from individual verifiers.
                bool individualVerifierResults = true;
                StringBuilder resultString = new StringBuilder();
                foreach (var verifier in nugetHelper.PackageInstallationVerifiers)
                {
                    bool? passed = verifier.Value.Validate(packagePath, nugetHelper.vsProjectManager);
                    if (!passed.HasValue)
                        continue;
                    else
                    {
                        resultString.AppendFormat("Output from {0} :", verifier.Metadata.Name);
                        if (passed == true)
                            resultString.AppendLine(verifier.Value.Output);
                        if (passed == false)
                        {
                            individualVerifierResults = false;
                            resultString.AppendLine(verifier.Value.Error);
                        }
                    }
                }
                Console.WriteLine(resultString.ToString());                
                nugetHelper.vsProjectManager.CloseSolution();               
                resultsDict.Add(new Tuple<string,string,string>("Install From VS", installPassed ? "Passed": "Failed", packageInstallOutput + Environment.NewLine + "Output from individual verifiers :" + Environment.NewLine + resultString));

            }
        }

        private static void StaticAnalysis()
        {
            using (NuGetPackageTestHelper nugetHelper = new NuGetPackageTestHelper())
            {
                //Analyze package and verify package installtion
                bool analysisPassed = true;
                string analysisOutput = nugetHelper.nuGetPackageManager.AnalyzePackage(packagePath);               
                Console.WriteLine("Output from package Analysis :{0}", analysisOutput);
                if (!string.IsNullOrEmpty(analysisOutput))
                {
                    string summaryLine = analysisOutput.Substring(0, analysisOutput.IndexOf(" found with package '"));
                    warningCount = Convert.ToInt32(summaryLine.Substring(summaryLine.IndexOf("warnings") - 3, 2).Trim());
                    analysisPassed = !summaryLine.Contains("errors");
                }
                resultsDict.Add(new Tuple<string, string, string>("Static analysis", analysisPassed ? "Passed" : "Failed", analysisOutput));
            }
        }

    }
}
