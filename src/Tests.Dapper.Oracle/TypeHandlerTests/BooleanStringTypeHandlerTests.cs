using Dapper.Oracle.TypeHandler;
using FluentAssertions;
using Oracle.ManagedDataAccess.Client;
using Xunit;

namespace Tests.Dapper.Oracle.TypeHandlerTests
{
    public class BooleanStringTypeHandlerTests
    {
        [Fact]
        public void ConvertToTrue()
        {
            var parameter = new OracleParameter();
            var sut = new BooleanStringTypeHandler("YEP","NOPE");
            sut.SetValue(parameter, true);

            parameter.Value.Should().Be("YEP");
            parameter.OracleDbType.Should().Be(OracleDbType.Varchar2);
        }
        
        [Fact]
        public void ConvertToFalse()
        {
            var parameter = new OracleParameter();
            var sut = new BooleanStringTypeHandler("YEP","NOPE");
            sut.SetValue(parameter, false);

            parameter.Value.Should().Be("NOPE");
            parameter.OracleDbType.Should().Be(OracleDbType.Varchar2);
        }

        [Fact]
        public void ConvertFromFalse()
        {            
            var sut = new BooleanStringTypeHandler("YEP","NOPE");
            var result = sut.Parse("NOPE");
            result.Should().Be(false);
        }
        
        [Fact]
        public void ConvertFromTrue()
        {            
            var sut = new BooleanStringTypeHandler("YEP","NOPE");
            var result = sut.Parse("YEP");
            result.Should().Be(true);
        }
    }
}