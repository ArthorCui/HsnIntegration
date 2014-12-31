using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.CA
{
    [Serializable]
    public class ExpressionParameterElement : IExpressionMetadata
    {
        #region Prop

        public string Key { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public string Filter { get; set; }
        public bool InheritDataType { get; set; }
        public string Name { get; set; }
        public string Default { get; set; }
        public string DefaultExpression { get; set; }
        public int Priority { get; set; }
        public string EnumSourceTypeName { get; set; }
        public string DynamicTypeLoader { get; set; }
        public bool ManualExecute { get; set; }

        #endregion

        public ExpressionParameterElement Clone()
        {
            return new ExpressionParameterElement
            {
                Key = Key,
                Description = Description,
                DataType = DataType,
                Filter = Filter,
                InheritDataType = InheritDataType,
                Name = Name,
                Default = Default,
                DefaultExpression = DefaultExpression,
                Priority = Priority,
                EnumSourceTypeName = EnumSourceTypeName,
                ManualExecute = ManualExecute,
            };
        }
    }
}
