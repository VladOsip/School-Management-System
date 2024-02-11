using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySchool.ViewModel.Utilities
{
    /// <summary>
    /// A structure for a validity checks, contains information about the check and relevant information
    /// </summary>
    public struct ValidityResult
    {
        public bool Valid;
        public string ErrorReport;
    }
}
