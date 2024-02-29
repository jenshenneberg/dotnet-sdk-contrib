using System;
using System.Net.Http;

namespace OpenFeature.Contrib.Providers.Statsig
{
    /// <Summary>
    ///     GoFeatureFlagProviderOptions contains the options to initialise the provider.
    /// </Summary>
    public class StatsigProviderOptions
    {
        /// <Summary>
        ///     (optional) timeout we are waiting when calling the go-feature-flag relay proxy API.
        ///     Default: 10000 ms
        /// </Summary>
        public TimeSpan Timeout { get; set; } = new TimeSpan(10000 * TimeSpan.TicksPerMillisecond);


        public bool LocalMode { get; set; }
    }
}