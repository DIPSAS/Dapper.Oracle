using System;
using System.Data;

namespace Dapper.Oracle.TypeHandler
{
    
    /// <summary>
    /// Conversion between <see cref="Boolean"/> in .net and NUMBER(1) oracle data type.
    /// Numeric value 0 is false, any other value equals true.
    /// </summary>
    public class BooleanNumericTypeHandler  : TypeHandlerBase<bool>
    {
        public override void SetValue(IDbDataParameter parameter, bool value)
        {
            SetOracleDbTypeOnParameter(parameter,"Int16");
            parameter.Value = value ? 1 : 0;
        }

        public override bool Parse(object value)
        {
            return ((int) value) != 0;
        }                            
    }

    internal static class NumericExtensions
    {
        internal static bool IsNumber(this object value)
        {
            return value is sbyte
                   || value is byte
                   || value is short
                   || value is ushort
                   || value is int
                   || value is uint
                   || value is long
                   || value is ulong
                   || value is float
                   || value is double
                   || value is decimal;
        }
    }
}