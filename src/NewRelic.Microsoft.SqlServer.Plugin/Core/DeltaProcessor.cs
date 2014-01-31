using NewRelic.Platform.Sdk.Processors;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
    /// <summary>
    /// A processor for processing delta of metrics from the last poll cycle
    /// </summary>
    public class DeltaProcessor : IProcessor
    {
        private float? _lastVal = null;

        public float? Process(float? val)
        {
            float? returnVal = null;

            if (val.HasValue && _lastVal.HasValue)
            {
                returnVal = (val.Value - _lastVal.Value);

                // Negative values are not supported
                if (returnVal < 0)
                {
                    return null;
                }
            }

            _lastVal = val;

            return returnVal;
        }
    }
}
