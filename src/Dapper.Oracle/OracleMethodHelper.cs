using System;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;

namespace Dapper.Oracle
{
    internal class OracleMethodHelper
    {
        private static readonly ConcurrentDictionary<Type, OracleParameterProperties> CachedOracleTypes = new ConcurrentDictionary<Type, OracleParameterProperties>();
        private static readonly ConcurrentDictionary<Type, CommandProperties> CachedOracleCommandProperties = new ConcurrentDictionary<Type, CommandProperties>();

        public static void SetArrayBindCount(IDbCommand command, int arrayBindCount)
        {
            Get(command).ArrayBindCount.SetValue(command, arrayBindCount);
        }

        public static void SetInitialLOBFetchSize(IDbCommand command, int arrayBindCount)
        {
            Get(command).InitialLOBFetchSize.SetValue(command, arrayBindCount);
        }

        public static void SetBindByName(IDbCommand command, bool bindByName)
        {
            Get(command).BindByName.SetValue(command, bindByName);
        }

        public static void SetOracleParameters(IDbDataParameter parameter, OracleDynamicParameters.OracleParameterInfo oracleParameterInfo)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var method = CachedOracleTypes.GetOrAdd(parameter.GetType(), GetOracleProperties);

            if (oracleParameterInfo.DbType != null)
            {
                object value = (int)oracleParameterInfo.DbType;
                method.OraDbType.SetValue(parameter, value);
            }

            if (oracleParameterInfo.IsNullable.HasValue)
            {
                method.IsNullable.SetValue(parameter, oracleParameterInfo.IsNullable.Value);
            }

            if (oracleParameterInfo.Scale.HasValue)
            {
                method.Scale.SetValue(parameter, oracleParameterInfo.Scale.Value);
            }

            if (oracleParameterInfo.Precision.HasValue)
            {
                method.Precision.SetValue(parameter, oracleParameterInfo.Precision.Value);
            }

            method.SourceVersion.SetValue(parameter, oracleParameterInfo.SourceVersion);

            if (oracleParameterInfo.SourceColumn != null)
            {
                method.SourceColumn.SetValue(parameter, oracleParameterInfo.SourceColumn);
            }

            if (oracleParameterInfo.CollectionType != OracleMappingCollectionType.None)
            {
                method.CollectionType.SetValue(parameter, Enum.Parse(method.OracleCollectionEnumType, oracleParameterInfo.CollectionType.ToString()));
            }

            if (oracleParameterInfo.ArrayBindSize != null)
            {
                method.ArrayBindSize.SetValue(parameter, oracleParameterInfo.ArrayBindSize);
            }
        }

        internal static  OracleDynamicParameters.OracleParameterInfo GetParameterInfo(IDbDataParameter parameter)
        {
            var method = GetOracleProperties(parameter.GetType());
            var paramInfo = new OracleDynamicParameters.OracleParameterInfo();
            paramInfo.Name = parameter.ParameterName;
            paramInfo.SourceVersion = parameter.SourceVersion;
            paramInfo.Precision = parameter.Precision;
            paramInfo.Size = parameter.Size;
            
            paramInfo.DbType = (OracleMappingType)Enum.Parse(
                    typeof(OracleMappingType),
                    Enum.GetName(method.OracleDbEnumType, method.OraDbType.GetValue(parameter)));
            paramInfo.ArrayBindSize = (int[])method.ArrayBindSize.GetValue(parameter);
            paramInfo.CollectionType= (OracleMappingCollectionType)Enum.Parse(
                    typeof(OracleMappingCollectionType),
                    Enum.GetName(method.OracleCollectionEnumType, method.CollectionType.GetValue(parameter)));            
            paramInfo.ParameterDirection = parameter.Direction;
            paramInfo.IsNullable = parameter.IsNullable;
            paramInfo.Scale = parameter.Scale;
            paramInfo.SourceColumn = parameter.SourceColumn;
            paramInfo.Status = (OracleParameterMappingStatus)Enum.Parse(
                    typeof(OracleParameterMappingStatus),
                    Enum.GetName(method.OracleStatusEnumType,method.Status.GetValue(parameter)));
            paramInfo.Value = parameter.Value;

            return paramInfo;
        }
        
        private static OracleParameterProperties GetOracleProperties(Type type)
        {
            Assembly assembly = type.Assembly;
            var result = new OracleParameterProperties()
            {
                OraDbType = type.GetProperty("OracleDbType"),
                Size = type.GetProperty("Size"),
                IsNullable = type.GetProperty("IsNullable"),
                Precision = type.GetProperty("Precision"),
                Scale = type.GetProperty("Scale"),
                SourceColumn = type.GetProperty("SourceColumn"),
                SourceVersion = type.GetProperty("SourceVersion"),
                CollectionType = type.GetProperty("CollectionType"),
                ArrayBindSize = type.GetProperty("ArrayBindSize"),
                Status = type.GetProperty("Status")
            };
            switch (type.FullName)
            {
                case "Oracle.DataAccess.Client.OracleParameter":
                    result.OracleDbEnumType = assembly.GetType("Oracle.DataAccess.Client.OracleDbType");
                    result.OracleCollectionEnumType = assembly.GetType("Oracle.DataAccess.Client.OracleCollectionType");
                    result.OracleStatusEnumType = assembly.GetType("Oracle.DataAccess.Client.OracleParameterStatus");
                    break;

                case "Oracle.ManagedDataAccess.Client.OracleParameter":
                    result.OracleDbEnumType = assembly.GetType("Oracle.ManagedDataAccess.Client.OracleDbType");
                    result.OracleCollectionEnumType = assembly.GetType("Oracle.ManagedDataAccess.Client.OracleCollectionType");
                    result.OracleStatusEnumType = assembly.GetType("Oracle.ManagedDataAccess.Client.OracleParameterStatus");
                    break;
                default:
                    throw new NotSupportedException(
                        $"OracleDynamicParameters only works with Oracle objects, you cannot use this for parameter type {type.FullName}");
            }

            return result;
        }

        private static CommandProperties Get(IDbCommand command)
        {
            return CachedOracleCommandProperties.GetOrAdd(command.GetType(), type =>
            {
                var c = new CommandProperties
                {
                    ArrayBindCount = type.GetProperty("ArrayBindCount"),
                    InitialLOBFetchSize = type.GetProperty("InitialLOBFetchSize"),
                    BindByName = type.GetProperty("BindByName")
                };
                return c;
            });
        }


        private class OracleParameterProperties
        {
            public PropertyInfo OraDbType { get; set; }

            public PropertyInfo Size { get; set; }

            public PropertyInfo IsNullable { get; set; }

            public PropertyInfo Precision { get; set; }

            public PropertyInfo Scale { get; set; }

            public PropertyInfo ArrayBindSize { get; set; }

            public PropertyInfo SourceColumn { get; set; }

            public PropertyInfo SourceVersion { get; set; }

            public PropertyInfo CollectionType { get; set; }

            public PropertyInfo Status { get; set; }

            public Type OracleDbEnumType { get; set; }

            public Type OracleStatusEnumType { get; set; }

            public Type OracleCollectionEnumType { get; set; }
            
        }

        private class CommandProperties
        {
            public PropertyInfo InitialLOBFetchSize { get; set; }

            public PropertyInfo ArrayBindCount { get; set; }

            public PropertyInfo BindByName { get; set; }
        }
    }
}
