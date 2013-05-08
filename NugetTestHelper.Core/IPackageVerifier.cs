using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetTestHelper
{
    /// <summary>
    /// This interface need to be implementated by classes that to provide custom validations on package install/uninstall.
    /// </summary>
    public interface IPackageVerifier
    {
        /// <summary>
        /// This method would validate the specific aspect of package install/uninstall - say adding references, contents, opening readme.txt and so on ...
        /// "NugetValidationException should be thrown by this method when the validation fails.
        /// </summary>
        bool? Validate(string packageFullPath, VsProjectManager dteHelper);
        bool? HasSuceeded { get; set; }
        string Output { get;  }
        string Error { get;  }

    }

    public interface IPackageVerifierMetadata
    {
        string Name { get;}
        string Action { get;}
    }

}
