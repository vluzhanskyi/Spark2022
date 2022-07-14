using System.Globalization;
using System.Collections.Generic;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlaybackModels;
using Newtonsoft.Json;

namespace NppClient
{
    public class NppClient
    {
        private readonly string _predictedCallsFile;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        private readonly IConfiguration _configuration;

        public NppClient( IConfiguration config, string predictedCallsFile, ILogger logger)
        {
            _predictedCallsFile = predictedCallsFile;
            _logger = logger;
            _httpClient = new HttpClient();
            _configuration = config;
        }

        public async Task RequestPredictedCalls()
        {
            try
            {
                var nppUrl = _configuration["NPP_URL"];
                var loginToken = _configuration["LoginToken"];

                _logger.LogDebug($"Start requesting predicted interactions for NPP: {nppUrl}");

                using var streamReader = new StreamReader(_predictedCallsFile);
                using var csvReader = new CsvHelper.CsvReader(streamReader, CultureInfo.InvariantCulture);

                var bulkRequest = new PlaybackBulkRequest()
                {
                    ApplicationId = "ConnectAPI",
                    ApplicationUserId = "1234",
                    ApplicationUserName = "John Smith",
                    ApplicationUserToken = loginToken,
                    SiteId = 1,
                    Calls = new List<PlaybackBulkItem>()
                };

                var records = csvReader.GetRecords<PlaybackStatisticsItem>();
                foreach (var record in records)
                {
                    bulkRequest.Calls.Add(new PlaybackBulkItem
                    {
                        CallId = record.InteractionId.ToString(),
                        MediaOutputType = (MediaOutputType)record.OutputType
                    });

                    if (bulkRequest.Calls.Count >= 10)
                    {
                        await SendBulkPlaybackRequest(nppUrl, bulkRequest);
                        bulkRequest.Calls.Clear();
                    }
                }

                // Send the rest of calls
                if (bulkRequest.Calls.Any())
                {
                    await SendBulkPlaybackRequest(nppUrl, bulkRequest);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The error occurred in RequestPredictedCalls");
            }
        }

        private async Task SendBulkPlaybackRequest(string url, PlaybackBulkRequest request)
        {
            var httpContent = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(url, httpContent);

            _logger.LogDebug($"{request.Calls.Count} predicted calls requested for transcoding");
        }
    }
}