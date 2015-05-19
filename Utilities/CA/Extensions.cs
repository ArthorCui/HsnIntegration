using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.CA
{
    public static class Extensions
    {
        public static string PlaceHolder(this object obj, IExpression expression)
        {
            var CHAR_TABLESPACE = ' ';
            var CHAR_ZERO = '0';

            if (expression.ExpressionMetadata != null)
            {
                var expressionParameter = (ExpressionParameterElement)expression.ExpressionMetadata;
                var dataType = expressionParameter.DataType;
                var length = expressionParameter.Length;
                var s = obj.ToString();

                switch (dataType)
                {
                    case "string":
                        if (s.Length == length) return s;
                        if (s.Length < length)
                        {
                            return s.PadLeft(length, CHAR_TABLESPACE);
                        }
                        else
                        {
                            throw new Exception(string.Format("Parameter {0} length {1} is out of range {2}", expressionParameter.Name, s.Length, length));
                        }
                    case "int":
                    case "long":
                        if (s.Length == length) return s;
                        if (s.Length < length)
                        {
                            return s.PadLeft(length, CHAR_ZERO);
                        }
                        else
                        {
                            throw new Exception(string.Format("Parameter {0} length {1} is out of range {2}", expressionParameter.Name, s.Length, length));
                        }

                    case "decimal":
                        s = (Convert.ToDecimal(s) * 100).ToString();
                        if (s.Length == length) return s;
                        if (s.Length < length)
                        {
                            return s.PadLeft(length, CHAR_ZERO);
                        }
                        else
                        {
                            throw new Exception(string.Format("Parameter {0} length {1} is out of range {2}", expressionParameter.Name, s.Length, length));
                        }

                }
            }
            return obj.ToString();
        }

        public static T Validate<T>(this T value, T maxValue, T minValue = default(T)) where T : IComparable<T>
        {
            if (value.CompareTo(maxValue) > 0 || value.CompareTo(minValue) < 0)
            {
                throw new Exception(string.Format("This value {0} is out of range", value));
            }
            return value;
        }
    }
}
