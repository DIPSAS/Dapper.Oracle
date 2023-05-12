param
(
  [string] $connectionstring
)

if ($connectionstring)
{
    [System.Environment]::SetEnvironmentVariable("DA_OR_CONNECTION",$connectionstring,[System.EnvironmentVariableTarget]::Process)
}

dotnet restore .\src\Dapper.Oracle.sln
dotnet build .\src\Dapper.Oracle.sln
dotnet test .\src\Dapper.Oracle.sln
dotnet pack .\src\Dapper.Oracle.sln -p:PackageVersion=2.0.4