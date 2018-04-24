using System.Collections.Generic;
using System.Data;
using Dapper.Oracle;
using FluentAssertions;
using Xunit;
using UnManaged = Oracle.DataAccess.Client;
using Managed = Oracle.ManagedDataAccess.Client;

namespace Tests.Dapper.Oracle
{
    internal class TestableOracleDynamicParameters : OracleDynamicParameters
    {
        public void AddParam(IDbCommand command)
        {
            AddParameters(command, null);
        }
    }

    public class OracleDynamicParameterTests
    {
        private readonly TestableOracleDynamicParameters testObject = new TestableOracleDynamicParameters();

        public static IEnumerable<object[]> OracleCommands
        {
            get
            {
                return new[]
                {
                    new object[] { new Managed.OracleCommand(), new OracleManagedParameterRetretreiver()  },
                    new object[] { new UnManaged.OracleCommand(), new OracleUnmanagedParameterRetretreiver()  }
                };
            }
        }

        [Theory, MemberData(nameof(OracleCommands))]
        public void SetOracleParameter(IDbCommand cmd, IOracleParameterRetretreiver retreiver)
        {
            testObject.Add("Foo", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.ReturnValue);
            testObject.AddParam(cmd);
            cmd.Parameters.Should().HaveCount(1);
            var param = retreiver.GetParameter(cmd.Parameters[0]);
            param.OracleDbType.Should().Be("RefCursor");
            param.ParameterName.Should().Be("Foo");
        }

        [Theory, MemberData(nameof(OracleCommands))]
        public void SetOracleParameterCommand(IDbCommand cmd, IOracleParameterRetretreiver retreiver)
        {
            testObject.Add("Foo", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.ReturnValue);

            testObject.AddParam(cmd);
            cmd.Parameters.Should().HaveCount(1);
            var param = retreiver.GetParameter(cmd.Parameters[0]);
            param.ParameterName.Should().Be("Foo");
            param.OracleDbType.Should().Be("RefCursor");            
        }

        [Theory, MemberData(nameof(OracleCommands))]
        public void SetOracleParameterCommandPlSqlAssociativeArray(IDbCommand cmd, IOracleParameterRetretreiver retreiver)
        {
            testObject.Add("Foo", collectionType: OracleMappingCollectionType.PLSQLAssociativeArray);

            testObject.AddParam(cmd);
            cmd.Parameters.Should().HaveCount(1);
            var param = retreiver.GetParameter(cmd.Parameters[0]);
            param.CollectionType.Should().Be("PLSQLAssociativeArray");
        }

        [Theory, MemberData(nameof(OracleCommands))]
        public void SetAllProperties(IDbCommand cmd, IOracleParameterRetretreiver retreiver)
        {
            testObject.Add("Foo", "Bar", OracleMappingType.Varchar2, ParameterDirection.Input, 42, true, 0, 0, "MySource", DataRowVersion.Original);

            testObject.AddParam(cmd);
            cmd.Parameters.Should().HaveCount(1);
            var param = retreiver.GetParameter(cmd.Parameters[0]);
            param.ParameterName.Should().Be("Foo");
            param.Value.Should().Be("Bar");
            param.OracleDbType.Should().Be("Varchar2");
            param.Direction.Should().Be(ParameterDirection.Input);
            param.Size.Should().Be(42);
            param.IsNullable.Should().Be(true);
            param.Scale.Should().Be(0);
            param.Precision.Should().Be(0);
            param.SourceColumn.Should().Be("MySource");
            param.SourceVersion.Should().Be(DataRowVersion.Original);
        }

        [Theory, MemberData(nameof(OracleCommands))]
        public void SetBindByNameFalse(IDbCommand cmd, IOracleParameterRetretreiver retreiver)
        {
            testObject.BindByName = true;
            testObject.Add("Foo","Bar");
            testObject.AddParam(cmd);

            var value = (bool) cmd.GetType().GetProperty("BindByName").GetValue(cmd);
            value.Should().BeTrue();
        }

        [Theory, MemberData(nameof(OracleCommands))]
        public void SetBindByNameNotSet(IDbCommand cmd, IOracleParameterRetretreiver retreiver)
        {
            testObject.BindByName = false;
            testObject.Add("Foo", "Bar");
            testObject.AddParam(cmd);

            var value = (bool)cmd.GetType().GetProperty("BindByName").GetValue(cmd);
            value.Should().BeFalse();
        }
    }
}
