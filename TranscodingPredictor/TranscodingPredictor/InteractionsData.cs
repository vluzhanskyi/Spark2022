using System.Net.Http.Headers;
using Microsoft.ML.Data;

internal class InteractionsData
{
    
    [LoadColumn(0)]
    public float AgentId;
    [LoadColumn(1)]
    public float Call_duration;
    [LoadColumn(2)]
    public float Label;
    [LoadColumn(3)]
    public float Media_OutputType;
     [LoadColumn(4)]
     
    public long InteractionId;
}