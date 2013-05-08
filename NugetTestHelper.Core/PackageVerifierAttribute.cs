using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace NuGetTestHelper
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class VerifierMetadataAttribute : ExportAttribute
    {
        public VerifierMetadataAttribute(string name, string action)
            : base(typeof(IPackageVerifierMetadata))
        {
            Name = name;
            Action = action;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Action
        {
            get;
            private set;
        }

      
    }
}
