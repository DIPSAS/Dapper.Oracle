using System;
using System.Text;
using Dapper.Oracle;
using FluentAssertions;
using Oracle.ManagedDataAccess.Types;

using Xunit;

namespace Tests.Dapper.Oracle
{
    public class OracleValueConverterTests
    {
        [Fact]
        public void GetOracleBoolean()
        {
            var oraBool = new OracleBoolean(true);
            var result = OracleValueConverter.Convert<bool>(oraBool);
            result.Should().Be(true);

            oraBool = new OracleBoolean(-1);
            result = OracleValueConverter.Convert<bool>(oraBool);
            result.Should().Be(true);

            oraBool = new OracleBoolean(1);
            result = OracleValueConverter.Convert<bool>(oraBool);
            result.Should().Be(true);

            oraBool = new OracleBoolean(0);
            result = OracleValueConverter.Convert<bool>(oraBool);
            result.Should().Be(false);

            var oraBoolArr = new OracleBoolean[2] { true, false };
            var resultArr = OracleValueConverter.Convert<bool[]>(oraBoolArr);
            resultArr.Should().BeOfType<bool[]>();
            resultArr.Should().HaveCount(2);
        }

        [Fact]
        public void GetOracleBinary()
        {
            var bytes = Encoding.UTF8.GetBytes("Lorem ipsum");
            var oracleBytes = new OracleBinary(bytes);
            var result = OracleValueConverter.Convert<byte[]>(oracleBytes);
            result.Should().Equal(bytes);
        }

        [Fact]
        public void GetOracleTimeStamp()
        {
            var dateNow = DateTime.Now;
            var oraTimeStamp = new OracleTimeStamp(dateNow);
            var result = OracleValueConverter.Convert<DateTime>(oraTimeStamp);
            result.Should().Be(dateNow);

            var oraTimeStampArr = new OracleTimeStamp[1] { dateNow };
            var secondResult = OracleValueConverter.Convert<DateTime[]>(oraTimeStampArr);
            secondResult.Should().BeOfType<DateTime[]>();
            secondResult.Should().HaveCount(1);
        }

        [Fact]
        public void GetOracleDateArray()
        {
            var oraDateArray = new OracleDate[] { 
                (OracleDate)DateTime.Now,
                (OracleDate)DateTime.Now.AddYears(1),
                new OracleDate("10/29/2020 16:14:23")
            };

            var nullableOraDateArray = new OracleDate?[] {
                (OracleDate)DateTime.Now,
                (OracleDate)DateTime.Now.AddYears(1),
                null,
                new OracleDate("10/29/2020 16:14:23")
            };
            var result = OracleValueConverter.Convert<DateTime[]>(oraDateArray);
            result.Should().BeOfType<DateTime[]>();
            result.Should().HaveCount(oraDateArray.Length);

            var secondResult = OracleValueConverter.Convert<DateTime?[]>(nullableOraDateArray);
            secondResult.Should().BeOfType<DateTime?[]>();
            secondResult.Should().HaveCount(nullableOraDateArray.Length);
        }

        [Fact]
        public void GetStringArray()
        {
            var oraArray = new[] { "Foo", "Bar" };
            var result = OracleValueConverter.Convert<string[]>(oraArray);
            result.Should().BeOfType<string[]>();
            result.Should().HaveCount(2);

            oraArray = new[] { "Foo", "Bar", null };
            result = OracleValueConverter.Convert<string[]>(oraArray);
            result.Should().BeOfType<string[]>();
            result.Should().HaveCount(3);
        }

        [Fact]
        public void GetOracleStringArray()
        {
            var oraArray = new OracleString[] { "Foo", "Bar" };
            var result = OracleValueConverter.Convert<OracleString[]>(oraArray);
            result.Should().BeOfType<OracleString[]>();
            result.Should().HaveCount(2);
        }

        [Fact]
        public void GetOracleString2StringArray()
        {
            var oraArray = new OracleString[] { "Foo", "Bar" };
            var result = OracleValueConverter.Convert<string[]>(oraArray);
            result.Should().BeOfType<string[]>();
            result.Should().HaveCount(2);
        }

        [Fact]
        public void GetIntArray()
        {
            var oraArray = new OracleDecimal[] { 1, 2 };
            var result = OracleValueConverter.Convert<int[]>(oraArray);
            result.Should().BeOfType<int[]>();
            result.Should().HaveCount(2);
        }


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
