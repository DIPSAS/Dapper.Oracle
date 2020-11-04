using System;
using System.Text;
using Dapper.Oracle;
using FluentAssertions;
#if NETCOREAPP2_0
using Oracle.ManagedDataAccess.Types;
#else
using Oracle.DataAccess.Types;
#endif

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
            var result = OracleValueConverter.Convert<decimal>(null);
            result.Should().Be(0);
        }

        [Fact]
        public void GetDbNullAsDecimalReturns0()
        {
            var result = OracleValueConverter.Convert<decimal>(DBNull.Value);
            result.Should().Be(0);
        }

        [Fact]
        public void GetNullAsInt32Returns0()
        {
            var result = OracleValueConverter.Convert<int>(null);
            result.Should().Be(0);
        }

        [Fact]
        public void GetDbNullAsInt32Returns0()
        {
            var result = OracleValueConverter.Convert<int>(DBNull.Value);
            result.Should().Be(0);
        }

        [Fact]
        public void GetNullAsInt64Returns0()
        {
            var result = OracleValueConverter.Convert<long>(null);
            result.Should().Be(0);
        }

        [Fact]
        public void GetDbNullAsInt64Returns0()
        {
            var result = OracleValueConverter.Convert<long>(DBNull.Value);
            result.Should().Be(0);
        }

        [Fact]
        public void GetNullAsNullableDecimalReturnsNull()
        {
            var result = OracleValueConverter.Convert<decimal?>(null);
            result.Should().BeNull();
        }

        [Fact]
        public void GetNullAsNullableInt32ReturnsNull()
        {
            var result = OracleValueConverter.Convert<int?>(null);
            result.Should().BeNull();
        }

        [Fact]
        public void GetNullAsNullableInt64ReturnsNull()
        {
            var result = OracleValueConverter.Convert<long?>(null);
            result.Should().BeNull();
        }

        [Fact]
        public void GetDbNullAsDecimalReturnsNull()
        {
            var result = OracleValueConverter.Convert<decimal?>(DBNull.Value);
            result.Should().Be(null);
        }

        [Fact]
        public void GetDbNullAsInt32ReturnsNull()
        {
            var result = OracleValueConverter.Convert<int?>(DBNull.Value);
            result.Should().Be(null);
        }

        [Fact]
        public void GetDbNullAsInt64ReturnsNull()
        {
            var result = OracleValueConverter.Convert<long?>(DBNull.Value);
            result.Should().Be(null);
        }


        [Fact]
        public void GetOracleDecimalReturnsDecimal()
        {
            decimal expected = 100;

            var result = OracleValueConverter.Convert<decimal>(new OracleDecimal(expected));

            result.Should().Be(expected);
        }

        [Fact]
        public void GetOracleDecimalReturnsNullableDecimal()
        {
            decimal expected = 100;

            var result = OracleValueConverter.Convert<decimal?>(new OracleDecimal(expected));

            result.Should().Be(expected);
        }

        [Fact]
        public void GetOracleDecimalReturnsInt32()
        {
            decimal input = 100;
            var expected = Convert.ToInt32(input);

            var result = OracleValueConverter.Convert<int>(new OracleDecimal(input));

            result.Should().Be(expected);
        }

        [Fact]
        public void GetOracleDecimalReturnsNullableInt32()
        {
            decimal input = 100;
            var expected = Convert.ToInt32(input);

            var result = OracleValueConverter.Convert<int?>(new OracleDecimal(input));

            result.Should().Be(expected);
        }

        [Fact]
        public void GetOracleDecimalReturnsInt64()
        {
            decimal input = 100;
            var expected = Convert.ToInt64(input);

            var result = OracleValueConverter.Convert<long>(new OracleDecimal(input));

            result.Should().Be(expected);
        }

        [Fact]
        public void GetOracleDecimalReturnsNullableInt64()
        {
            decimal input = 100;
            var expected = Convert.ToInt64(input);

            var result = OracleValueConverter.Convert<long?>(new OracleDecimal(input));

            result.Should().Be(expected);
        }

        [Fact]
        public void GetOracleDateTimeReturnsDateTime()
        {
            var result = OracleValueConverter.Convert<DateTime>(new OracleDate(DateTime.Today));
            result.Should().Be(DateTime.Today);
        }
    }  
}
