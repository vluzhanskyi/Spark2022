using System;
using System.Collections.Generic;
using System.Text;

namespace PlaybackModels
{
    public class PredictionResult
    {
        public virtual float Label { get; set; }
        public virtual float Score { get; set; }
    }
}
