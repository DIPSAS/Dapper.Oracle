using Dapper;
using Dapper.Oracle;
using Dapper.Oracle.TypeHandler;
using FluentAssertions;
using Tests.Dapper.Oracle.IntegrationTests.Util;
using Xunit;

namespace Tests.Dapper.Oracle.IntegrationTests
{
    [Collection("OracleDocker")]
    public class BooleanStringTypeMapperTests
    {
        private DatabaseFixture Fixture { get; }

        public BooleanStringTypeMapperTests(DatabaseFixture fixture)
        {
            Fixture = fixture;

            var columns = new[]
            {
                new NumberColumn {Name = "Id", DataType = OracleMappingType.Int32, PrimaryKey = true},
                new TableColumn {Name = "BooleanValue", DataType = OracleMappingType.Char, Size = 1}
            };
            OracleTypeMapper.AddTypeHandler(typeof(bool),new BooleanStringTypeHandler("Y","N"));            
            TableCreator.Create(Fixture.Connection,"BoolCharTypeMappingTest",columns);                       
        }

        private int InsertValue(int id, bool booleanValue)
        {
            string sql = "INSERT INTO BoolCharTypeMappingTest(Id,BooleanValue) VALUES(:ID,:BOOL)";

            OracleDynamicParameters parameters = new OracleDynamicParameters();
            parameters.Add("ID",id);
            parameters.Add("BOOL",booleanValue);
            return Fixture.Connection.Execute(sql, parameters);
        }

        [Fact]
        public void TestInsertMapping()
        {
            var count = InsertValue(1, true);
            count.Should().Be(1);
        }

        [Fact]
        public void TestSelectMappingFalse()
        {
            InsertValue(10, false);
            string sql = "SELECT * FROM BoolCharTypeMappingTest WHERE ID=10";
            var actual = Fixture.Connection.QuerySingle<BoolTestMapping>(sql);
            actual.BooleanValue.Should().BeFalse();
        }
        
        [Fact]
        public void TestSelectMappingTrue()
        {
            InsertValue(11, true);
            string sql = "SELECT * FROM BoolCharTypeMappingTest WHERE ID=11";
            var actual = Fixture.Connection.QuerySingle<BoolTestMapping>(sql);
            actual.BooleanValue.Should().BeTrue();
        }

        internal class BoolTestMapping
        {
            public int Id { get; set; }
            public bool BooleanValue { get; set; }
        }
        
    }
}