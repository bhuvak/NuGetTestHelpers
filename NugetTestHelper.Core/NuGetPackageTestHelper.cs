using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace NuGetTestHelper
{
    public class NuGetPackageTestHelper : IDisposable
    {
        public VsProjectManager vsProjectManager;
        public NuGetPackageManager nuGetPackageManager;
        [ImportMany(typeof(IPackageVerifier))]
        public IEnumerable <Lazy<IPackageVerifier, IPackageVerifierMetadata>> PackageInstallationVerifiers { get; set; }

        public NuGetPackageTestHelper()
        {
            this.vsProjectManager = new VsProjectManager();
            this.nuGetPackageManager = NuGetPackageManager.GetNuGetPackageManager(this.vsProjectManager);
            ImportValidationHelpers();
        }

        public bool VerifyPackageInstallation(string packageFullPath)
        {
            bool passed = true;
            foreach (var helper in PackageInstallationVerifiers)
            {
                if (helper.Metadata.Action.Equals(Constants.InstallAction, StringComparison.OrdinalIgnoreCase))
                {
                    bool? hasSuceeded = helper.Value.Validate(packageFullPath, this.vsProjectManager);
                    if (hasSuceeded == false)
                        passed = false; // assign it to false and continue with the next ValidationHelpers.
                }
            }
               return passed;                
        }

        public void Dispose()
        {        
            this.vsProjectManager.Dispose();
        }

        #region PrivateMembers
        /// <summary>
        /// Imports all the classes with extendes "INugetValidationHelper" using MEF.
        /// </summary>
        private void ImportValidationHelpers()
        {
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();
            //Adds all the parts found current directory.
            catalog.Catalogs.Add(new DirectoryCatalog(Environment.CurrentDirectory));
           // catalog.Catalogs.Add(new AssemblyCatalog(System.Reflection.Assembly.GetExecutingAssembly()));
            //Create the CompositionContainer with the parts in the catalog
            _container = new CompositionContainer(catalog);
            //Fill the imports of this object          
            this._container.ComposeParts(this);           
        }
     
        private CompositionContainer _container;
        #endregion PrivateMembers


    }
}
