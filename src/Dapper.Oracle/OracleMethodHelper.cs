using System;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;

namespace Dapper.Oracle
{
    internal class OracleMethodHelper
    {        
        private static readonly ConcurrentDictionary<Type, OracleProperties> CachedOracleTypes = new ConcurrentDictionary<Type, OracleProperties>();
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
            Get(command).BindByName.SetValue(command,bindByName);            
        }

        public static void SetOracleParameters(IDbDataParameter parameter, OracleDynamicParameters.ParamInfo paramInfo)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var method = CachedOracleTypes.GetOrAdd(parameter.GetType(), GetOracleProperties);

            if (paramInfo.DbType != null)
            {
                object value = (int)paramInfo.DbType;
                method.OraDbType.SetValue(parameter, value);
            }

            if (paramInfo.IsNullable.HasValue)
            {
                method.IsNullable.SetValue(parameter, paramInfo.IsNullable.Value);
            }

            if (paramInfo.Scale.HasValue)
            {
                method.Scale.SetValue(parameter, paramInfo.Scale.Value);
            }

            if (paramInfo.Precision.HasValue)
            {
                method.Precision.SetValue(parameter, paramInfo.Precision.Value);
            }

            method.SourceVersion.SetValue(parameter, paramInfo.SourceVersion);

            if (paramInfo.SourceColumn != null)
            {
                method.SourceColumn.SetValue(parameter, paramInfo.SourceColumn);
            }

            if (paramInfo.CollectionType != OracleMappingCollectionType.None)
            {
                method.CollectionType.SetValue(parameter, Enum.Parse(method.OracleCollectionEnumType, paramInfo.CollectionType.ToString()));
            }
        }

        private static OracleProperties GetOracleProperties(Type type)
        {
            Assembly assembly = type.Assembly;
            var result = new OracleProperties()
            {
                OraDbType = type.GetProperty("OracleDbType"),
                Size = type.GetProperty("Size"),
                IsNullable = type.GetProperty("IsNullable"),
                Precision = type.GetProperty("Precision"),
                Scale = type.GetProperty("Scale"),
                SourceColumn = type.GetProperty("SourceColumn"),
                SourceVersion = type.GetProperty("SourceVersion"),
                CollectionType = type.GetProperty("CollectionType")
            };
            switch (type.FullName)
            {
                case "Oracle.DataAccess.Client.OracleParameter":
                    result.OracleDbEnumType = assembly.GetType("Oracle.DataAccess.Client.OracleDbType");
                    result.OracleCollectionEnumType = assembly.GetType("Oracle.DataAccess.Client.OracleCollectionType");
                    break;

                case "Oracle.ManagedDataAccess.Client.OracleParameter":
                    result.OracleDbEnumType = assembly.GetType("Oracle.DataAccess.Client.OracleDbType");
                    result.OracleCollectionEnumType = assembly.GetType("Oracle.ManagedDataAccess.Client.OracleCollectionType");
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


        private class OracleProperties
        {
            public PropertyInfo OraDbType { get; set; }

            public PropertyInfo Size { get; set; }

            public PropertyInfo IsNullable { get; set; }

            public PropertyInfo Precision { get; set; }

            public PropertyInfo Scale { get; set; }

            public PropertyInfo SourceColumn { get; set; }

            public PropertyInfo SourceVersion { get; set; }

            public PropertyInfo CollectionType { get; set; }

            public Type OracleDbEnumType { get; set; }

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
