﻿using OpenFeature.Constant;
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
            if (GetStatus() != ProviderStatus.Ready)
                return Task.FromResult(new ResolutionDetails<bool>(flagKey, defaultValue, ErrorType.ProviderNotReady));
            var result = StatsigServer.CheckGateSync(context.AsStatsigUser(), flagKey);
            //Workaround for fallback to true default value
            if (result == false && defaultValue == true)
            {
                if (!StatsigServer.GetFeatureGateList().Exists(x => x == flagKey.ToLowerInvariant()))
                {
                    return Task.FromResult(new ResolutionDetails<bool>(flagKey, true));
                }
            }

            return Task.FromResult(new ResolutionDetails<bool>(flagKey, result));
        }

        public override Task<ResolutionDetails<double>> ResolveDoubleValue(string flagKey, double defaultValue, EvaluationContext context = null)
        {
            throw new NotImplementedException();
        }

        public override Task<ResolutionDetails<int>> ResolveIntegerValue(string flagKey, int defaultValue, EvaluationContext context = null)
        {
            throw new NotImplementedException();
        }

        public override Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue, EvaluationContext context = null)
        {
            throw new NotImplementedException();
        }

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

            //// We start listening for status changes and then we check the current status change. If we do not check
            //// then we could have missed a status change. If we check before registering a listener, then we could
            //// miss a change between checking and listening. Doing it this way we can get duplicates, but we filter
            //// when the status does not actually change, so we won't emit duplicate events.
            //if (_client.Initialized)
            //{
            //    _statusProvider.SetStatus(ProviderStatus.Ready);
            //    _initCompletion.TrySetResult(true);
            //}

            //if (_client.DataSourceStatusProvider.Status.State == DataSourceState.Off)
            //{
            //    _statusProvider.SetStatus(ProviderStatus.Error, ProviderShutdownMessage);
            //    _initCompletion.TrySetException(new LaunchDarklyProviderInitException(ProviderShutdownMessage));
            //}
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
