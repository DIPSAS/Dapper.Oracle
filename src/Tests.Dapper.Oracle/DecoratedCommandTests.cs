using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Dapper;
using Dapper.Oracle;
using FluentAssertions;
using Oracle.ManagedDataAccess.Client;
using Xunit;

namespace Tests.Dapper.Oracle
{
    public class DecoratedCommandTests
    {


        private static OracleCommand CreateCommand => new OracleCommand();

        public static IEnumerable<object[]> Commands()
        {
            yield return new object[] { CreateCommand, new OracleManagedParameterRetretreiver() };
            yield return new object[] { new DecoratedDbCommand(CreateCommand), new OracleManagedParameterRetretreiver() };
            yield return new object[] { new DecoratedDbCommand(new DecoratedDbCommand(CreateCommand)), new OracleManagedParameterRetretreiver() };
        }

        [Theory, MemberData(nameof(Commands))]
        public void Works_On_Decorated_Commands(IDbCommand command, IOracleParameterRetretreiver retreiver)
        {
            var parameters = new TestableOracleDynamicParameters();
            parameters.Add("Foo", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.ReturnValue);
            parameters.AddParam(command);
            var oracleParam = retreiver.GetParameter(command.Parameters[0]);
            oracleParam.OracleDbType.Should().Be("RefCursor");
            oracleParam.Direction.Should().Be(ParameterDirection.ReturnValue);
        }

        [Theory, MemberData(nameof(Commands))]
        public void Set_BindByName_On_Decorated_Commands(IDbCommand command, IOracleParameterRetretreiver retreiver)
        {
            var parameters = new TestableOracleDynamicParameters();
            parameters.BindByName = true;
            parameters.Add("Foo", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.ReturnValue);
            parameters.AddParam(command);
            var oracleParam = retreiver.GetParameter(command.Parameters[0]);
            oracleParam.OracleDbType.Should().Be("RefCursor");
            oracleParam.Direction.Should().Be(ParameterDirection.ReturnValue);
        }
    }
}
