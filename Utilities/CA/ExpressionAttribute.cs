using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.CA
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ExpressionAttribute : Attribute
    {
        public string Key { get; set; }

        public string Description { get; set; }

        public string ReturnDataType { get; set; }

        public bool InheritDataType { get; set; }

        public string Filter { get; set; }

        public string Name { get; set; }

        public bool IsDynamic { get; set; }

        public ExpressionAttribute()
        {
        }

        public ExpressionAttribute(string key, string dataType)
        {
            Key = key;
            ReturnDataType = dataType;
        }
    }
}
