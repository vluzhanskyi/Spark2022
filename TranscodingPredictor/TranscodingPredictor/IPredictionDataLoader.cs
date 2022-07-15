using PlaybackModels;

namespace TranscodingPredictor
{
    public interface IPredictionDataLoader
    {
        void LoadSqlData();
        string GetLastPlaybackStatistics();
        IEnumerable<InteractionsData> GetNewInteractions();
        string WriteResults(List<PlaybackStatisticsItem> results);
    }
}
