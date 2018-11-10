# Dapper.Oracle

Oracle support for Dapper Micro ORM.

[![NuGet](https://img.shields.io/nuget/v/Dapper.Oracle.svg)](https://www.nuget.org/packages/Dapper.Oracle/)

### Introduction

Dapper is a great tool if you want to write database-agnostic code.
However, sometimes you need to access functionality that is provider-specific.  This assembly adds support for writing Oracle-specific SQL, that supports all dbtypes used by the Oracle managed provider on a parameter, supports setting various properties on the command(LOBFetchSize, ArrayBindCount, BindByName), as well as setting CollectionType on the parameter.
Using this package, you can now run stored procedures that returns RefCursor, or use array bind count to execute a sql statements with a array of parameters.

### Supported Oracle-specific properties
**OracleParameter(Managed and UnManaged)**
- OracleDbType enum (all members used by the managed provider)
- CollectionType enum
- ParameterStatus (return type when executing stored procedure)
- ArrayBindSize

**OracleCommand (Managed and UnManaged)**
- ArrayBindCount property
- BindByName property
- InitialLOBFetchSize (LOB = Large Object Binary)

### Works with both Managed and Unmanaged driver

Dapper.Oracle uses reflection to set parameters on both the managed and unmanaged driver(ODP.Net), 
so it does not have any direct dependencies to a specific Oracle driver.  
However, you still need to reference either Oracle.DataAccess or Oracle.ManagedDataAccess in addition to this package.
Usage is pretty much like standard Dapper, see usage-section below.

### Usage examples
```csharp

public string RunStoredProcedure(string parametervalue1,string parametervalue2)
{
    var connection = new OracleConnection("mydatabaseconnectionstring");
    var parameters = new OracleDynamicParameters();
    parameters.Add("RETURN_VALUE", string.Empty, OracleMappingType.Varchar2, ParameterDirection.ReturnValue, 4000, true, 0, 0, string.Empty, DataRowVersion.Current);
    parameters.Add("PARAMETER1", parametervalue1, OracleMappingType.Varchar2, ParameterDirection.Input, 4000, true, 0, 0, String.Empty, DataRowVersion.Current);
    parameters.Add("PARAMETER2", parametervalue1, OracleMappingType.Xml, ParameterDirection.Input, 4000, true, 0, 0, string.Empty, DataRowVersion.Current);

    connection.Execute("Schema.Package.MyStoredProcedure", parameters, commandType: CommandType.StoredProcedure);

    return parameters.Get<string>("RETURN_VALUE");
}

public void RunStoredProcedureWithArrayAsParameters(IEnumerable<long> idvalues)
{
    var parameters = new OracleDynamicParameters();
    var idArray = idvalues.ToArray();
    parameters.ArrrayBindCount = idArray.Count;

    parameters.Add("ArrayParameter", idArray, OracleMappingType.Int64, ParameterDirection.Input);
    connection.Execute("Schema.Package.MyStoredProcedure", parameters, commandType: CommandType.StoredProcedure);
}
```

## Building
From a powershell script, run `build.ps1` from the root folder of the repo.

Example:

```powershell
build.ps1 -Task Compile
```