
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlaybackModels;

namespace DbClient
{
    public class DbDataProvider: DbContext, IDbDataProvider
    {
        private readonly SqlConnection _connection;
        private readonly ILogger _logger;

        public DbDataProvider(IConfiguration config, ILogger<DbDataProvider> logger)
        {
            _connection = new SqlConnection(config.GetConnectionString("NiceDb"));
            _connection.Open();
            _logger = logger;
        }

        public List<PlaybackStatisticsItem> LoadInteractionsData(DbFilterType type)
        {
            var result = new List<PlaybackStatisticsItem>();
            try
            {
                string sql =
                    "SELECT iInitiatorUserID, biInteractionDuration, iInteractionID " +
                    $"FROM nice_interactions.dbo.vwInteraction WHERE dtInteractionGMTStopTime >= DATEADD(day,-{type}, GETDATE())";

                using (SqlCommand command = new SqlCommand(sql, _connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new()
                            {
                                AgentId = reader.GetInt32(0),
                                Duration = reader.GetInt64(1) / 1000,
                                Label = 10 - reader.GetInt32(0),
                                OutputType = 1,
                                InteractionId = reader.GetInt64(2)
                            });
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError("Fail to get data from DB {0}", exception);
            }

            return result;
        }
    }

    public enum DbFilterType
    {
        FromYesterday = 2,
        FromToday = 1
    }

}