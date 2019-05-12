## Type Handlers

### Introduction

There is no implicit conversion between some .net datatypes and Oracle native types.
Unlike SQL Server, Oracle does not have a native type capable of storing a Guid.  The best match you can have is using a 16-bit bytearray in Oracle (RAW(16)).
Oracle does not have something that can represent a boolean value either, here the norm is to either use a number(1) and setting it to 0 or 1 to represent boolean values,
or use a CHAR(1) and Y / N to represent Yes or No values.  

To have dapper convert these types per default, you will have to tell Dapper.Oracle about them.
Dapper.Oracle contains a static registry of typemappers.  
For example, to add a Guid handler, add this to your projects initialization code:
```
using Dapper.Oracle.TypeHandler;

// your other initialization code
OracleTypeMapper.AddTypeHandler<Guid>(new GuidRaw16TypeHandler());

```

### Built-in type handlers

There are currently three typemappers that ships together with the assembly:
```
Dapper.Oracle.TypeHandler.GuidRaw16TypeHandler
Dapper.Oracle.TypeHandler.BooleanNumericTypeHandler
Dapper.Oracle.TypeHandler.BooleanStringTypeHandler
```
You will need to register the types in order to have Dapper.Oracle use them, as this changes behavior for some queries from previous versions, and we don't want to break compatability.

### Create your own type handler
To create your own type handler for custom types, you can create a derived class from `Dapper.Oracle.TypeHandler.TypeHandlerBase<T>`, and implement the neccessary methods.

