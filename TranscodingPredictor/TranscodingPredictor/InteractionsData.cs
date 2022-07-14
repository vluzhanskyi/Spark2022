using System.Net.Http.Headers;
using Microsoft.ML.Data;
using PlaybackModels;

internal class InteractionsData : PlaybackStatisticsItem
{
    [LoadColumn(0)] 
    public override int AgentId { get; set; }

    [LoadColumn(1)] 
    public override float Duration { get; set; }

    [LoadColumn(2)] 
    public override float Label { get; set; }

    [LoadColumn(3)] 
    public override int OutputType { get; set; }

    [LoadColumn(4)]
     public override long InteractionId { get; set; }
}