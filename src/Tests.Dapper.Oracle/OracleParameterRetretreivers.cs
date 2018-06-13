using System;
using System.Data;
#if NETCOREAPP2_0
using Managed = Oracle.ManagedDataAccess.Client;
#else
using UnManaged = Oracle.DataAccess.Client;
using Managed = Oracle.ManagedDataAccess.Client;
#endif

namespace Tests.Dapper.Oracle
{
    public interface IOracleParameterRetretreiver
    {
        OracleParameterData GetParameter(object parameter);
    }

    public class OracleParameterData
    {
        public string ParameterName { get; set; }

        public string OracleDbType { get; set; }

        public string CollectionType { get; set; }

        public object Value { get; set; }

        public ParameterDirection Direction { get; set; }

        public int Size { get; set; }

        public bool IsNullable { get; set; }

        public int Scale { get; set; }

        public int Precision { get; set; }

        public string SourceColumn { get; set; }

        public int[] ArrayBindSize { get; set; }

        public DataRowVersion SourceVersion { get; set; }
    }
#if NETCOREAPP2_0
#else
    public class OracleUnmanagedParameterRetretreiver : IOracleParameterRetretreiver
    {
        public OracleParameterData GetParameter(object parameter)
        {
            var oraParam = (UnManaged.OracleParameter)parameter;
            return new OracleParameterData()
            {
                ParameterName = oraParam.ParameterName,
                OracleDbType = Enum.GetName(typeof(UnManaged.OracleDbType), oraParam.OracleDbType),
                CollectionType = Enum.GetName(typeof(UnManaged.OracleCollectionType), oraParam.CollectionType),
                Value = oraParam.Value,
                Direction = oraParam.Direction,
                Size = oraParam.Size,
                IsNullable = oraParam.IsNullable,
                Precision = oraParam.Precision,
                SourceColumn = oraParam.SourceColumn,
                SourceVersion = oraParam.SourceVersion,
                ArrayBindSize = oraParam.ArrayBindSize
            };
        }        
    }
#endif
    public class OracleManagedParameterRetretreiver : IOracleParameterRetretreiver
    {        

        public OracleParameterData GetParameter(object parameter)
        {
            var oraParam = (Managed.OracleParameter)parameter;
            return new OracleParameterData()
            {
                ParameterName = oraParam.ParameterName,
                OracleDbType = Enum.GetName(typeof(Managed.OracleDbType), oraParam.OracleDbType),
                CollectionType = Enum.GetName(typeof(Managed.OracleCollectionType), oraParam.CollectionType),
                Value = oraParam.Value,
                Direction = oraParam.Direction,
                Size = oraParam.Size,
                IsNullable = oraParam.IsNullable,
                Precision = oraParam.Precision,
                SourceColumn = oraParam.SourceColumn,
                SourceVersion = oraParam.SourceVersion,
                ArrayBindSize = oraParam.ArrayBindSize
            };
        }        
    }
}
