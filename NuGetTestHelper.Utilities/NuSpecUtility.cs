using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;
using System.IO;
using System.Text.RegularExpressions;

namespace NuGetTestHelper
{
    /// <summary>
    /// Utilities to Update NuSpec File
    /// </summary>
    internal class NuSpecUtility
    {   
        internal static void UpdateNuspecFile(string NuspecFilepath,string searchString, string replacementString)
        {    
            if (File.Exists(NuspecFilepath))
            {
                string specText = File.ReadAllText(NuspecFilepath);
                specText = specText.Replace(searchString, replacementString);      
                File.WriteAllLines(NuspecFilepath, new string[] { specText });
            }
        }

        internal static void UpdateDependency(string NuspecFilepath, string name, string oldVersion, string version)
        {
            //Update contents Nuspec file.
            StringBuilder replacementString = new StringBuilder(@"<dependency id=" + @"""" + name + @"""" + " version=" + @"""" + version + @"""" + @" />  </dependencies>");
            StringBuilder searchString = new StringBuilder(@"</dependencies>");
            UpdateNuspecFile(NuspecFilepath, searchString.ToString(), replacementString.ToString());
        }       
    }
}
