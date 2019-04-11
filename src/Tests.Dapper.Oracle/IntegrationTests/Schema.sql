BEGIN
EXECUTE IMMEDIATE 'CREATE TABLE GuidCustomerTest( 
    CustomerId raw(16) NOT NULL,
    CustomerName varchar2(50) NOT NULL,
    City varchar2(50),
    CONSTRAINT customers_pk PRIMARY KEY (CustomerId))';        
END;