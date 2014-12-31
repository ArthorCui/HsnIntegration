using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using System.IO;

namespace Utilities.CA
{
    public class ValidationSetting
    {
        private static XElement _validationSettings;

        private static XElement ValidationSettings
        {
            get
            {
                if (_validationSettings != null) return _validationSettings;

                string validationRulePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\ValidationRule";
                var filePathNames = Directory.GetFiles(validationRulePath, "*.ValidationRule.xml", SearchOption.AllDirectories);
                _validationSettings = new XElement("Expressions");

                foreach (var fileName in filePathNames)
                {
                    if (File.Exists(fileName))
                    {
                        var doc = XDocument.Load(fileName, LoadOptions.PreserveWhitespace);
                        _validationSettings.Add(doc.Element("Expressions").Elements("Expression"));
                    }
                }

                return _validationSettings;
            }
        }

        public static IList<string> GetValidationKey(string expressionKey, string parameterName)
        {
            var ret = new List<string>();
            if (ValidationSettings == null)
                return ret;
            var validationElems = ValidationSettings.Elements("Expression");
            foreach (var validationElem in validationElems)
            {
                if (validationElem.Element("Key").Value == expressionKey)
                {
                    var parameter = validationElem.Element("Parameters").Elements("Parameter").Where(t => t.Attribute("Name").Value == parameterName).FirstOrDefault();
                    if (parameter != null && parameter.Element("Validators") != null)
                    {
                        var validators = parameter.Element("Validators").Elements("Validator");

                        ret.AddRange(from validator in validators where !String.IsNullOrEmpty(validator.Value) select validator.Value);
                        return ret;
                    }

                }
            }
            return ret;
        }

        public static IList<IValidationParameter> GetValidations(string expressionKey, string parameterName)
        {
            var ret = new List<IValidationParameter>();
            if (ValidationSettings == null)
                return ret;
            var validationElems = ValidationSettings.Elements("Expression");
            foreach (var validationElem in validationElems)
            {
                if (validationElem.Element("Key").Value == expressionKey)
                {
                    var parameter = validationElem.Element("Parameters").Elements("Parameter").Where(t => t.Attribute("Name").Value == parameterName).FirstOrDefault();
                    if (parameter != null && parameter.Element("Validators") != null)
                    {
                        var validators = parameter.Element("Validators").Elements("Validator");

                        foreach (var validator in validators)
                        {
                            if (String.IsNullOrEmpty(validator.Value)) continue;
                            var key = validator.Value;
                            var parameters = validator.Attributes().ToDictionary(attribute => attribute.Name.LocalName, attribute => attribute.Value);
                            ret.Add(new ValidationParameter { ValidationKey = key, Parameters = parameters });
                        }

                        return ret;
                    }

                }
            }
            return ret;
        }
    }
}
