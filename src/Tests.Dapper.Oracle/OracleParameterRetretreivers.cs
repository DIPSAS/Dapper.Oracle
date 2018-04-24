using System;
using System.Data;
using UnManaged = Oracle.DataAccess.Client;
using Managed = Oracle.ManagedDataAccess.Client;

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

        public DataRowVersion SourceVersion { get; set; }
    }

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
                SourceVersion = oraParam.SourceVersion
            };
        }        
    }

    public class OracleManagedParameterRetretreiver : IOracleParameterRetretreiver
    {        

        public OracleParameterData GetParameter(object parameter)
        {
            var oraParam = (Managed.OracleParameter)parameter;
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
                SourceVersion = oraParam.SourceVersion
            };
        }        
    }
}
