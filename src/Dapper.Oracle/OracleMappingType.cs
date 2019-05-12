namespace Dapper.Oracle
{  
    /// <summary>
    /// Enum for mapping to Oracle native DB types.  Only for use together with Dapper and <see cref="OracleDynamicParameters"/>
    /// Enum has all datatypes from Oracle.ManagedDataAccess, Ref, Object and Array used by ODP.Net are not on the list.
    /// </summary>
    public enum OracleMappingType
    {
        BFile = 101, // 0x00000065
        Blob = 102, // 0x00000066
        Byte = 103, // 0x00000067
        Char = 104, // 0x00000068
        Clob = 105, // 0x00000069
        Date = 106, // 0x0000006A
        Decimal = 107, // 0x0000006B
        Double = 108, // 0x0000006C
        Long = 109, // 0x0000006D
        LongRaw = 110, // 0x0000006E
        Int16 = 111, // 0x0000006F
        Int32 = 112, // 0x00000070
        Int64 = 113, // 0x00000071
        IntervalDS = 114, // 0x00000072
        IntervalYM = 115, // 0x00000073
        NClob = 116, // 0x00000074
        NChar = 117, // 0x00000075
        NVarchar2 = 119, // 0x00000077
        Raw = 120, // 0x00000078
        RefCursor = 121, // 0x00000079
        Single = 122, // 0x0000007A
        TimeStamp = 123, // 0x0000007B
        TimeStampLTZ = 124, // 0x0000007C
        TimeStampTZ = 125, // 0x0000007D
        Varchar2 = 126, // 0x0000007E
        XmlType = 127, // 0x0000007F
        BinaryDouble = 132, // 0x00000084
        BinaryFloat = 133, // 0x00000085
    }

    /// <summary>
    /// BulkMapping enum to map Collection type for parameter when using PL/Sql associative arrays without referencing oracle directly.
    /// </summary>
    public enum OracleMappingCollectionType
    {
        None,
        PLSQLAssociativeArray,
    }

    /// <summary>
    /// BulkMapping enum to map Parameter status for OracleParameter
    /// </summary>
    public enum OracleParameterMappingStatus
    {
        Success,
        NullFetched,
        NullInsert,
        Truncation,
    }
}
