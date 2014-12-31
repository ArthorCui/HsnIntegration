using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.CA
{
    public interface IValidationFunction
    {
        Func<IExpression, Dictionary<string, string>, ValidationResult> Function { get; set; }
        string ValidationKey { get; set; }
    }
}
