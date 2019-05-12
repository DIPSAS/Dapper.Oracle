## Bulk Operation


If you want to insert, update or delete many rows at a time from a database, the most efficient way is to minimize the number of round trips.
Oracle has this built in to the client api, but using it is rather cumbersome to use.  
Dapper.Oracle contains some extension methods that makes this easier.

Consider the following class, a simplified dataaccess:

```
public class CustomerDAL
    {
        public IDbConnection Connection { get; }

        public CustomerDAL(IDbConnection connection)
        {
            Connection = connection;
        }

        public void InsertCustomers(IEnumerable<Customer> customers)
        {
            string insertSql = "INSERT INTO CUSTOMERS(CUSTOMERID,NAME,ADDRESS,POSTALCODE,CITY) VALUES(:CUSTOMERID,:NAME,:POSTALCODE,:CITY)";
            foreach (var customer in customers)
            {
                var parameters = new OracleDynamicParameters();
                parameters.Add("CUSTOMERID",customer.CustomerId);
                parameters.Add("NAME",customer.Name);
                parameters.Add("ADDRESS",customer.Address);
                parameters.Add("POSTALCODE",customer.Address);
                parameters.Add("POSTALCODE", customer.PostalCode);
                parameters.Add("CITY",customer.City);
                Connection.Execute(insertSql, parameters);

            }
        }
    }
```
The method InsertCustomers takes in a `IEnumerable<Customer>`, and iterates over it, inserting customers into the database.
This is a fairly common pattern, but it has some drawbacks, the biggest one being that it performs a full database roundtrip per row to insert into the database.
A much better approach is to send over an array of parameters, and have the database execute all statements in a single roundtrip.  
This allows for > 1000 inserts/second.

The same class, rewritten using Dapper.Oracle Bulk Sql

```
public class CustomerDAL
    {
        public IDbConnection Connection { get; }

        public CustomerDAL(IDbConnection connection)
        {
            Connection = connection;
        }

        public void InsertCustomers(IEnumerable<Customer> customers)
        {
            string insertSql = "INSERT INTO CUSTOMERS(CUSTOMERID,NAME,ADDRESS,POSTALCODE,CITY) VALUES(:CUSTOMERID,:NAME,:POSTALCODE,:CITY)";

            var mapping = new BulkMapping<Customer>[]
            {
                new BulkMapping<Customer>("CUSTOMERID",c=>c.CustomerId),
                new BulkMapping<Customer>("NAME",c=>c.Name),
                new BulkMapping<Customer>("ADDRESS",c=>c.Address),
                new BulkMapping<Customer>("POSTALCODE",c=>c.PostalCode),
                new BulkMapping<Customer>("CITY",c=>c.City),
            };

            Connection.SqlBulk(insertSql, customers, mapping);
        }
    }
```