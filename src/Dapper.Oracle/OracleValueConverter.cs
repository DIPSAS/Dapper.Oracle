using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace Dapper.Oracle
{
    internal static class OracleValueConverter
    {
        /// <summary>
        /// Convert the value to the provided generic type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T Convert<T>(object value)
        {
            value = GetValue(value);

            if (value == null || value == DBNull.Value)
            {
                return default(T);
            }

            // Convert the Oracle native data type to .NET data type.
            // See: https://docs.oracle.com/en/database/oracle/oracle-database/19/clrnt/datatype-conversion.html#GUID-70A2F34D-AB7F-4E0C-89C9-452A45FF1CAC
            if (value is IConvertible)
            {
                var nullableType = Nullable.GetUnderlyingType(typeof(T));
                return (T)System.Convert.ChangeType(value, nullableType ?? typeof(T));
            }

            return default(T);
        }

        /// <summary>
        /// Gets the underlying primitive value from a nested type instance.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static object GetValue(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            // Recursively handle nested database parameters.
            // An OracleParameter can have a value that is an instance of an Oracle data structure (OracleBlob, OracleDecimal, etc.).
            // This assumes we want the underlying primitive value of the parameter.
            while (value is DbParameter parameter)
            {
                value = parameter.Value;
            }

            Type valueType = value.GetType();

            if (IsOracleDataStructure(valueType))
            {
                var isNull = (bool)valueType.GetProperty("IsNull", typeof(bool))?.GetValue(value);

                if (isNull)
                {
                    return null;
                }

                value = valueType.GetProperty("Value")?.GetValue(value);
            }

            return value;
        }

        /// <summary>
        /// Determine if the type is an Oracle data structure.
        /// </summary>
        /// <param name="valueType"></param>
        /// <returns></returns>
        private static bool IsOracleDataStructure(Type valueType)
        {
            // We need this, because, you know, Oracle.  OracleDecimal,OracleFloat,OracleYaddiAddy,OracleYourUncle etc value types.    
            // See: https://docs.oracle.com/en/database/oracle/oracle-database/19/odpnt/intro003.html#GUID-425C9EBA-CFFC-47FE-B490-604251714ACA
            return Regex.IsMatch(valueType.FullName, @"Oracle\.\w+\.Types\.Oracle\w+");
        }
    }
}
