using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AM.HealthAPI
{
    public class SQLConnectionHealthCheck : IHealthCheck
    {
        private static readonly string DefaultSqlQuery = "SELECT 1";
        public string ConnectionString { get; set; }
        public string TestQuery { get; set; }

        public SQLConnectionHealthCheck(string connectionString) : this(connectionString, testQuery: DefaultSqlQuery)
        {

        }

        public SQLConnectionHealthCheck(string connectionString, string testQuery)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            TestQuery = testQuery;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    await conn.OpenAsync(cancellationToken);

                    if (TestQuery != null)
                    {
                        var command = conn.CreateCommand();
                        command.CommandText = TestQuery;

                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
                catch (DbException ex)
                {
                    return new HealthCheckResult(status: context.Registration.FailureStatus, exception: ex);
                }
            }

            return HealthCheckResult.Healthy();
        }
    }
}
