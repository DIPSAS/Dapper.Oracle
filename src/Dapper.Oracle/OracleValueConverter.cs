﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dapper.Oracle
{
    internal static class OracleValueConverter
    {
        private static string[] OracleStringTypes { get; } = { "Oracle.DataAccess.Types.OracleString", "Oracle.ManagedDataAccess.Types.OracleString" };

        public static T Convert<T>(object val)
        {
            if (val == null)
            {
                return default(T);
            }

            if (val == DBNull.Value)
            {
                if (default(T) != null)
                {
                    throw new ApplicationException("Attempting to cast a DBNull to a non nullable type!");
                }

                return default(T);
            }

            Type valueType = val.GetType();
            if (typeof(T) == typeof(string) && OracleStringTypes.Contains(valueType.FullName))
            {
                // Need this, because... you know,Oracle...
                val = val.ToString();

                if ((string)val == "null")  // Seriously.  W.T.F ????
                {
                    return default(T);
                }

                return (T)val;
            }

            // We need this, because, you know, Oracle.  OracleDecimal,OracleFloat,OracleYaddiAddy,OracleYourUncle etc value types.            
            if (Regex.IsMatch(valueType.FullName, @"Oracle\.\w+\.Types\.Oracle\w+"))
            {
                // Fix Oracle 11g (to not throw the exception "Invalid operation on null data" if a function returns NULL)
                var isNullProperty = valueType.GetProperty("IsNull");
                if (isNullProperty != null && isNullProperty.CanRead)
                {
                    var isNull = (bool)isNullProperty.GetValue(val);
                    if (isNull)
                    {
                        if (default(T) != null)
                        {
                            throw new ApplicationException("Attempting to cast a DBNull to a non nullable type!");
                        }
                        return default(T);
                    }
                    // If not isNull, continue and get the Value
                }
                
                var valueProperty = valueType.GetProperty("Value");
                if (valueProperty != null && valueProperty.CanRead)
                {
                    object reflected = valueProperty.GetValue(val);
                    return (T)reflected;
                }
            }

            return (T)val;
        }
    }
}
