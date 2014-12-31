using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.CA
{
    [Serializable]
    public class ExpressionElement : IExpressionMetadata
    {
        #region Prop

        public string Key { get; set; }
        public string Description { get; set; }
        public string ReturnDataType { get; set; }
        public bool InheritDataType { get; set; }
        public string Filter { get; set; }
        public string Name { get; set; }
        public bool IsDynamic { get; set; }
        public List<ExpressionParameterElement> Parameters { get; set; }
        public string AssemblyName { get; set; }

        #endregion

        public ExpressionElement()
        {
            this.Parameters = new List<ExpressionParameterElement>();
        }

        public ExpressionElement Clone()
        {
            var parameters = new List<ExpressionParameterElement>();
            foreach (var parameter in Parameters)
            {
                parameters.Add(parameter.Clone());
            }

            return new ExpressionElement
            {
                Key = Key,
                Description = Description,
                ReturnDataType = ReturnDataType,
                InheritDataType = InheritDataType,
                Filter = Filter,
                Name = Name,
                IsDynamic = IsDynamic,
                Parameters = parameters,
                AssemblyName = AssemblyName,
            };
        }
    }
}
