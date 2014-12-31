using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.CA
{
    public class ValidationResult
    {
        public bool IsValidate { get; set; }
        public string ValidateMessage { get; set; }

        public ValidationResult()
        {
            IsValidate = true;
        }
    }
}
