using System;
using System.Text;

namespace PlaybackModels
{
    public class PlaybackStatisticsItem
    {
        public float PlaybackInitiatorId { get; set; }
        public float InteractionId { get; set; }
        public float OutputType { get; set; }
        public float Duration { get; set; }
        public float AgentId { get; set; }
    }
}
