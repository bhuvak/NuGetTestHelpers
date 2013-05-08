using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetTestHelper
{
    public sealed class Constants
    {
        public const string InstallAction = "Install";
        public const string UninstallAction = "Uninstall";
        public const string UpgradeAction = "Upgrade";
        public const string PackageSourcesSectionName = "packageSources";
        public const string ActivePackageSourcesSectionName = "activePackageSource";
        public const string NuGetOfficialSourceFeedName = "NuGet official package source";
        public const string NuGetOfficialSourceFeedValue = "https://nuget.org/api/v2/";
    }

    public static class ProjectTemplates
    {
        #region StandardTemplateNames
        public static string MVC4RazorWebAppCsharp = "MvcWebApplicationProjectTemplate.11.cshtml.vstemplate";
        public static string MVC4CshtmlWebAppTemplateName = "MvcFacebookApplicationProjectTemplate.11.cshtml.vstemplate";
        public static string ConsoleAppCSharp = "csConsoleApplication.vstemplate";
        public static string WindowsFormAppCSharp = "csWindowsApplication.vstemplate";
        public static string PortableClassLibrary = "csPortableClassLibrary.vstemplate";
        public static string Windows8CSharp = "Microsoft.CS.WinRT.ClassLibrary";
        public static string SilverLight = "SilverlightClassLibrary.vstemplate";
        public static string WindowsPhone = "Discover.vstemplate";
        #endregion StandardTemplateNames
    }

    public static class ProjectTargetFrameworks
    {
        #region StandardTemplateNames
       public static string Net45 = "|$targetframeworkversion$=4.5";
       public static string Net40 = "|$targetframeworkversion$=4.0";
       public static string Net35 = "|$targetframeworkversion$=3.5";
       public static string Net20 = "|$targetframeworkversion$=2.0";
      
        #endregion StandardTemplateNames
    }
}
