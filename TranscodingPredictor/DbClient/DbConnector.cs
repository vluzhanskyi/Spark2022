
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PlaybackModels;

namespace DbClient
{
    public class DbConnector: DbContext
    {
        private readonly SqlConnection connection;
        public DbConnector()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "DBDM-14-221.voicelab.local"; 
            builder.UserID = "sa";            
            builder.Password = "nicecti1!";     
            builder.InitialCatalog = "nice_interactions";
            connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
        }

        public List<PlaybackStatisticsItem> LoadInteractionsData(DbFilterType type)
        {
            var result = new List<PlaybackStatisticsItem>();
            try
            {
                string sql =
                    "SELECT iInitiatorUserID, biInteractionDuration, iInteractionID " +
                    $"FROM nice_interactions.dbo.vwInteraction WHERE dtInteractionGMTStopTime >= DATEADD(day,-{type}, GETDATE())";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new PlaybackStatisticsItem()
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

            }

            return result;
        }

        private string GetFilterString(DbFilterType type)
        {
            switch (type)
            {
                case DbFilterType.FromYestarday:
                    return "dtInteractionGMTStopTime >= DATEADD(day, -2, GETDATE())";
                case DbFilterType.FromToday:
                    return "dtInteractionGMTStopTime >= DATEADD(day, -1, GETDATE())";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }

    public enum DbFilterType
    {
        FromYestarday = 2,
        FromToday = 1
    }

}