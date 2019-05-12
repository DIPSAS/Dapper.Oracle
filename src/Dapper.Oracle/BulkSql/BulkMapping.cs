using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Dapper.Oracle.BulkSql
{
    /// <summary>
    /// Contains mapping between a property on T and a database query parameter
    /// </summary>
    /// <typeparam name="T">Entity type for mapping</typeparam>
    public class BulkMapping<T>
    {
        public string Name { get; set; }

        public Func<T, object> Property { get; set; }

        public ParameterDirection ParameterDirection { get; set; }

        public OracleMappingType? DbType { get; set; }

        public int? Size { get; set; }

        public bool? IsNullable { get; set; }

        public byte? Precision { get; set; }

        public byte? Scale { get; set; }

        public string SourceColumn { get; set; } = string.Empty;

        public DataRowVersion SourceVersion { get; set; }

        public OracleMappingCollectionType CollectionType { get; set; } = OracleMappingCollectionType.None;

        public int[] ArrayBindSize { get; set; }

        /// <summary>
        /// Creates an instance of parametermapping to be used in bulk operations
        /// </summary>
        /// <param name="name">Name.  Must match the named parameter in the sql statement or stored procedure</param>
        /// <param name="property">Selectorfunc for querying an IEnumerable of T for a specific property</param>
        /// <param name="dbType">Oracle database type</param>
        /// <param name="direction">Parameter direction.  Defaults to Input</param>
        /// <param name="size"></param>
        /// <param name="isNullable"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="sourceColumn"></param>
        /// <param name="sourceVersion"></param>
        /// <param name="collectionType"></param>
        /// <param name="arrayBindSize"></param>
        public BulkMapping(string name,
            Func<T, object> property,
            OracleMappingType? dbType = null,
            ParameterDirection? direction = null,
            int? size = null,
            bool? isNullable = null,
            byte? precision = null,
            byte? scale = null,
            string sourceColumn = null,
            DataRowVersion? sourceVersion = null,
            OracleMappingCollectionType? collectionType = null,
            int[] arrayBindSize = null)
        {
            Name = name;
            Property = property;
            ParameterDirection = direction ?? ParameterDirection.Input;
            DbType = dbType;
            Size = size;
            IsNullable = isNullable ?? false;
            Precision = precision;
            Scale = scale;
            SourceColumn = sourceColumn;
            SourceVersion = sourceVersion ?? DataRowVersion.Current;
            CollectionType = collectionType ?? OracleMappingCollectionType.None;
            ArrayBindSize = arrayBindSize;
        }
    }
}
