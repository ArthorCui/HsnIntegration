using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.CA
{
    public class ValidationParameter : IValidationParameter
    {
        public string ValidationKey { get; set; }

        public Dictionary<string, string> Parameters { get; set; }
    }
}
