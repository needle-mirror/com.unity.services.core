using System;
using System.Collections.Generic;

namespace Unity.Services.Core.Telemetry.Internal
{
    [Serializable]
    struct Metric
    {
        public string Name;

        public double Value;

        public MetricType Type;

        public Dictionary<string, string> Tags;
    }
}
