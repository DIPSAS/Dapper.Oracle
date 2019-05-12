using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Dapper.Oracle.IntegrationTests.BulkSql
{
    public class Customer
    {
        public Guid CustomerId { get; set; }
        public string CompanyName { get; set; }

        public string City { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public long TidsStempel { get; set; }
        public string OpprettetAv { get; set; }
        public DateTime OpprettetTid { get; set; }
        public string SistEndretAv { get; set; }
        public DateTime SistEndretTid { get; set; }

        public long DipsId { get; set; }

    }
}
