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
    public class ValidatePackage
    {       
        private string packageId = string.Empty;
        private string packageVersion = string.Empty;
        private string packagePath = string.Empty;
        private string projName = string.Empty;
        private string TestRunPath = string.Empty;
        private List<Tuple<string, string, string>> resultsDict = new List<Tuple<string, string, string>>();
        private Tuple<string, string> errors = null;
        private VSVersion vsVersion = VSVersion.VS2012;
        private VSSKU vsSKU = VSSKU.VSU;
        private string testResultPath = string.Empty;
        public void Execute(string packageFullPath, TestMode currentMode, out string output, out Tuple<string, string> outErrors, out string resultsPath)
        {
            packagePath = packageFullPath;
            Initialize(packageFullPath);

            if (currentMode == TestMode.All)
            {
                InstallPackage(packageFullPath);
                StaticAnalysis();
            }
            if (currentMode == TestMode.Analyze)
            {
                StaticAnalysis();
            }
            if (currentMode == TestMode.Install)
            {
                InstallPackage(packageFullPath);
            }

            output = DumpLogs();

            outErrors = errors;

            resultsPath = testResultPath;
            
        }

        public  string DumpLogs()
        {
            StringBuilder resultOverview = new StringBuilder();
            testResultPath = Path.Combine(TestRunPath, packageId + ".htm");
            HTMLLogger logger = new HTMLLogger();
            logger.WriteSummary();        
            logger.WriteTestCaseResultTableHeader(new string[] { "Scenario", "Result", "Details" },true);
            foreach (Tuple<string, string, string> result in resultsDict)
            {
                logger.WriteTestCaseResult(result.Item1, result.Item2, result.Item3.Replace("<<<<", ""));
                resultOverview.AppendFormat("{0}:{1}; ", result.Item1, result.Item2);
            }        
            StreamReader sr = new StreamReader("PackageResultsTemplate.htm");
            string body = sr.ReadToEnd();
            sr.Close();
            body = body.Replace("{Rows}", logger.stringwriter.ToString());
            body = body.Replace("{PackageID}", packageId);

            StreamWriter sw = new StreamWriter(testResultPath);
            sw.Write(body);
            sw.Flush();
            sw.Close();
            return resultOverview.ToString();
        }

        private void Initialize(string packagePath)
        {
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
            AppSettingsHelper.TestResultsPathKeyValue = TestRunPath;

            //initialize other values.
            projName = DateTime.Now.Ticks.ToString();
            vsVersion = (VSVersion)Enum.Parse(typeof(VSVersion), AppSettingsHelper.VSVersionKeyValue, true);
            vsSKU = (VSSKU)Enum.Parse(typeof(VSSKU), AppSettingsHelper.VSSKUKeyValue, true);

        }

        private void InstallPackage(string packagePath, bool updateAll = false)
        {
            try
            {
                using (NuGetPackageTestHelper nugetHelper = new NuGetPackageTestHelper())
                {
                    if (AppSettingsHelper.CloseAllSKUsKeyValue.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        nugetHelper.vsProjectManager.CloseAllSkus();
                    }                
                    //Launch appropriate version of VS and SKU
                    nugetHelper.vsProjectManager.LaunchVS(vsVersion, vsSKU);
                    //Create project with desired template and framework.
                    try
                    {
                        if (string.IsNullOrEmpty(AppSettingsHelper.ProjectTemplateFullPathKeyValue))
                        {
                            nugetHelper.vsProjectManager.CreateProject(AppSettingsHelper.ProjectTemplateNameKeyValue, AppSettingsHelper.ProjectTemplateLanguageKeyValue, ProjectTargetFrameworks.Net45, projName, Path.Combine(TestRunPath, DateTime.Now.Ticks.ToString()));
                        }
                        else
                        {
                            nugetHelper.vsProjectManager.CreateProject(AppSettingsHelper.ProjectTemplateFullPathKeyValue, ProjectTargetFrameworks.Net45, projName, Path.Combine(TestRunPath, DateTime.Now.Ticks.ToString()));
                        }
                    }catch(Exception e)
                    {                        
                        throw new ArgumentException(string.Format("Error while creating project out of template {0} for package {1}.Make sure that the template name is specified properly", AppSettingsHelper.ProjectTemplateNameKeyValue, packagePath));
                    }
                    string packageInstallOutput = string.Empty;
                    //Install package and get output.
                    bool installPassed = nugetHelper.nuGetPackageManager.InstallPackage(packagePath, out packageInstallOutput, updateAll);
                  

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
                  
                    nugetHelper.vsProjectManager.CloseSolution();
                    resultsDict.Add(new Tuple<string, string, string>("Install", installPassed ? "Passed" : "Failed", packageInstallOutput + Environment.NewLine + "Output from individual verifiers :" + Environment.NewLine + resultString));

                }
            }catch(Exception e)
            {
                 resultsDict.Add(new Tuple<string, string, string>("Install", "Failed",string.Format("Error while installing package from VS. Message {0} {1} Stack track {2}", e.Message,Environment.NewLine,e.StackTrace)));
            }
        }

        private void StaticAnalysis()
        {
            try
            {
                using (NuGetPackageTestHelper nugetHelper = new NuGetPackageTestHelper())
                {
                    //Analyze package and verify package installtion
                    bool analysisPassed = true;
                    string analysisOutput = nugetHelper.nuGetPackageManager.AnalyzePackage(packagePath);
                  
                    if (!string.IsNullOrEmpty(analysisOutput))
                    {
                        analysisPassed = !analysisOutput.Contains("ERROR: ");                        
                    }

                    if (!analysisPassed)
                    {
                        errors = new Tuple<string, string> ("Static Analysis Failed", analysisOutput);
                    }

                    resultsDict.Add(new Tuple<string, string, string>("StaticAnalysis", analysisPassed ? "Passed" : "Failed", analysisOutput));
                }
            }catch(Exception e)
            {
                resultsDict.Add(new Tuple<string, string, string>("StaticAnalysis", "Failed",string.Format("Error while running static analysis. Message {0} {1} Stack track {2}", e.Message,Environment.NewLine,e.StackTrace)));
            }
        } 

    }
}
