using System;
using Dapper.Oracle;
using FluentAssertions;
using Oracle.DataAccess.Types;
using Xunit;

namespace Tests.Dapper.Oracle
{
    public class OracleValueConverterTests
    {
        [Fact]
        public void GetOracleStringWithContentReturnsContent()
        {            
            var oraString = new OracleString("Foo");
            var result = OracleValueConverter.Convert<string>(oraString);
            result.Should().Be("Foo");
        }

        [Fact]
        public void GetOracleStringAsNullReturnsNull()
        {
            var oraString = new OracleString();
            var result = OracleValueConverter.Convert<string>(oraString);
            result.Should().BeNull();
        }

        [Fact]
        public void GetStringAsDbNullReturnsNull()
        {            
            var result = OracleValueConverter.Convert<string>(DBNull.Value);
            result.Should().BeNull();
        }

        [Fact]
        public void GetNullAsStringReturnsNull()
        {            
            var result = OracleValueConverter.Convert<string>(null);
            result.Should().BeNull();
        }

        [Fact]
        public void GetNullAsDecimalReturns0()
        {
            var result = OracleValueConverter.Convert<double>(null);
            result.Should().Be(0);
        }

        [Fact]
        public void GetOracleDecimalReturnsDecimal()
        {
            var result = OracleValueConverter.Convert<decimal>(new OracleDecimal(100));
            result.Should().Be(100);
        }

        [Fact]
        public void GetOracleDateTimeReturnsDateTime()
        {
            var result = OracleValueConverter.Convert<DateTime>(new OracleDate(DateTime.Today));
            result.Should().Be(DateTime.Today);
        }


    }
}
