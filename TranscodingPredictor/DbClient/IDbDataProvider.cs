using PlaybackModels;

namespace DbClient
{
    public interface IDbDataProvider
    {
        List<PlaybackStatisticsItem> LoadInteractionsData(DbFilterType type);
    }
}
