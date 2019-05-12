using System;
using Dapper.Oracle.TypeHandler;
using Dapper.Oracle.TypeMapping;
using FluentAssertions;
using Oracle.ManagedDataAccess.Client;
using Xunit;

namespace Tests.Dapper.Oracle.TypeHandlerTests
{
    public class GuidRaw16TypeHandlerTests
    {
        [Fact]
        public void ConvertTo()
        {
            Guid input = Guid.NewGuid();

            var parameter = new OracleParameter();
            var sut = new GuidRaw16TypeHandler();
            sut.SetValue(parameter,input);
            
            parameter.Value.Should().BeEquivalentTo(input.ToByteArray());
            parameter.OracleDbType.Should().Be(OracleDbType.Raw);
        }
        
        [Fact]
        public void ConvertFrom()
        {
            Guid input = Guid.NewGuid();
           
            var sut = new GuidRaw16TypeHandler();
            var result = sut.Parse(input.ToByteArray());
            result.Should().Be(input);
        }
        
        
    }
}