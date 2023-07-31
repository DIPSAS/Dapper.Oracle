using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Dapper.Oracle.Expressions;
using Dapper.Oracle.Util;

namespace Dapper.Oracle
{



    internal class OracleMethodHelper
    {
        private static readonly ConcurrentDictionary<Type, OracleParameterExpressions> CachedOracleTypes = new ConcurrentDictionary<Type, OracleParameterExpressions>();
        private static readonly ConcurrentDictionary<Type, CommandExpressions> CachedOracleCommandProperties = new ConcurrentDictionary<Type, CommandExpressions>();

        public static void SetArrayBindCount(IDbCommand command, int arrayBindCount)
        {
            IDbCommand actual = command.IsWrapped() ? command.DownCastCommand() : command;
            Get(actual).ArrayBindCount.SetValue(actual, arrayBindCount);
        }

        public static void SetInitialLOBFetchSize(IDbCommand command, int arrayBindCount)
        {
            IDbCommand actual = command.IsWrapped() ? command.DownCastCommand() : command;
            Get(actual).InitialLOBFetchSize.SetValue(actual, arrayBindCount);
        }

        public static void SetBindByName(IDbCommand command, bool bindByName)
        {
            IDbCommand actual = command.IsWrapped() ? command.DownCastCommand() : command;
            Get(actual).BindByName.SetValue(actual, bindByName);
        }

        public static void SetOracleParameters(IDbDataParameter parameter, OracleDynamicParameters.OracleParameterInfo oracleParameterInfo)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var method = CachedOracleTypes.GetOrAdd(parameter.GetType(), GetOracleProperties);

            if (oracleParameterInfo.DbType.HasValue)
            {
                method.OraDbType.SetValue(parameter, oracleParameterInfo.DbType.Value);
            }

            if (oracleParameterInfo.IsNullable.HasValue)
            {
                method.IsNullable.SetValue(parameter,oracleParameterInfo.IsNullable.Value);                
            }

            if (oracleParameterInfo.Scale.HasValue)
            {
                parameter.Scale = oracleParameterInfo.Scale.Value;
            }

            if (oracleParameterInfo.Precision.HasValue)
            {
                parameter.Precision = oracleParameterInfo.Precision.Value;
            }

            parameter.SourceVersion = oracleParameterInfo.SourceVersion;


            if (oracleParameterInfo.SourceColumn != null)
            {
                parameter.SourceColumn = oracleParameterInfo.SourceColumn;
            }

            if (oracleParameterInfo.CollectionType != OracleMappingCollectionType.None)
            {
                method.CollectionType.SetValue(parameter, oracleParameterInfo.CollectionType);
            }

            if (oracleParameterInfo.ArrayBindSize != null)
            {
                method.ArrayBindSize.SetValue(parameter, oracleParameterInfo.ArrayBindSize);
            }
        }

        internal static OracleDynamicParameters.OracleParameterInfo GetParameterInfo(IDbDataParameter parameter)
        {
            var method = GetOracleProperties(parameter.GetType());
            var paramInfo = new OracleDynamicParameters.OracleParameterInfo();
            paramInfo.Name = parameter.ParameterName;
            paramInfo.SourceVersion = parameter.SourceVersion;
            paramInfo.Precision = parameter.Precision;
            paramInfo.Size = parameter.Size;

            paramInfo.DbType = method.OraDbType.GetValue(parameter);
            paramInfo.ArrayBindSize = method.ArrayBindSize.GetValue(parameter);
            paramInfo.CollectionType = method.CollectionType.GetValue(parameter);
            paramInfo.ParameterDirection = parameter.Direction;
            paramInfo.IsNullable = parameter.IsNullable;
            paramInfo.Scale = parameter.Scale;
            paramInfo.SourceColumn = parameter.SourceColumn;
            paramInfo.Status = method.Status.GetValue(parameter);
            paramInfo.Value = parameter.Value;

            return paramInfo;
        }

        private static OracleParameterExpressions GetOracleProperties(Type type)
        {
            return new OracleParameterExpressions(type);
        }

        private static CommandExpressions Get(IDbCommand command)
        {
            return CachedOracleCommandProperties.GetOrAdd(command.GetType(), type =>
            {
                return new CommandExpressions(type);
            });
        }

        private class OracleParameterExpressions
        {
            public OracleParameterExpressions(Type oracleParameterType)
            {
                if (!oracleParameterType.Namespace.StartsWith("Oracle") && !oracleParameterType.Namespace.StartsWith("Castle"))
                {
                    throw new NotSupportedException($"Whoopsies! This library will only work with Oracle types, you are attempting to use type {oracleParameterType.FullName}.");
                }

                OraDbType = new ObjectEnumWrapper<IDbDataParameter, OracleMappingType>("OracleDbType", "OracleDbType", oracleParameterType);
                ArrayBindSize = new ObjectWrapper<IDbDataParameter, int[]>("ArrayBindSize", oracleParameterType);
                CollectionType = new ObjectEnumWrapper<IDbDataParameter, OracleMappingCollectionType>("OracleCollectionType", "CollectionType", oracleParameterType);
                Status = new ObjectEnumWrapper<IDbDataParameter, OracleParameterMappingStatus>("Status", "Status", oracleParameterType);
                IsNullable = new ObjectWrapper<IDbDataParameter, bool>("IsNullable",oracleParameterType);
            }

            public ObjectEnumWrapper<IDbDataParameter, OracleMappingType> OraDbType { get; }

            public ObjectWrapper<IDbDataParameter, int[]> ArrayBindSize { get; }

            public ObjectWrapper<IDbDataParameter, bool> IsNullable { get; }

            public ObjectEnumWrapper<IDbDataParameter, OracleMappingCollectionType> CollectionType { get; }

            public ObjectEnumWrapper<IDbDataParameter, OracleParameterMappingStatus> Status { get; }
        }

        private class CommandExpressions
        {
            public ObjectWrapper<IDbCommand, bool> BindByName { get; }
            public ObjectWrapper<IDbCommand, int> InitialLOBFetchSize { get; }
            public ObjectWrapper<IDbCommand, int> ArrayBindCount { get; }

            public CommandExpressions(Type commandType)
            {
                if (!commandType.Namespace.StartsWith("Oracle") && !commandType.Namespace.StartsWith("Castle"))
                {
                    throw new NotSupportedException($"Whoopsies! This library will only work with Oracle types, you are attempting to use type {commandType.FullName}, which is not supported.");
                }

                BindByName = new ObjectWrapper<IDbCommand, bool>("BindByName", commandType);
                InitialLOBFetchSize = new ObjectWrapper<IDbCommand, int>("InitialLOBFetchSize", commandType);
                ArrayBindCount = new ObjectWrapper<IDbCommand, int>("ArrayBindCount", commandType);
            }


        }

        private class CommandProperties
        {
            public PropertyInfo InitialLOBFetchSize { get; set; }

            public PropertyInfo ArrayBindCount { get; set; }

            public PropertyInfo BindByName { get; set; }
        }
    }
}
