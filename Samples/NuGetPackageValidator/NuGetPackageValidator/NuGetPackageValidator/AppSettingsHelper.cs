using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;


namespace NuGetPackageValidator
{
    /// <summary>
    /// This class encapsulates the default appsettings keys and values.
    /// </summary>
    public static class AppSettingsHelper
    {
        //AppSettings Keys.
        public static string VSVersionKey = "VSVersion";
        public static string VSSKUKey = "VSSKU";
        public static string FrameworkKey = "Framework";
        public static string AdditionalPackageSourcesKey = "AdditionalPackageSources";
        public static string PackageFullPathKey = "PackageFullPath";
        public static string ProjectTemplateNameKey = "ProjectTemplateName";
        public static string ProjectTemplateFullPathKey = "ProjectTemplateFullPath";
        public static string ProjectTemplateLanguageKey = "ProjectTemplateLanguage";
        public static string TestResultsPathKey = "TestResultsPath";
        public static string SolutionPathKey = "SolutionPath";
        public static string CurrentWorkingDirKey = "CurrentWorkingDir";
       


        //AppSetting values.
        public static string VSVersionKeyValue = ConfigurationManager.AppSettings[VSVersionKey];
        public static string VSSKUKeyValue = ConfigurationManager.AppSettings[VSSKUKey];
        public static string FrameworkKeyValue = ConfigurationManager.AppSettings[FrameworkKey];
        public static string PackageSourceKeyValue = ConfigurationManager.AppSettings[AdditionalPackageSourcesKey];
        public static string PackageFullPathKeyValue = ConfigurationManager.AppSettings[PackageFullPathKey];
        public static string ProjectTemplateNameKeyValue = ConfigurationManager.AppSettings[ProjectTemplateNameKey];
        public static string ProjectTemplateFullPathKeyValue = ConfigurationManager.AppSettings[ProjectTemplateFullPathKey];
        public static string ProjectTemplateLanguageKeyValue = ConfigurationManager.AppSettings[ProjectTemplateLanguageKey];
        public static string TestResultsPathKeyValue = ConfigurationManager.AppSettings[TestResultsPathKey];       
        public static string SolutionPathKeyValue = ConfigurationManager.AppSettings[SolutionPathKey];
        public static string CurrentWorkingDirKeyValue
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[CurrentWorkingDirKey]))
                    return Environment.CurrentDirectory;
                else
                    return ConfigurationManager.AppSettings[CurrentWorkingDirKey];
            }
        }

    }
}

