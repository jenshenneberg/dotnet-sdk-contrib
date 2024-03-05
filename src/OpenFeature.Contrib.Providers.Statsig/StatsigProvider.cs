using OpenFeature.Constant;
using OpenFeature.Model;
using Statsig;
using Statsig.Client;
using Statsig.Server;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFeature.Contrib.Providers.Statsig
{
    /// <summary>
    /// An OpenFeature <see cref="FeatureProvider"/> which enables the use of the Statsig Server-Side SDK for .NET
    /// with OpenFeature.
    /// </summary>
    /// <example>
    ///     var provider = new Provider(Configuration.Builder("my-sdk-key").Build());
    ///
    ///     OpenFeature.Api.Instance.SetProvider(provider);
    ///
    ///     var client = OpenFeature.Api.Instance.GetClient();
    /// </example>
    public sealed class StatsigProvider : FeatureProvider
    {
        bool initialized = false;
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly Metadata _providerMetadata = new Metadata("Statsig provider");
        private readonly string sdkKey = "secret-"; //Dummy sdk key that works with local mode
        private readonly StatsigProviderOptions _options;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sdkKey"></param>
        /// <param name="options"></param>
        public StatsigProvider(string sdkKey, StatsigProviderOptions options)
        {
            ValidateInputOptions(options);
            this.sdkKey = sdkKey;
            _options = options;
        }

        /// <summary>
        ///     validateInputOptions is validating the different options provided when creating the provider.
        /// </summary>
        /// <param name="options">Options used while creating the provider</param>
        /// <exception cref="StatsigProviderException">if no options are provided or we have a wrong configuration.</exception>
        private void ValidateInputOptions(StatsigProviderOptions options)
        {
            if (options is null) throw new StatsigProviderException("No options provided");
        }

        /// <inheritdoc/>
        public override Metadata GetMetadata() => _providerMetadata;

        /// <inheritdoc/>
        public override Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null)
        {
            //TODO: defaultvalue = true not yet supported due to https://github.com/statsig-io/dotnet-sdk/issues/33
            if (defaultValue == true)
                throw new StatsigProviderException("defaultvalue = true not supported(https://github.com/statsig-io/dotnet-sdk/issues/33)");
            if (GetStatus() != ProviderStatus.Ready)
                return Task.FromResult(new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.ProviderNotReady));
            var result = StatsigServer.CheckGateSync(context.AsStatsigUser(), flagKey);
            return Task.FromResult(new ResolutionDetails<bool>(flagKey, result));
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<double>> ResolveDoubleValue(string flagKey, double defaultValue, EvaluationContext context = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<int>> ResolveIntegerValue(string flagKey, int defaultValue, EvaluationContext context = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue, EvaluationContext context = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<Value>> ResolveStructureValue(string flagKey, Value defaultValue, EvaluationContext context = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override ProviderStatus GetStatus()
        {
            return initialized ? ProviderStatus.Ready : ProviderStatus.NotReady;
        }

        /// <inheritdoc />
        public override async Task Initialize(EvaluationContext context)
        {
            if (!initialized)
            {
                await semaphore.WaitAsync();
                try
                {
                    var initResult = await StatsigServer.Initialize(sdkKey, new StatsigServerOptions() { LocalMode = _options.LocalMode });
                    if (initResult == InitializeResult.Success || initResult == InitializeResult.AlreadyInitialized || initResult == InitializeResult.LocalMode)
                    {
                        initialized = true;
                    }
                    else
                        initialized = false;
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <inheritdoc />
        public override Task Shutdown()
        {
            if (initialized)
                return StatsigServer.Shutdown();
            return Task.CompletedTask;
        }
    }
}
