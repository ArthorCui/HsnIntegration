using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.CA
{
    public interface IValidationParameter
    {
        string ValidationKey { get; set; }
        Dictionary<string, string> Parameters { get; set; }
    }
}
