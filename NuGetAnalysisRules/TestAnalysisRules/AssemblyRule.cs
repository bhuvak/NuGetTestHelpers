using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using NuGet;
using System.Runtime.InteropServices;

namespace TestAnalysisRules
{
    [Export(typeof(IPackageRule))]
    [Serializable]
    public class AssemblyRule : IPackageRule
    {
        #region PrivateMethods
        bool hasSignedIssue = false;
        byte[] buffer;
        /// <summary>
        /// Loads the assembly in a different appdomain.
        /// </summary>
        private void TempLoadAssembly()
        {
            AppDomain tempDomain = AppDomain.CreateDomain("TemporaryAppDomain");
            tempDomain.DoCallBack(new CrossAppDomainDelegate(LoaderCallback));
            AppDomain.Unload(tempDomain);
        }

        private void LoaderCallback()
        {
          var assembly =   Assembly.ReflectionOnlyLoad(buffer);
          AssemblyName assemblyName = assembly.GetName();
            byte[] publicKeytoken = assemblyName.GetPublicKeyToken();
            hasSignedIssue = (publicKeytoken == null || publicKeytoken.Length == 0);           
        }
        
        [DllImport("mscoree.dll", CharSet = CharSet.Unicode)]
        static extern bool StrongNameSignatureVerificationEx(string wszFilePath, bool fForceVerification, ref bool pfWasVerified);


        #endregion PrivateMethods
        /// <summary>
        /// Validates if all the dlls and exes present in the package is properly signed.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public IEnumerable<PackageIssue> Validate(IPackage package)
        {
            
            foreach (IPackageFile assemblyFile in package.GetFiles())
            {
            
                string extension = Path.GetExtension(assemblyFile.Path);
                string tempFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

                // REVIEW: Do we need to check .winmd file too?
                if (extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) || extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    bool hasSignedIssue = false;
                    try
                    {
                        using (Stream stream = assemblyFile.GetStream())
                        {
                            // write the assembly to a temporary file and load it
                             buffer = new byte[stream.Length];
                            stream.Read(buffer, 0, buffer.Length);
                            FileStream fs = new FileStream(tempFile,FileMode.Create);
                            for (int i=0; i< buffer.Length; i++)
                            fs.WriteByte(buffer[i]);
                            fs.Flush();
                            fs.Close();

                            //Load in different app domain and check for signing.
                            TempLoadAssembly();

                            //Check for delay signing.             
                            bool Forced = false;
                            bool verified = StrongNameSignatureVerificationEx(tempFile, true, ref Forced);
                            if ((!verified))
                                hasSignedIssue = true;                           
                            
                            // release memory early
                            buffer = null;
                        }
                    }
                    catch (Exception e)
                    {
                      
                    }
                    if (hasSignedIssue)
                    {                       
                            yield return new PackageIssue(
                                "Assembly not strongname signed",
                                "The assembly '" + assemblyFile.Path + "' in this package is either not signed or delay signed.",
                                "Strong name sign this assembly.",
                                PackageIssueLevel.Error);
                        
                    }
                }
            }
        }
    }
}