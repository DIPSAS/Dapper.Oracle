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

            var valueType = value.GetType();
            var nullableType = Nullable.GetUnderlyingType(typeof(T));

            // Convert the Oracle native data type to .NET data type.
            // See: https://docs.oracle.com/en/database/oracle/oracle-database/19/clrnt/datatype-conversion.html#GUID-70A2F34D-AB7F-4E0C-89C9-452A45FF1CAC
            if (value is IConvertible)
            {
                return (T)System.Convert.ChangeType(value, nullableType ?? typeof(T));
            }

            // Convert the Oracle Array native data type to .NET Array data type.
            // For example OracleString[] to string[] or
            // For example OracleDecimal[] to decimal[] or
            if (typeof(T).BaseType == typeof(Array) || valueType.BaseType == typeof(Array))
            {
                value = ConvertArray<T>(value);
                return (T)System.Convert.ChangeType(value, nullableType ?? typeof(T));
            }

            return default(T);
        }

        /// <summary>
        /// OracleString[] does not implement IConvertible therefore this is the only way to convert OracleString[] to string[] or OracleDecimal[] to .NET native data type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        private static T ConvertArray<T>(object value)
        {
            var nullableType = Nullable.GetUnderlyingType(typeof(T));
            var arr = (Array)value;

            switch (typeof(T).FullName)
            {
                case "Oracle.ManagedDataAccess.Types.OracleString[]":
                    return (T)System.Convert.ChangeType(value, nullableType ?? typeof(T));

                case "System.Int16[]":
                    var shortArray = new short[arr.Length];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        shortArray[i] = short.Parse(arr.GetValue(i).ToString());
                    }
                    return (T)System.Convert.ChangeType(shortArray, nullableType ?? typeof(T));

                case "System.Int32[]":
                    var intArray = new int[arr.Length];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        intArray[i] = int.Parse(arr.GetValue(i).ToString());
                    }
                    return (T)System.Convert.ChangeType(intArray, nullableType ?? typeof(T));

                case "System.Int64[]":
                    var longArray = new long[arr.Length];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        longArray[i] = long.Parse(arr.GetValue(i).ToString());
                    }
                    return (T)System.Convert.ChangeType(longArray, nullableType ?? typeof(T));

                case "System.Decimal[]":
                case "Oracle.ManagedDataAccess.Types.OracleDecimal[]":
                    var decimalArray = new decimal[arr.Length];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        decimalArray[i] = decimal.Parse(arr.GetValue(i).ToString());
                    }
                    return (T)System.Convert.ChangeType(decimalArray, nullableType ?? typeof(T));

                default:
                    var strArray = new string[arr.Length];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        strArray[i] = arr.GetValue(i).ToString();
                    }
                    return (T)System.Convert.ChangeType(strArray, nullableType ?? typeof(T));
            }
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
                var isNullProp = valueType.GetProperty("IsNull", typeof(bool));
                var isNull = false;

                if (isNullProp != null)
                {
                    var propValue = isNullProp?.GetValue(value);
                    if (propValue != null)
                    {
                        isNull = (bool)propValue;
                    }
                }

                if (isNull)
                {
                    return null;
                }

                var val = valueType.GetProperty("Value")?.GetValue(value);
                if (val != null)
                {
                    value = val;
                }
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
