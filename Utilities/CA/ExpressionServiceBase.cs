using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using PayMedia.Integration.FrameworkService.Interfaces.Common;

namespace Utilities.CA
{
    public class ExpressionServiceBase : IExpressionService
    {
        public Dictionary<string, Func<IExpression, object>> ExpressionServices = new Dictionary<string, Func<IExpression, object>>();
        protected readonly ExpressionServiceMetadata _expressionServiceMetadata = new ExpressionServiceMetadata();

        public ExpressionServiceBase()
        {
            var methods = this.GetType().GetMethods();
            foreach (var methodInfo in methods)
            {
                var attributes = methodInfo.GetCustomAttributes(typeof(ExpressionAttribute), true);
                if (attributes.Length > 0)
                {
                    var attribute = attributes[0] as ExpressionAttribute;
                    var exp = BuildCurrentExpressionElement(methodInfo, attribute);

                    var paramterAttributes = methodInfo.GetCustomAttributes(typeof(ExpressionParameterAttribute), true);
                    foreach (var o in paramterAttributes)
                    {
                        var parameterAttribute = o as ExpressionParameterAttribute;
                        BuildDefaultExpressionParameterElement(exp, parameterAttribute);
                    }

                    exp.Parameters = exp.Parameters.OrderBy(p => p.Priority).ToList();
                    _expressionServiceMetadata.AllExpressions.Add(exp);
                }
            }

            _expressionServiceMetadata.AllExpressions = _expressionServiceMetadata.AllExpressions.OrderBy(s => s.Key).ToList();
        }

        public virtual Func<IExpression, object> GetService(string key)
        {
            return (ExpressionServices.ContainsKey(key)) ? ExpressionServices[key] : null;
        }

        protected string GetBaseCommandName(IExpression expression)
        {
            string commandName = expression.GetCommandName();
            if (string.IsNullOrEmpty(commandName))
            {
                commandName = DataContext.Current.MessageContext.Event[IFEventPropertyNames.PROV_EXT_CMD_ID];
            }
            return commandName;
        }

        public virtual IList<ExpressionElement> ExpressionServiceMetadata(IList<string> assemblyNames)
        {
            if (assemblyNames == null)
            {
                _expressionServiceMetadata.Expressions = _expressionServiceMetadata.AllExpressions;
                return _expressionServiceMetadata.Expressions;
            }
            _expressionServiceMetadata.Expressions = (from e in _expressionServiceMetadata.AllExpressions
                                                      where assemblyNames.Contains(e.AssemblyName)
                                                      select e).ToList();
            return _expressionServiceMetadata.Expressions;
        }

        public virtual object SendCommandToPipeline(IExpression expression)
        {
            //var cmd = expression.Parameters["Command"].GetValue<CACommand>();
            //if (DataContext.Current.IsValidating)
            //    return cmd.GenerateMessage();

            //var types = cmd.KnownTypes;
            //var cmdString = SerializationUtilities<CACommand>.DataContract.Serialize(cmd, types);


            //DataContext.Current.MessageContext.Msg[Constants.CA_COMMAND_KEY] = cmdString;
            //DataContext.Current.MessageContext.Msg[Constants.CONFIG_EXPRESSION_SETTING_ID] = expression.Parameters["PipelineData"].GetValue<string>();

            //DataContext.Current.MessageContext.Services.MsgEnqueuer.SendToQueue(DataContext.Current.MessageContext, expression.Parameters["PipelineKey"].GetValue<string>(), EventPriority.High);

            return "";
        }

        protected ExpressionElement BuildCurrentExpressionElement(MethodInfo methodInfo, ExpressionAttribute expressionAttribute)
        {
            if (ExpressionServices.ContainsKey(expressionAttribute.Key))
            {
                throw new Exception(string.Format("The expression service with key '{0}' has already existed.", expressionAttribute.Key));
            }

            var parameters = methodInfo.GetParameters();
            if (parameters.Length == 1 && typeof(IExpression).IsAssignableFrom(parameters[0].ParameterType))
            {
                var func = (Func<IExpression, object>)Delegate.CreateDelegate(typeof(Func<IExpression, object>), this, methodInfo);
                ExpressionServices.Add(expressionAttribute.Key, func);
            }

            expressionAttribute.Name = (string.IsNullOrEmpty(expressionAttribute.Name)) ? methodInfo.Name : expressionAttribute.Name;
            var exp = new ExpressionElement
            {
                ReturnDataType = expressionAttribute.ReturnDataType,
                Description = expressionAttribute.Description,
                Filter = expressionAttribute.Filter,
                Key = expressionAttribute.Key,
                Name = expressionAttribute.Name,
                AssemblyName = this.GetType().Assembly.ManifestModule.Name,
                IsDynamic = expressionAttribute.IsDynamic,
                InheritDataType = expressionAttribute.InheritDataType
            };

            return exp;
        }

        protected void BuildDefaultExpressionParameterElement(ExpressionElement exp, ExpressionParameterAttribute expressionParameterAttribute)
        {
            if (expressionParameterAttribute != null)
            {
                exp.Parameters.Add(new ExpressionParameterElement
                {
                    DataType = expressionParameterAttribute.DataType,
                    Description = expressionParameterAttribute.Description,
                    Filter = expressionParameterAttribute.Filter,
                    Key = expressionParameterAttribute.Name,
                    Name = expressionParameterAttribute.Name,
                    Default = expressionParameterAttribute.Default,
                    DefaultExpression = expressionParameterAttribute.DefaultExpression,
                    Priority = expressionParameterAttribute.Priority,
                    ManualExecute = expressionParameterAttribute.ManualExecute,
                    EnumSourceTypeName = expressionParameterAttribute.EnumSource == null ? null : expressionParameterAttribute.EnumSource.AssemblyQualifiedName,
                    DynamicTypeLoader = expressionParameterAttribute.DynamicTypeLoader,
                    InheritDataType = expressionParameterAttribute.InheritDataType,
                });
            }
        }
    }
}
