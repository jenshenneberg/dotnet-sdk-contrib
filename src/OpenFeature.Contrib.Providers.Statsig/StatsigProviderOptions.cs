using System;
using System.Net.Http;

namespace OpenFeature.Contrib.Providers.Statsig
{
    /// <Summary>
    ///     StatsigProviderOptions contains the options to initialise the provider.
    /// </Summary>
    public class StatsigProviderOptions
    {
        /// <summary>
        /// Run Statsig in LocalMode
        /// </summary>
        public bool LocalMode { get; set; }
    }
}