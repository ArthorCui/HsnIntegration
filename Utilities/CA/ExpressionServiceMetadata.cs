using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.CA
{
    public class ExpressionServiceMetadata
    {
        public List<ExpressionElement> AllExpressions { get; set; }
        public List<ExpressionElement> Expressions { get; set; }
        public ExpressionServiceMetadata()
        {
            AllExpressions = new List<ExpressionElement>();
        }
    }
}
