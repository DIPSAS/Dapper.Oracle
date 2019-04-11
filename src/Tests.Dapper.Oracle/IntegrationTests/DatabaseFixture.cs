using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tests.Dapper.Oracle.IntegrationTests
{
    [CollectionDefinition("OracleDocker")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture> {}
    
    public class DatabaseFixture : IDisposable,IAsyncLifetime
    {               
        public IDbConnection Connection { get; private set; }
        
        private IDbTransaction Transaction { get; set; }

        public DatabaseFixture()
        {
            
        }

        
        public void Dispose()
        {           
        }

        public async Task InitializeAsync()
        {                        
            var si = new ProcessStartInfo("powershell",@".\LocalOracleDockerDb.ps1");
            si.LoadUserProfile = true;
            si.RedirectStandardOutput = true;
            si.WorkingDirectory = GetBootstrapFolder();
            var proccess = Process.Start(si);
            proccess.OutputDataReceived += delegate(object sender, DataReceivedEventArgs args)
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    
                    Console.WriteLine(args.Data);  //TODO: Figure out how to get data from powershell proccess into test logs...
                }
            };
            while (!proccess.HasExited)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            if (proccess.ExitCode != 0)
            {
                throw new Exception("Error during bootstrap.  Read readme.txt and check container state manually");
            }
            
            Connection = new OracleConnection("Data Source=localhost/ORCLPDB1.localdomain;User Id=SYS;Password=Oradoc_db1;DBA Privilege=SYSDBA");
            Connection.Open();
            Transaction = Connection.BeginTransaction();


        }

        public Task DisposeAsync()
        {
            Transaction?.Rollback();
            Connection?.Close();
            Connection?.Dispose();

            return Task.CompletedTask;
        }        

        private static string GetBootstrapFolder()
        {
            var folder = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            while (folder.Parent != null &&
                   !File.Exists(Path.Combine(folder.FullName, "bootstrap", "LocalOracleDockerDb.ps1")))
            {
                folder = folder.Parent;
            }

            if (!File.Exists(Path.Combine(folder.FullName, "bootstrap", "LocalOracleDockerDb.ps1")))
            {
                throw new ApplicationException("Unable to find bootstrap folder, please verify repository contents");
            }

            return Path.Combine(folder.FullName, "bootstrap");

        }
    }       
}