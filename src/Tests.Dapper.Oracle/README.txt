This testsuite contains both unit tests and integration tests.
Unit tests can be run without any Oracle database available or an installed oracle client.

To run integrations tests, you have to provide either a connection string to an existing oracle database, with credentials that enable you to create and drop tables.
Set environment variable DA_OR_CONNECTION to a valid connectionstring to run unit tests this way.

Alternatively, the unit test suite can download a Oracle docker container to your system, and execute the tests against that container.
To do so, you will need to 
a) Have a valid account for oracle container registry(free of charge, sign up at https://container-registry.oracle.com )
b) Accept eula for oracle 12.2.0.1 entreprise database at same site(https://container-registry.oracle.com/pls/apex/f?p=113:4:299602031545)
c) Add two environment variables to your computer: 
    DA_OR_UID - username for registered oracle-user at container-registry.oracle.com
    DA_OR_PWD - password for registered oracle-user at container-registry.oracle.com

    The build script needs those two set to be able to perform a docker login command to download the docker container.


