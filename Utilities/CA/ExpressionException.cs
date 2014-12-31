using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.CA
{
    public class ExpressionException : Exception
    {
        public string CommandName { get; set; }
        public IExpression Expression { get; set; }

        #region Ctor

        public ExpressionException()
        {
        }

        public ExpressionException(string message)
            : base(message)
        {
        }

        public ExpressionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ExpressionException(string message, string commandName, Exception innerException)
            : base(message, innerException)
        {
            this.CommandName = commandName;
        }

        #endregion
    }
}
