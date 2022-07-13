using System.Net.Http.Headers;
using Microsoft.ML.Data;

internal class InteractionsData
{
    [LoadColumn(0)]
    public float Playback_Initiator;
    [LoadColumn(1)]
    public float AgentId;
    [LoadColumn(2)]
    public float InteractionId;
    [LoadColumn(3)]
    public float Call_duration;
    [LoadColumn(4)]
    public float Media_OutputType;
}