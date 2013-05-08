using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace NuGetTestHelper
{
    /// <summary>
    /// Helpers to update various sections of the Nuget config file
    /// </summary>
    public class NugetSettingsUtility
    {
        public static void SetPackageSources(IList<KeyValuePair<string,string>> sources)
        {
               //Clear existing sources to remove stale entries and add the current entries.
               ISettings settings = Settings.LoadDefaultSettings(null);
               settings.DeleteSection("packageSources");
               sources.Add(new KeyValuePair<string, string>("NuGet official package source","https://nuget.org/api/v2/"));
               settings.SetValues("packageSources", sources);
        }

        public static void SetActivePackageSource(string key, string value)
        {
            ISettings settings = Settings.LoadDefaultSettings(null);
            settings.DeleteSection("activePackageSource");
            settings.SetValue("activePackageSource", key, value);
        }
        public static void ClearAllPackageSources()
        {
            ISettings settings = Settings.LoadDefaultSettings(null);
            settings.DeleteSection("packageSources");
        }     

    }
}
