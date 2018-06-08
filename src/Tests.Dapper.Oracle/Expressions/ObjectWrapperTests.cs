using System.Collections.Generic;
using System.Data;
using Dapper.Oracle;
using Dapper.Oracle.Expressions;
using FluentAssertions;
using Xunit;

using Managed = Oracle.ManagedDataAccess.Client;
#if NET452
using UnManaged = Oracle.DataAccess.Client;
#endif

namespace Tests.Dapper.Oracle.Expressions
{
    public class ParameterBaseTests
    {
        public static IEnumerable<object[]> OracleDataFixture
        {
            get
            {
#if NETCOREAPP2_0
                yield return new object[] {new Managed.OracleCommand()};
#else
                yield return new object[] { new Managed.OracleCommand() };
                yield return new object[] { new UnManaged.OracleCommand() };
#endif                                                
            }
        }
    }

    public class ObjectEnumWrapperTests : ParameterBaseTests
    {
        [Theory, MemberData(nameof(OracleDataFixture))]
        public void BasicTest(IDbCommand cmd)
        {
            var param = cmd.CreateParameter();

            var setter = new ObjectEnumWrapper<IDbDataParameter, OracleMappingType>("OracleDbType", "OracleDbType", param.GetType());
            setter.SetValue(param, OracleMappingType.Date);
            var value = setter.GetValue(param);
            value.ToString().Should().Be("Date");
        }        
    }

    public class ObjectWrapperTests : ParameterBaseTests
    {
        [Theory, MemberData(nameof(OracleDataFixture))]
        public void Test(IDbCommand cmd)
        {
            var param = cmd.CreateParameter();
            var wrapper = new ObjectWrapper<IDbDataParameter, int>("Size", param.GetType());
            wrapper.SetValue(param,100);
            var result = wrapper.GetValue(param);
            result.Should().Be(100);
        }
    }
}
