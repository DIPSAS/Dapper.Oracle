using System;
using System.Data;

namespace Dapper.Oracle.TypeHandler
{
    /// <summary>
    /// Conversion between <see cref="Guid"/> and RAW(16) Oracle data type 
    /// </summary>
    public class GuidRaw16TypeHandler : TypeHandlerBase<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            SetOracleDbTypeOnParameter(parameter, "Raw", 16);
            parameter.Value = value.ToByteArray();
        }

        public override Guid Parse(object value)
        {
            if (value is byte[] bytearray)
            {
                return new Guid(bytearray);
            }

            throw new NotImplementedException($"Dont know how to convert a {value.GetType().Name} to a System.Guid.");
        }              
    }
}