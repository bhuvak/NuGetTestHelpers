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
using System.Configuration;

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
        public static List<Tuple<string, string, string>> resultsDict = new List<Tuple<string, string, string>>();
        public static void Main(string[] args)
        {
            List<string> listOfPackages = new List<string>();
            try
            {
                if (args.Length == 0 || args[0].Equals("help", StringComparison.OrdinalIgnoreCase) || args[0].Equals("/?", StringComparison.OrdinalIgnoreCase))
                {
                    PrintHelp();
                    return;
                }

                if (args.Length > 1)
                {
                    string[] testmodevalues = args[1].Split(new char[] { ':' });
                    if (Enum.TryParse<TestMode>(testmodevalues[1],true, out currentMode))
                        currentMode = (TestMode)Enum.Parse(typeof(TestMode), testmodevalues[1],true);
                }
            }catch(Exception e)
            {
                Console.WriteLine("Error processing input arguments. Use Help command for more details and make sure you pass a valid folder location or .nupkg file as argument. Exception message : {0}",e.Message);
                //Environment.Exit(-1);
                return;
            }

            try
            {
                listOfPackages = GetListofPackageFileNames(args[0]);
            }catch(Exception e)
            {
                Console.WriteLine("Error in getting the list of .nupkg files from the input parameter {0}. Please check the input values. Exception message : {1}", args[0], e.Message);
            }
            foreach(string package in listOfPackages)
            {
                try
                {
                    string output = string.Empty;
                    string resultPath = string.Empty;
                    ValidatePackage validator = new ValidatePackage();
                    validator.Execute(package, currentMode,out output,out resultPath);
                    resultsDict.Add(new Tuple<string, string, string>(new ZipPackage(package).Id,output, resultPath));
                }catch(Exception e)
                {
                    Console.WriteLine("Error while validating package {0}. Exception message {1}. Stack trace : {2}", package, e.Message, e.StackTrace);
                }
            }

            DumpLog();
          
        }  

        public static void DumpLog()
        {
            HTMLLogger logger = new HTMLLogger(Path.Combine(AppSettingsHelper.TestResultsPathKeyValue,"Consolidated" + ".htm"));           
            logger.WriteTitle(" Consolidated report for NuGet package validation");
            logger.WriteTestCaseResultTableHeader(new string[] { "Package", "Result", "Result file Path" });
            foreach (Tuple<string, string, string> result in resultsDict)
            {
                logger.WriteTestCaseResultWithoutLink(result.Item1, result.Item2, result.Item3);                
            }
            logger.WriteEnd();
            if(!string.IsNullOrEmpty(ConfigurationManager.AppSettings["SmtpUserName"]) && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["SmtpPassword"]) && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["MailRecepientAddress"]))
              MailHelper.SendMail(logger.stringwriter.ToString());
            logger.Dispose();
        }

        public static void PrintHelp()
        {
            Console.WriteLine("");
            Console.WriteLine("Usage : NuGetPackageValidator <Path to the Package> [-testmode:install|static|all]");
            Console.WriteLine("");
            Console.WriteLine("Path to Package : Full path to a single nupkg file or full path pointing to a folder having a setting a nupkg files.");
            Console.WriteLine("");
            Console.WriteLine(@"Example: \\fc-fileserv\Mypackage.1.0.0.nupkg , C:\Packages");
            Console.WriteLine("");
            Console.WriteLine("Test mode : This is optional parameter. Specify 'install' if you want to run only package installation test and 'static' if you want to run only static analysis tests");
            Console.WriteLine("Default value is 'All' and both of them will be run");
        }

        private static List<string> GetListofPackageFileNames(string inputPath)
        {
            List<string> listOfPackages = new List<string>();
            string fileExt = Path.GetExtension(inputPath);
            if(string.IsNullOrEmpty(fileExt))
            {
                listOfPackages = Directory.GetFiles(inputPath, "*.nupkg", SearchOption.TopDirectoryOnly).ToList();
            }
            else if(fileExt.Equals(".nupkg"))
            {
                listOfPackages.Add(inputPath);
            }
            else
            {
                throw new ArgumentException("Not a valid nupkg file specified");
            }
            if (listOfPackages == null || listOfPackages.Count == 0)
                throw new ArgumentException("The specified directory doesn't have any nupkg files at the root level");
            return listOfPackages;
        }
    }
}
