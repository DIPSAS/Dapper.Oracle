using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Dapper.Oracle;
using Dapper.Oracle.BulkSql;
using FluentAssertions;
using Newtonsoft.Json;
using Tests.Dapper.Oracle.IntegrationTests.Util;
using Xunit;

namespace Tests.Dapper.Oracle.IntegrationTests.BulkSql
{
    [Collection("OracleDocker")]
    public class BulkOperationTests
    {
        const string InsertSql = "Insert into BULKCUSTOMERS (CUSTOMERID,COMPANYNAME,CITY,CONTACTNAME,CONTACTTITLE,ADDRESS,POSTALCODE,COUNTRY,PHONE,FAX,TIDSSTEMPEL,OPPRETTETAV,OPPRETTETTID,SISTENDRETAV,SISTENDRETTID,DIPSID) " +
                                 "values (:CUSTOMERID,:COMPANYNAME,:CITY,:CONTACTNAME,:CONTACTTITLE,:ADDRESS,:POSTALCODE,:COUNTRY,:PHONE,:FAX,:TIDSSTEMPEL,:OPPRETTETAV,:OPPRETTETTID,:SISTENDRETAV,:SISTENDRETTID,:DIPSID)";

        private DatabaseFixture Fixture { get; }

        public BulkOperationTests(DatabaseFixture fixture)
        {
            Fixture = fixture;

            var columns = new[]
            {
                new TableColumn {Name = "CUSTOMERID", DataType = OracleMappingType.Raw, Size = 16, PrimaryKey = true},
                new TableColumn {Name = "COMPANYNAME", DataType = OracleMappingType.Varchar2, Size = 40},
                new TableColumn {Name = "CITY", DataType = OracleMappingType.Varchar2, Size = 40},
                new TableColumn {Name = "CONTACTNAME", DataType = OracleMappingType.Varchar2, Size = 40},
                new TableColumn {Name = "CONTACTTITLE", DataType = OracleMappingType.Varchar2, Size = 40},
                new TableColumn {Name = "ADDRESS", DataType = OracleMappingType.Varchar2, Size = 60},
                new TableColumn {Name = "POSTALCODE", DataType = OracleMappingType.Varchar2, Size = 40, Nullable=true},
                new TableColumn {Name = "COUNTRY", DataType = OracleMappingType.Varchar2, Size = 40},
                new TableColumn {Name = "PHONE", DataType = OracleMappingType.Varchar2, Size = 40, Nullable=true},
                new TableColumn {Name = "FAX", DataType = OracleMappingType.Varchar2, Size = 40, Nullable=true},
                new NumberColumn {Name = "TIDSSTEMPEL", DataType = OracleMappingType.Int64, Nullable=true},
                new TableColumn {Name = "OPPRETTETAV", DataType = OracleMappingType.Varchar2, Size = 40, Nullable=true},
                new TableColumn {Name = "OPPRETTETTID", DataType = OracleMappingType.Date, Nullable=true},
                new TableColumn {Name = "SISTENDRETAV", DataType = OracleMappingType.Varchar2, Size=40, Nullable=true},
                new TableColumn {Name = "SISTENDRETTID", DataType = OracleMappingType.Date, Nullable=true},
                new NumberColumn {Name = "DIPSID", DataType = OracleMappingType.Int64, Nullable=true},
            };

            TableCreator.Create(Fixture.Connection, "BULKCUSTOMERS", columns);
        }

        [Fact, Trait("Category", "IntegrationTest")]
        public void BulkSql()
        {
            var customers = GetCustomersFromEmbeddedResource();
            foreach (var customer in customers)
            {
                customer.CustomerId = Guid.NewGuid();
            }
            var customerCount = customers.Count;
            int result = -1;

            result = Fixture.Connection.SqlBulk(InsertSql, customers, CreateMapping());
            result.Should().Be(customerCount);
        }

        [Fact, Trait("Category", "IntegrationTest")]
        public async Task BulkSqlAsync()
        {
            var customers = GetCustomersFromEmbeddedResource();
            foreach (var customer in customers)
            {
                customer.CustomerId = Guid.NewGuid();
            }

            var customerCount = customers.Count;

            var asyncQueryResult = await Fixture.Connection.SqlBulkAsync(InsertSql, customers, CreateMapping());
            asyncQueryResult.ExecuteResult.Should().Be(customerCount);
            asyncQueryResult.Parameters.Should().NotBeNull();
        }


        [Fact, Trait("Category", "IntegrationTest")]
        public void BulkInsert_Test()
        {
            var customers = GetCustomersFromEmbeddedResource();
            foreach (var customer in customers)
            {
                customer.CustomerId = Guid.NewGuid();
            }

            var customerCount = customers.Count;

            var dal = new CustomerDAL(connection: Fixture.Connection);
            dal.InsertCustomers(customers);
        }

        IEnumerable<BulkMapping<Customer>> CreateMapping()
        {
            yield return new BulkMapping<Customer>("CUSTOMERID", c => c.CustomerId.ToByteArray(), OracleMappingType.Raw);
            yield return new BulkMapping<Customer>("COMPANYNAME", c => c.CompanyName, OracleMappingType.Varchar2);
            yield return new BulkMapping<Customer>("CITY", c => c.City, OracleMappingType.Varchar2);
            yield return new BulkMapping<Customer>("CONTACTNAME", c => c.ContactName, OracleMappingType.Varchar2);
            yield return new BulkMapping<Customer>("CONTACTTITLE", c => c.ContactTitle, OracleMappingType.Varchar2);
            yield return new BulkMapping<Customer>("ADDRESS", c => c.Address, OracleMappingType.Varchar2);
            yield return new BulkMapping<Customer>("POSTALCODE", c => c.PostalCode, OracleMappingType.Varchar2);
            yield return new BulkMapping<Customer>("COUNTRY", c => c.Country, OracleMappingType.Varchar2);
            yield return new BulkMapping<Customer>("PHONE", c => c.Phone, OracleMappingType.Varchar2);
            yield return new BulkMapping<Customer>("FAX", c => c.Fax, OracleMappingType.Varchar2);
            yield return new BulkMapping<Customer>("TIDSSTEMPEL", c => c.TidsStempel, OracleMappingType.Int64);
            yield return new BulkMapping<Customer>("OPPRETTETAV", c => c.OpprettetAv, OracleMappingType.Varchar2);
            yield return new BulkMapping<Customer>("OPPRETTETTID", c => c.OpprettetTid, OracleMappingType.Date);
            yield return new BulkMapping<Customer>("SISTENDRETAV", c => c.SistEndretAv, OracleMappingType.Varchar2);
            yield return new BulkMapping<Customer>("SISTENDRETTID", c => c.SistEndretTid, OracleMappingType.Date);
            yield return new BulkMapping<Customer>("DIPSID", c => c.DipsId, OracleMappingType.Int64);
        }


        private List<Customer> GetCustomersFromEmbeddedResource()
        {
            using (Stream s = this.GetType().Assembly.GetManifestResourceStream("Tests.Dapper.Oracle.IntegrationTests.BulkSql.customers.json"))
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    string json = sr.ReadToEnd();
                    return JsonConvert.DeserializeObject<IEnumerable<Customer>>(json).ToList();
                }
            }
        }
    }

    public class CustomerDAL
    {
        public IDbConnection Connection { get; }

        public CustomerDAL(IDbConnection connection)
        {
            Connection = connection;
        }

        public void InsertCustomers(IEnumerable<Customer> customers)
        {
            string insertSql = @"INSERT INTO BULKCUSTOMERS(CUSTOMERID,COMPANYNAME,ADDRESS,POSTALCODE,CITY, CONTACTNAME, CONTACTTITLE, Country, tidsstempel, opprettettid) 
                VALUES(:CUSTOMERID,:COMPANYNAME,:ADDRESS, :POSTALCODE,:CITY, :ContactName, :CONTACTTITLE, :Country, :tidsstempel, :opprettettid)";

            var mapping = new BulkMapping<Customer>[]
            {
                new BulkMapping<Customer>("CUSTOMERID",c=>c.CustomerId),
                new BulkMapping<Customer>("COMPANYNAME",c=>c.ContactName),
                new BulkMapping<Customer>("ADDRESS",c=>c.Address),
                new BulkMapping<Customer>("POSTALCODE",c=>c.PostalCode),
                new BulkMapping<Customer>("CITY",c=>c.City),
                new BulkMapping<Customer>("ContactName",c=>c.ContactName),
                new BulkMapping<Customer>("ContactTitle",c=>c.ContactTitle),
                new BulkMapping<Customer>("Country", c=> c.Country),
                new BulkMapping<Customer>("Tidsstempel", c=> c.TidsStempel),
                new BulkMapping<Customer>("Opprettettid", c=> c.OpprettetTid)
            };


            Connection.SqlBulk(insertSql, customers, mapping);

        }
    }
}
