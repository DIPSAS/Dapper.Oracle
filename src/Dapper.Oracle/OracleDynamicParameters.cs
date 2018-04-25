//// Based on Gist found here: https://gist.github.com/vijaysg/3096151

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Dapper.Oracle
{
    /// <summary>
    /// Parameter support for Oracle-specific types and functions.  For use with Dapper.
    /// Implements <see cref="SqlMapper.IDynamicParameters"/>.
    /// </summary>
    public class OracleDynamicParameters : SqlMapper.IDynamicParameters
    {
        private static Dictionary<SqlMapper.Identity, Action<IDbCommand, object>> ParamReaderCache { get; } = new Dictionary<SqlMapper.Identity, Action<IDbCommand, object>>();

        private Dictionary<string, ParamInfo> Parameters { get; } = new Dictionary<string, ParamInfo>();

        private List<object> templates;

        /// <summary>
        /// construct a dynamic parameter bag
        /// </summary>
        public OracleDynamicParameters()
        {
        }

        /// <summary>
        /// construct a dynamic parameter bag
        /// </summary>
        /// <param name="template">can be an anonymous type or a DynamicParameters bag</param>
        public OracleDynamicParameters(object template)
        {
            AddDynamicParams(template);
        }

        /// <summary>
        /// Gets or sets the value for Oracle ArrayBindCount.  Refer to Oracle documentation for how to use it.
        /// </summary>
        public int ArrayBindCount { get; set; }

        /// <summary>
        /// Gets or sets the value for InitialLOBFetchSize.  Refer to Oracle documentation for how to use it.
        /// </summary>
        public int InitialLOBFetchSize { get; set; }

        public bool BindByName { get; set; }

        /// <summary>
        /// Append a whole object full of params to the dynamic
        /// EG: AddDynamicParams(new {A = 1, B = 2}) // will add property A and B to the dynamic
        /// </summary>
        /// <param name="param"></param>
        public void AddDynamicParams(dynamic param)
        {
            if (param is object obj)
            {
                if (!(obj is OracleDynamicParameters subDynamic))
                {
                    if (!(obj is IEnumerable<KeyValuePair<string, object>> dictionary))
                    {
                        templates = templates ?? new List<object>();
                        templates.Add(obj);
                    }
                    else
                    {
                        foreach (var kvp in dictionary)
                        {
                            Add(kvp.Key, kvp.Value);
                        }
                    }
                }
                else
                {
                    if (subDynamic.Parameters != null)
                    {
                        foreach (var kvp in subDynamic.Parameters)
                        {
                            Parameters.Add(kvp.Key, kvp.Value);
                        }
                    }

                    if (subDynamic.templates != null)
                    {
                        templates = templates ?? new List<object>();
                        foreach (var t in subDynamic.templates)
                        {
                            templates.Add(t);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add a parameter to this dynamic parameter list
        /// </summary>
        /// <param name="name">Parameter name.  If it starts with @,: or ? it is stripped before adding it to the parameter list.</param>
        /// <param name="value">Value of parameter.</param>
        /// <param name="dbType">Parameter type</param>
        /// <param name="direction">Parameter direction</param>
        /// <param name="size"></param>
        /// <param name="isNullable"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="sourceColumn"></param>
        /// <param name="sourceVersion"></param>
        /// <param name="collectionType"></param>
        public void Add(
            string name,
            object value = null,
            OracleMappingType? dbType = null,
            ParameterDirection? direction = null,
            int? size = null,
            bool? isNullable = null,
            byte? precision = null,
            byte? scale = null,
            string sourceColumn = null,
            DataRowVersion? sourceVersion = null,
            OracleMappingCollectionType? collectionType = null,
            int[] arrayBindSize= null)
        {
            Parameters[Clean(name)] = new ParamInfo()
            {
                Name = name,
                Value = value,
                ParameterDirection = direction ?? ParameterDirection.Input,
                DbType = dbType,
                Size = size,
                IsNullable = isNullable ?? false,
                Precision = precision,
                Scale = scale,
                SourceColumn = sourceColumn,
                SourceVersion = sourceVersion ?? DataRowVersion.Current,
                CollectionType = collectionType ?? OracleMappingCollectionType.None,
                ArrayBindSize = arrayBindSize
            };
        }

        /// <summary>
        /// All the names of the param in the bag, use Get to yank them out
        /// </summary>
        public IEnumerable<string> ParameterNames
        {
            get
            {
                return Parameters.Select(p => p.Key);
            }
        }

        /// <summary>
        /// Get the value of a parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns>The value, note DBNull.Value is not returned, instead the value is returned as null</returns>
        public T Get<T>(string name)
        {
            var val = Parameters[Clean(name)].AttachedParam.Value;
            return OracleValueConverter.Convert<T>(val);
        }

        void SqlMapper.IDynamicParameters.AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            AddParameters(command, identity);
        }

        /// <summary>
        /// Add all the parameters needed to the command just before it executes
        /// </summary>
        /// <param name="command">The raw command prior to execution</param>
        /// <param name="identity">Information about the query</param>
        protected virtual void AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            if (ArrayBindCount > 0)
            {
                OracleMethodHelper.SetArrayBindCount(command, ArrayBindCount);
            }

            if (InitialLOBFetchSize > 0)
            {
                OracleMethodHelper.SetInitialLOBFetchSize(command, InitialLOBFetchSize);
            }

            if (BindByName)
            {
                OracleMethodHelper.SetBindByName(command, BindByName);
            }

            if (templates != null)
            {
                foreach (var template in templates)
                {
                    var newIdent = identity.ForDynamicParameters(template.GetType());
                    Action<IDbCommand, object> appender;

                    lock (ParamReaderCache)
                    {
                        if (!ParamReaderCache.TryGetValue(newIdent, out appender))
                        {
                            appender = SqlMapper.CreateParamInfoGenerator(newIdent, false, false);
                            ParamReaderCache[newIdent] = appender;
                        }
                    }

                    appender(command, template);
                }
            }

            foreach (var param in Parameters.Values)
            {
                var name = Clean(param.Name);
                var add = !command.Parameters.Contains(name);
                IDbDataParameter p;
                if (add)
                {
                    p = command.CreateParameter();
                    p.ParameterName = name;
                }
                else
                {
                    p = (IDbDataParameter)command.Parameters[name];
                }

                OracleMethodHelper.SetOracleParameters(p, param);

                var val = param.Value;
                p.Value = val ?? DBNull.Value;
                p.Direction = param.ParameterDirection;
                var s = val as string;
                if (s?.Length <= 4000)
                {
                    p.Size = 4000;
                }

                if (param.Size != null)
                {
                    p.Size = param.Size.Value;
                }

                if (add)
                {
                    command.Parameters.Add(p);
                    param.AttachedParam = p;
                }
            }
        }

        private static string Clean(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                switch (name[0])
                {
                    case '@':
                    case ':':
                    case '?':
                        return name.Substring(1);
                }
            }

            return name;
        }

        internal class ParamInfo
        {
            public string Name { get; set; }

            public object Value { get; set; }

            public ParameterDirection ParameterDirection { get; set; }

            public OracleMappingType? DbType { get; set; }

            public int? Size { get; set; }

            public bool? IsNullable { get; set; }

            public byte? Precision { get; set; }

            public byte? Scale { get; set; }

            public string SourceColumn { get; set; } = string.Empty;

            public DataRowVersion SourceVersion { get; set; }

            public OracleMappingCollectionType CollectionType { get; set; } = OracleMappingCollectionType.None;

            public int[] ArrayBindSize { get; set; } = null;

            public IDbDataParameter AttachedParam { get; set; }
        }
    }
}
