using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.CA
{
    public interface IExpressionService
    {
        Func<IExpression, object> GetService(string key);
        IList<ExpressionElement> ExpressionServiceMetadata(IList<string> assemblyNames);
    }
}
