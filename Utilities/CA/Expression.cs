using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections;

namespace Utilities.CA
{
    [Serializable]
    public class Expression : IExpression
    {
        #region Prop

        public string Id { get; set; }
        public string Key { get; set; }
        public object Value { get; set; }
        public virtual string FullKey { get { return GetFullKey(); } }
        public object DisplayValue { get; set; }
        public string Name { get; set; }
        public string CommandName { get; set; }
        public bool ManualExecute { get; set; }
        public bool HasDynamicParameter { get; set; }
        public DateTime ExecuteTime { get; set; }
        public double CostMilliseconds { get; set; }
        public double TotalEclapseTime { get; set; }
        public bool IsTopExpression { get { return this.ParentExpression == null; } }

        public Func<IExpression, object> ExpressionFunction { get; set; }
        public Dictionary<string, IExpression> Parameters { get; set; }
        public IList<IValidationFunction> ValidationFunctions { get; set; }
        public ValidationResultCollection ValidationsResult { get; set; }
        public IExpression ParentExpression { get; set; }
        public IExpression SubExpression { get; set; }
        public IExpressionMetadata ExpressionMetadata { get; set; }

        #endregion

        #region Ctor

        public Expression()
        {
            Id = Guid.NewGuid().ToString("N");
            Parameters = new Dictionary<string, IExpression>();
            ValidationFunctions = new List<IValidationFunction>();
            ValidationsResult = new ValidationResultCollection();
        }

        #endregion

        #region Validate & Clear

        private void Validate(bool isValidating)
        {
            ValidationResult validationResult;

            IList<IValidationParameter> parameters = new List<IValidationParameter>();
            if (this.ParentExpression != null)
                parameters = ValidationSetting.GetValidations(this.ParentExpression.Key, this.Name);

            foreach (var validation in ValidationFunctions)
            {
                var parameter = parameters.FirstOrDefault(p => p.ValidationKey == validation.ValidationKey);
                validationResult = validation.Function.Invoke(this, (parameter == null) ? null : parameter.Parameters);
                if (!validationResult.IsValidate)
                {
                    if (!isValidating)
                    {
                        throw new Exception(validationResult.ValidateMessage);
                    }
                    else
                    {
                        ValidationsResult.Add(validationResult);
                    }
                }
            }
        }

        public virtual void ClearValidationResult()
        {
            foreach (var parameter in Parameters)
            {
                parameter.Value.ClearValidationResult();
            }

            if (SubExpression != null)
            {
                SubExpression.ClearValidationResult();
            }

            ValidationsResult.Clear();
        }

        #endregion

        #region Revoke & Execute

        public void Revoke()
        {
            PreExecute();
            Execute();
            PostExecute();
        }

        public void PreExecute()
        {
            this.ExecuteTime = DateTime.Now;
        }

        public void PostExecute()
        {
            var span = DateTime.Now - this.ExecuteTime;

            // do something
            this.CostMilliseconds = span.TotalMilliseconds;
        }

        public virtual void Execute(bool isValidating = false)
        {
            if (ManualExecute) return;
            try
            {
                var startTime = DateTime.Now;
                //Evaluate parameters
                foreach (var parameter in Parameters)
                {
                    parameter.Value.Execute(isValidating);
                }
                //Evaluate subexpression
                if (SubExpression != null)
                {
                    SubExpression.Execute(isValidating);
                    Value = SubExpression.Value;
                }
                //Evaluate ExpressionFunction
                if (ExpressionFunction != null)
                {
                    Value = ExpressionFunction.Invoke(this);
                }
                //if (Logger.ShouldLog(Logger.Category.Trace, TraceEventType.Information))
                //{
                //    Logger.Info(string.Format("{0} = {1}, cost {2} ms", FullKey, Value, (DateTime.Now - startTime).TotalMilliseconds));
                //}
                //Validate value
                Validate(isValidating);
            }
            catch (ExpressionException ex)
            {
                throw new ExpressionException(string.Format("{0} has error:{1}", FullKey, ex.Message), ex) { Expression = ex.Expression };

            }
            catch (Exception ex)
            {
                throw new ExpressionException(string.Format("{0} has error:{1}", FullKey, ex.Message), this.GetCommandName(), ex) { Expression = this };

            }
        }

        public void IncParentCostTime(int costTime)
        {
            if (this.ParentExpression != null)
            {
                this.ParentExpression.TotalEclapseTime += costTime;

                this.ParentExpression.IncParentCostTime(costTime);
            }
        }

        #endregion

        #region Helper Get

        public T GetValue<T>()
        {
            //return (T) Value;
            Type type = this.Value != null ? this.Value.GetType() : null;
            Type targetType = typeof(T);
            if (typeof(Enum).IsAssignableFrom(typeof(T)))
            {
                return (T)Enum.Parse(typeof(T), Convert.ToString(this.Value));
            }
            if (this.Value is IConvertible)
            {
                return (T)Convert.ChangeType(this.Value, typeof(T));
            }
            //Check if the target type is generic list of value only
            if (type != null && type.IsGenericType && targetType.IsGenericType && !targetType.IsAssignableFrom(type))
            {
                return ConvertToListOfObjects<T>(this.Value as List<object>);
            }
            // Last try at desperation
            return (T)Value;
        }

        private static T ConvertToListOfObjects<T>(List<object> list)
        {
            Type eleType = typeof(T).GetGenericArguments().FirstOrDefault();
            var instance =
                (IList)typeof(List<>).MakeGenericType(eleType).GetConstructor(Type.EmptyTypes).Invoke(null);
            if (eleType != null)
            {
                var addMethod = instance.GetType().GetMethod("Add");
                list.ForEach(p => addMethod.Invoke(instance, new object[]
                {
                    p
                }));
            }
            return (T)instance;
        }

        public T GetNonEmptyValue<T>()
        {
            if (string.IsNullOrEmpty(this.Value.ToString()))
            {
                throw new ArgumentException(string.Format("{0} Value is null", Name));
            }
            return GetValue<T>();
        }

        protected virtual string GetFullKey()
        {
            if (ParentExpression != null)
            {
                return string.Format("{0}.[{1}]", ParentExpression.FullKey, this.Key);
            }
            return string.Format("[{0}]", Key);
        }

        public string GetCommandName()
        {
            var exp = this as IExpression;
            while (ParentExpression != null && string.IsNullOrEmpty(exp.CommandName))
            {
                exp = exp.ParentExpression;
            }
            return exp == null ? string.Empty : exp.CommandName;
        }

        #endregion

        #region Helper Write

        public virtual string ToXml()
        {
            var xml = string.Empty;
            var sw = new System.IO.StringWriter();
            var xw = new XmlTextWriter(sw);
            xw.Formatting = Formatting.Indented;
            xw.Indentation = 2;
            xw.IndentChar = ' ';
            WriteXml(xw);

            xml = sw.ToString();
            xw.Close();
            sw.Close();
            return xml;
        }

        public virtual void WriteXml(XmlTextWriter writer)
        {
            writer.WriteStartElement("Expression");

            writer.WriteAttributeString("Name", Name);
            //writer.WriteAttributeString("Description", ExpressionMetadata.Description);
            if (ManualExecute)
            {
                writer.WriteAttributeString("ManualExecute", ManualExecute.ToString());
            }
            writer.WriteAttributeString("ReturnDataType", ((ExpressionElement)ExpressionMetadata).ReturnDataType);
            if (((ExpressionElement)ExpressionMetadata).IsDynamic)
            {
                writer.WriteAttributeString("IsDynamic", ((ExpressionElement)ExpressionMetadata).IsDynamic.ToString());
            }
            if (!String.IsNullOrEmpty(CommandName))
            {
                writer.WriteAttributeString("CommandName", CommandName);
            }

            writer.WriteStartElement("Key");
            writer.WriteString(this.Key);
            writer.WriteEndElement();//end of "Key"

            if (Parameters.Count > 0)
            {
                writer.WriteStartElement("Parameters");
                foreach (var item in Parameters)
                {
                    item.Value.WriteXml(writer);
                }
                writer.WriteEndElement();//end of "Parameters"
            }

            writer.WriteEndElement();//end of "Expression"
        }

        #endregion
    }
}
