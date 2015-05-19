using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Utilities.CA
{
    public partial class Expression_Service : ExpressionServiceBase
    {
        public MethodInfo[] MethodInfoList { get { return this.GetType().GetMethods(); } }

        public Dictionary<string, ExpressionElement> TemplateDic = new Dictionary<string, ExpressionElement>();

        public Expression_Service()
        {
            Refresh();
            Initialize();
        }

        protected void Initialize()
        {
            foreach (var methodInfo in MethodInfoList)
            {
                var attributes = methodInfo.GetCustomAttributes(typeof(ExpressionAttribute), true);
                if (attributes.Length > 0)
                {
                    var expressionAttribute = attributes[0] as ExpressionAttribute;
                    var exp = BuildCurrentExpressionElement(methodInfo, expressionAttribute);

                    var expreesionTemplateAttribute = methodInfo.GetCustomAttributes(typeof(ExpressionTemplateAttribute), true);

                    var paramterAttributes = methodInfo.GetCustomAttributes(typeof(ExpressionParameterAttribute), true);
                    BuildExpressionParameterElement(exp, expreesionTemplateAttribute, paramterAttributes);

                    exp.Parameters = exp.Parameters.OrderBy(p => p.Priority).ToList();
                    _expressionServiceMetadata.AllExpressions.Add(exp);
                }
            }
        }

        private void BuildExpressionParameterElement(ExpressionElement exp, object[] expreesionTemplateAttribute, object[] paramterAttributes)
        {
            if (expreesionTemplateAttribute.Length > 0)
            {
                var templateAttribute = expreesionTemplateAttribute.SingleOrDefault() as ExpressionTemplateAttribute;
                foreach (var o in paramterAttributes)
                {
                    var parameterAttribute = o as ExpressionParameterAttribute;
                    BuildCurrentExpressionParameterElement(exp, templateAttribute, parameterAttribute);
                }
            }
            else
            {
                foreach (var o in paramterAttributes)
                {
                    var parameterAttribute = o as ExpressionParameterAttribute;
                    BuildDefaultExpressionParameterElement(exp, parameterAttribute);
                }
            }
        }

        protected void Refresh()
        {
            foreach (var item in _expressionServiceMetadata.AllExpressions)
            {
                ExpressionServices.Remove(item.Key);
            }

            _expressionServiceMetadata.AllExpressions.RemoveAll(p => p.AssemblyName.Equals(this.GetType().Assembly.ManifestModule.Name));
        }

        protected void BuildCurrentExpressionParameterElement(ExpressionElement exp, ExpressionTemplateAttribute templateAttribute, ExpressionParameterAttribute parameterAttribute)
        {
            var key = string.Format("{0}.{1}", templateAttribute.Key, parameterAttribute.Name);

            if (TemplateDic.ContainsKey(key))
            {
                TemplateDic.TryGetValue(key, out exp);
            }
            else
            {
                if (parameterAttribute != null)
                {
                    foreach (var item in exp.Parameters)
                    {
                        //exp.Parameters.Add(new ExpressionParameterElement() { Default = ""});
                        TemplateDic.Add(key, exp);
                    }
                }
            }

        }

        private ExpressionParameterElement SetCurrentExpressionParameterElement()
        {
            var parameterElement = new ExpressionParameterElement() { Default = "" };
            return null;
        }

        public override IList<ExpressionElement> ExpressionServiceMetadata(IList<string> assemblyNames)
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
    }
}
