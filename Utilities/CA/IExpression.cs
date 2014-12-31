using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Utilities.CA
{
    public interface IExpression
    {
        string Id { get; set; }
        string Key { get; set; }
        string FullKey { get; }
        string Name { get; set; }
        object Value { get; set; }
        object DisplayValue { get; set; }
        bool ManualExecute { get; set; }
        bool HasDynamicParameter { get; set; }
        string CommandName { get; set; }
        DateTime ExecuteTime { get; set; }
        double CostMilliseconds { get; set; }
        double TotalEclapseTime { get; set; }
        bool IsTopExpression { get; }

        IExpressionMetadata ExpressionMetadata { get; set; }
        Func<IExpression, object> ExpressionFunction { get; set; }
        Dictionary<string, IExpression> Parameters { get; set; }
        IList<IValidationFunction> ValidationFunctions { get; set; }
        ValidationResultCollection ValidationsResult { get; set; }
        IExpression ParentExpression { get; set; }
        IExpression SubExpression { get; set; }

        void IncParentCostTime(int costTime);
        void Execute(bool isValidating = false);
        void ClearValidationResult();
        T GetValue<T>();
        T GetNonEmptyValue<T>();
        string GetCommandName();

        string ToXml();
        void WriteXml(XmlTextWriter writer);
    }
}
