using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.CA
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ExpressionTemplateAttribute : Attribute
    {
        public string Key { get; set; }

        public string ParameterDataType { get; set; }

        public ExpressionTemplateAttribute()
        {
        }

        public ExpressionTemplateAttribute(string key, string parameterDataType)
        {
            this.Key = key;
            this.ParameterDataType = parameterDataType;
        }

    }
}
