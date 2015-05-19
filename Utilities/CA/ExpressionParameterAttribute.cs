using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.CA
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ExpressionParameterAttribute : Attribute
    {
        public Type EnumSource { get; set; }

        public string DynamicTypeLoader { get; set; } //in case options for this paramter need to be loaded by calling API and alike

        public string Description { get; set; }

        public string DataType { get; set; }

        public string Filter { get; set; }

        public bool InheritDataType { get; set; }

        public string Name { get; set; }

        public int Length { get; set; }

        public string Default { get; set; }

        public string DefaultExpression { get; set; }

        public int Priority { get; set; }

        public bool ManualExecute { get; set; }

        public ExpressionParameterAttribute()
        {
        }

        public ExpressionParameterAttribute(string name, string dataType)
        {
            Name = name;
            DataType = dataType;
        }
    }
}
