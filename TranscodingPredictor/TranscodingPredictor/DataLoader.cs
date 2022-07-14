using DbClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlaybackModels;

namespace TranscodingPredictor
{
    internal class DataLoader
    {
        private List<PlaybackStatisticsItem> prevData;
        private List<PlaybackStatisticsItem> newData;
        private readonly DbConnector connector;
        private string folderPath;
        private string playbackStatisticFileName;
        public DataLoader()
        {
            connector = new DbConnector();
            prevData = new List<PlaybackStatisticsItem>();
            newData = new List<PlaybackStatisticsItem>();
            folderPath = Path.Combine(Environment.CurrentDirectory, "Data");
            playbackStatisticFileName = "PlaybackStatistics.csv";
        }

        public string GetLastPlaybackStatistics()
        {
            string filePath = Path.Combine(folderPath, playbackStatisticFileName);
            File.Copy($@"D:\Program Files\Nice Systems\NICEPlayerPro\{playbackStatisticFileName}", filePath);
            return filePath;
        }

        public IEnumerable<InteractionsData> GetNewInteractions()
        {
            return newData.Select(dataItem => new InteractionsData()
                {
                    Duration = dataItem.Duration,
                    AgentId = dataItem.AgentId,
                    InteractionId = dataItem.InteractionId,
                    Label = dataItem.Label,
                    OutputType = dataItem.OutputType
                });
        }

        public void LoadSqlData()
        {
            prevData = connector.LoadInteractionsData(DbFilterType.FromYestarday);
            newData = connector.LoadInteractionsData(DbFilterType.FromToday);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            WriteCsv();
        }

        public void WriteCsv()
        {
            
            WriteData(prevData, "interactions-train.csv");
          //  WriteData(newData, "recommend-interactions.csv");
        }

        public string WriteResults(List<PlaybackStatisticsItem> dataList)
        {
            var file = Path.Combine(folderPath, "Prediction_results.txt");
            WriteResultsData(dataList, file);
            return file;
        }

        private void WriteData(List<PlaybackStatisticsItem> dataList, string fileName)
        {
            var csvHeader = "AgentId,Duration,Label,OutputType,InteractionId";
           
            string filePath = Path.Combine(folderPath, fileName);
            if (File.Exists(filePath)) File.Delete(filePath);
            using var stream = File.CreateText(filePath);

            stream.WriteLine(csvHeader);
            foreach (var data in dataList)
            {
                stream.WriteLine($"{data.AgentId},{data.Duration},{data.Label},{data.OutputType},{data.InteractionId}");
            }
        }

        private void WriteResultsData(List<PlaybackStatisticsItem> dataList, string filePath)
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            using var stream = File.CreateText(filePath);
           
            foreach (var data in dataList)
            {
                stream.WriteLine($"{data.OutputType},{data.InteractionId}");
            }
        }


    }
}
