using System;
using System.Text;

namespace PlaybackModels
{
    public class PlaybackStatisticsItem
    {
        public virtual float Label { get; set; }
        public virtual long InteractionId { get; set; }
        public virtual int OutputType { get; set; }
        public virtual float Duration { get; set; }
        public virtual int AgentId { get; set; }
    }
}
