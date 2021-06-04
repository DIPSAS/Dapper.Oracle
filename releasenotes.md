# Releasenotes

## 2.0.3
- Enabling using Dapper.Oracle when running under a profiled DbCommand.
Basically, any database profiler will decorate an oracle connection, so Dapper.Oracle now looks for a public property of type DbCommand when it detects that it is not working against a OracleCommand, and will recursively search for the actual DbCommand in order to set the Oracle-specific properties.

## 2.0.2 
- Bugfix of typeconversion for arrays

## 2.0.1
- Minor bugfix of issue #46 , which was a regression of how `OracleValueConverter` behaves. Many thanks to @opejanovic for fixing this.

## 2.0.0
- Dependency switched to Dapper 2.0.35
- Value conversion improvements (Thanks to @Havagan in PR #44 )
- Parameters collection is now available before execution (Thanks to @opejanovic in PR #37)
- Minimum full framework version bumped to .net 4.6.2, given that .net 4.5 is EOL.

## 1.2.1
- Fixed bug in type converter
- Added doc for Type handlers
- New feature: Bulk Sql, se doc/BulkSql.md for details.

## 1.2.0
- New buildsystem, now using Cakebuild instead of psake.
- Cleanup of file tree.
- New project: Dapper.Oracle.StrongName.  Package with same contents as Dapper.Oracle, except it is strongnamed and depends upon Dapper.StrongName

## 1.0.3
- Fix Oracle 11g (to not throw the exception "Invalid operation on null data" if a function returns NULL)

## 1.0.2
- Linq compiled expression setter not working properly when attempting to set instance property on interface.  Fixed.

## 1.0.1
- Fix to .netstandard2 compliancy

## 1.0.0
- Speedup: Using compiled Expressions instead of reflection for setting Oracle-specific properties
- Package icon and author changed to DIPS AS
- Using built-in packagereferences instead of paket(less hassle for such a small project)

## 0.9.9
- No changes except build system, moving to dotnet pack broke nuget package metadata
## 0.9.8
- Multitarget build for both .net452 and .netstandard2.0
## 0.9.7
- Added Status to OracleParameter, this is returned by some stored procs.
## 0.9.6
- Added ArrayBindSize property to OracleParameter, can now set this property on OracleCommand for both managed and unmanaged driver.
## 0.9.5
- Made OracleDynamicParameters.AddParameters virtual, so that it can be extended in a derived class.
## 0.9.0
- Initial commit to Github.