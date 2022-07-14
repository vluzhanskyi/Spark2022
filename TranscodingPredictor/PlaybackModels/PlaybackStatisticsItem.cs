using System;
using System.Text;

namespace PlaybackModels
{
    public class PlaybackStatisticsItem
    {
        public virtual int Label { get; set; }
        public virtual long InteractionId { get; set; }
        public virtual int OutputType { get; set; }
        public virtual long Duration { get; set; }
        public virtual int AgentId { get; set; }
    }
}
