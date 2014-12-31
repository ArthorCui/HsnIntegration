using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.CA
{
    public class ValidationResultCollection : List<ValidationResult>
    {
        public bool IsValidate
        {
            get
            {
                return this.Aggregate(true, (current, result) => current & result.IsValidate);
            }
        }

        public ValidationResultCollection()
        {

        }

        public ValidationResultCollection(IEnumerable<ValidationResult> results)
        {
            this.AddRange(results);
        }
    }
}
