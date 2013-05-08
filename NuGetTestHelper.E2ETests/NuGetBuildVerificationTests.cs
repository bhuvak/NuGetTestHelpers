using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGetTestHelper;
using NuGetPackageVerifiers;
using System.ComponentModel.Composition;
using System.IO;
using System.Collections.Generic;
using VSLangProj;
using System.Configuration;

namespace NuGetTestHelper
{
    [TestClass]
    public class NuGetBuildVerificationTests
    {
        #region privaeMethods
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

        //Specify the test values here       
        private string packagePath = string.Empty;
        private string solnPath = string.Empty;
        private string projName = string.Empty;
        #endregion privaeMethods

        [TestInitialize()]
        public void Initialize()
        {
            packagePath = ConfigurationManager.AppSettings["PackageFullPath"];
            string source = Path.GetFullPath(Path.GetDirectoryName(packagePath));           
            projName = DateTime.Now.Ticks.ToString();
            solnPath = Path.Combine(ConfigurationManager.AppSettings["SolutionPath"], projName);
            IList<KeyValuePair<string, string>> sources = new List<KeyValuePair<string, string>>();
            sources.Add(new KeyValuePair<string, string>("TestSource", source));
            NugetSettingsUtility.SetPackageSources(sources);
            NugetSettingsUtility.SetActivePackageSource("TestSource", source);
        }


        [TestMethod]
        public void TestPackageInstall()
        {
            using (NuGetPackageTestHelper nugetHelper = new NuGetPackageTestHelper())
            {
                //nugetHelper.vsProjectManager.CloseAllSkus(); //close any stale instances.
                //Launch appropriate version of VS and SKU
                nugetHelper.vsProjectManager.LaunchVS(VSVersion.VS2012, VSSKU.VSU);
                //Create project with desired template and framework.
                nugetHelper.vsProjectManager.CreateProject(ProjectTemplates.ConsoleAppCSharp, ProjectTargetFrameworks.Net45, projName, solnPath);
                //Analyze package and verify package installtion
                string analysisOutput = nugetHelper.nuGetPackageManager.AnalyzePackage(packagePath);
                TestContext.WriteLine("Output from Package analysis : {0}", analysisOutput);
                string packageInstallOutput = string.Empty;
                Assert.IsTrue(nugetHelper.nuGetPackageManager.InstallPackage(packagePath, out packageInstallOutput), "Install package failed.");
                TestContext.WriteLine("Output from Package Install : {0} {1}", Environment.NewLine, packageInstallOutput);
                Assert.IsTrue(nugetHelper.VerifyPackageInstallation(packagePath), "Install verification failed");
            }
        }

        [TestMethod]
        public void TestIndividualPackageVerifiers()
        {
            using (NuGetPackageTestHelper nugetHelper = new NuGetPackageTestHelper())
            {
                //nugetHelper.vsProjectManager.CloseAllSkus(); //close any stale instances.
                //Launch appropriate version of VS and SKU
                nugetHelper.vsProjectManager.LaunchVS(VSVersion.VS2012, VSSKU.VSU);
                //Create project with desired template and framework.
                nugetHelper.vsProjectManager.CreateProject(ProjectTemplates.ConsoleAppCSharp, ProjectTargetFrameworks.Net45, DateTime.Now.Ticks.ToString());
                string packageInstallOutput = string.Empty;
                //Install package and get output.
                Assert.IsTrue(nugetHelper.nuGetPackageManager.InstallPackage(packagePath, out packageInstallOutput));
                TestContext.WriteLine("Output from package installation :{0} {1}", Environment.NewLine, packageInstallOutput);
                //Get output from individual verifiers.
                bool individualVerifierResults = true;
                foreach (var verifier in nugetHelper.PackageInstallationVerifiers)
                {
                    bool? passed = verifier.Value.Validate(packagePath, nugetHelper.vsProjectManager);
                    if (!passed.HasValue)
                        continue;
                    else
                    {
                        TestContext.WriteLine("Output from {0} :", verifier.Metadata.Name);
                        if (passed == true)
                            TestContext.WriteLine(verifier.Value.Output);
                        if (passed == false)
                        {
                            individualVerifierResults = false;
                            TestContext.WriteLine(verifier.Value.Error);
                        }
                    }
                }
                Assert.IsTrue(individualVerifierResults, "Verification from individual verifiers has failed. Check logs for details");
                nugetHelper.vsProjectManager.CloseSolution();
            }

        }
    }
}


