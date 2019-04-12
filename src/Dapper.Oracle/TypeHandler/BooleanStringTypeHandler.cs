using System;
using System.Data;

namespace Dapper.Oracle.TypeHandler
{
    /// <summary>
    /// Conversion between <see cref="bool"/> and Oracle VARCHAR2    
    /// </summary>
    public class BooleanStringTypeHandler : TypeHandlerBase<bool>
    {
        private readonly string trueString;
        private readonly string falseString;
        private readonly StringComparison compareMode;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="trueValue">Mapping value to use in database for a boolean true value</param>
        /// <param name="falseValue">Mapping value to use in database for a boolean false value</param>
        public BooleanStringTypeHandler(string trueValue, string falseValue,StringComparison comparison = StringComparison.Ordinal)
        {
            trueString = trueValue;
            falseString = falseValue;
            compareMode = comparison;
        }

        public override void SetValue(IDbDataParameter parameter, bool value)
        {
            SetOracleDbTypeOnParameter(parameter, "Varchar2");
            parameter.Value = value ? trueString : falseString;
        }

        public override bool Parse(object value)
        {
            if (value is string valuestring)
            {
                if (valuestring.Equals(trueString, compareMode))
                {
                    return true;
                }
                
                if(valuestring.Equals(falseString,compareMode))
                {
                    return false;
                }
                
                throw new NotSupportedException($"'{valuestring}' was unexpected - expected '{trueString}' or '{falseString}'");                
            }
            
            throw new NotSupportedException($"Dont know how to convert a {value.GetType()} to a Boolean");
        }       
    }
}