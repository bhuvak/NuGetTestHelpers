using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGetTestHelper;
using NuGetPackageVerifiers;
using NuGet;
using System.ComponentModel.Composition;
using System.IO;
using System.Collections.Generic;
using VSLangProj;
using NuGetBVT.Helpers;
using System.Text;

namespace NuGetBVT
{
    [TestClass]
    public class NuGetEndToEndTests
    {
        private TestContext testContextInstance;
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestInitialize()]
        public void Initialize()
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
            vsVersion = (VSVersion) Enum.Parse(typeof(VSVersion), AppSettingsHelper.VSVersionKeyValue, true);
            vsSKU = (VSSKU)Enum.Parse(typeof(VSSKU), AppSettingsHelper.VSSKUKeyValue, true);

        }

        [TestCleanup()]
        public void TestCleanUpMethod()
        {
            //Add the test name and outcome.  
            string details = testContextInstance.Properties["Details"] != null ? testContextInstance.Properties["Details"].ToString() : string.Empty;
            resultsDict.Add(new Tuple<string,string,string>(testContextInstance.TestName, testContextInstance.CurrentTestOutcome.ToString(),details));                   
            if (testContextInstance.CurrentTestOutcome != UnitTestOutcome.Passed)
                    errorCount++;
            
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            HTMLLogger logger = new HTMLLogger(Path.Combine(TestRunPath, packageId + ".htm"));
            logger.WriteSummary(errorCount, warningCount);
            logger.WriteTitle(" {0} {1}", packageId, packageVersion);
            logger.WriteTestCaseResultTableHeader(new string[] { "Scenario", "Result", "Details" });
            foreach (Tuple<string, string, string> result in resultsDict)
                logger.WriteTestCaseResult(result.Item1, result.Item2, result.Item3.Replace("<<<<",""));                  
            logger.WriteEnd();
            logger.WriteLog("Check out the TRX file : {0} for more details.", "NugetBvt_" + packageId + ".trx");
            logger.Dispose();

        }

        [TestMethod]    
        public void PackageInstall()
        {
            InstallPackageInternal(packagePath,false);
        }
                
        [TestMethod]
        public void PackageInstallAfterUpdateAll()
        {
            InstallPackageInternal(packagePath,true);
        }

        [TestMethod]
        public void StaticAnalysis()
        {
            using (NuGetPackageTestHelper nugetHelper = new NuGetPackageTestHelper())
            {
                //Analyze package and verify package installtion
                string analysisOutput = nugetHelper.nuGetPackageManager.AnalyzePackage(packagePath);
                testContextInstance.Properties.Add("Details", analysisOutput);
                TestContext.WriteLine("Output from package Analysis :{0}", analysisOutput);
                if (!string.IsNullOrEmpty(analysisOutput))
                {
                    string summaryLine = analysisOutput.Substring(0, analysisOutput.IndexOf(" found with package '"));
                    warningCount = Convert.ToInt32(summaryLine.Substring(summaryLine.IndexOf("warnings") - 3, 2).Trim());
                    Assert.IsTrue(!summaryLine.Contains("errors"), "static analysis errors found in the package");
                }
                
            }
        }

        [TestMethod]
        [Ignore]
        public void InstallPackageFromFeed()
        {
            string feedUrl = NuGetTestHelper.Constants.NuGetOfficialSourceFeedValue;
            IList<KeyValuePair<string, string>> sources = new List<KeyValuePair<string, string>>();
            sources.Add(new KeyValuePair<string, string>("TestSource", feedUrl));
            NugetSettingsUtility.SetPackageSources(sources);
            NugetSettingsUtility.SetActivePackageSource("TestSource", feedUrl);
            using (NuGetPackageTestHelper nugetHelper = new NuGetPackageTestHelper())
            {
                nugetHelper.vsProjectManager.CloseAllSkus();
                //Launch appropriate version of VS and SKU
                nugetHelper.vsProjectManager.LaunchVS(VSVersion.VS2012, VSSKU.VSU);
                //Create project with desired template and framework.
                nugetHelper.vsProjectManager.CreateProject(ProjectTemplates.ConsoleAppCSharp, ProjectTargetFrameworks.Net45, Path.Combine(TestRunPath, DateTime.Now.Ticks.ToString()));
                string packageInstallOutput = string.Empty;
                //Install package and get output.
                bool installPassed = nugetHelper.nuGetPackageManager.InstallPackage("id", "version", out packageInstallOutput);
                TestContext.WriteLine("Output from package installation :{0} {1}", Environment.NewLine, packageInstallOutput);
                Assert.IsTrue(installPassed);
                nugetHelper.vsProjectManager.CloseSolution();
            }

        }

        #region PrivateMembers

        private void InstallPackageInternal(string packagePath,bool updateAll=false)
        {
            using (NuGetPackageTestHelper nugetHelper = new NuGetPackageTestHelper())
            {
                //nugetHelper.vsProjectManager.CloseAllSkus();
                //Launch appropriate version of VS and SKU
                nugetHelper.vsProjectManager.LaunchVS(vsVersion,vsSKU);
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
                bool installPassed = nugetHelper.nuGetPackageManager.InstallPackage(packagePath, out packageInstallOutput,updateAll);
                TestContext.WriteLine("Output from package installation :{0} {1}", Environment.NewLine, packageInstallOutput);
               
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
                TestContext.WriteLine(resultString.ToString());
                testContextInstance.Properties.Add("Details", packageInstallOutput);
                nugetHelper.vsProjectManager.CloseSolution();
                Assert.IsTrue(installPassed);
                Assert.IsTrue(individualVerifierResults, "Verification from individual verifiers has failed. Check logs for details");
                
            }
        }

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
        #endregion PrivateMembers
    }
}
