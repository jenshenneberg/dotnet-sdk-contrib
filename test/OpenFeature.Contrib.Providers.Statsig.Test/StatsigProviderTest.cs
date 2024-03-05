using AutoFixture.Xunit2;
using OpenFeature.Constant;
using OpenFeature.Model;
using Statsig.Server;
using System;
using System.Threading.Tasks;
using Xunit;

namespace OpenFeature.Contrib.Providers.Statsig.Test;

public class StatsigProviderTest
{
    private StatsigProvider statsigProvider;

    public StatsigProviderTest()
    {
        statsigProvider =
            new StatsigProvider("secret-", new StatsigProviderOptions() { LocalMode = true });
    }

    [Fact]
    public async Task StatsigProvider_Initialized_HasCorrectStatusAsync()
    {
        Assert.Equal(ProviderStatus.NotReady, statsigProvider.GetStatus());
        await statsigProvider.Initialize(null);
        Assert.Equal(ProviderStatus.Ready, statsigProvider.GetStatus());

    }

    [Theory]
    [InlineAutoData(true, true)]
    [InlineAutoData(false, false)]
    public async Task GetBooleanValue_ForFeatureWithContext(bool flagValue, bool expectedValue, string userId, string flagName)
    {
        await statsigProvider.Initialize(null);
        var ec = EvaluationContext.Builder().Set("UserID", userId).Build();
        StatsigServer.OverrideGate(flagName, flagValue, userId);
        Assert.Equal(expectedValue, statsigProvider.ResolveBooleanValue(flagName, false, ec).Result.Value);
    }

    [Theory]
    [InlineAutoData(true, false)]
    [InlineAutoData(false, false)]
    public async Task GetBooleanValue_ForFeatureWithNoContext_ReturnsFalse(bool flagValue, bool expectedValue, string flagName)
    {
        await statsigProvider.Initialize(null);
        StatsigServer.OverrideGate(flagName, flagValue);
        Assert.Equal(expectedValue, statsigProvider.ResolveBooleanValue(flagName, false).Result.Value);
    }

    [Theory]
    [AutoData]
    public async Task GetBooleanValue_ForFeatureWithDefaultTrue_ThrowsException(string flagName)
    {
        await statsigProvider.Initialize(null);
        Assert.ThrowsAny<StatsigProviderException>(() => statsigProvider.ResolveBooleanValue(flagName, true).Result.Value);
    }


    //TODO: Test for default value true throws exception

    //TODO: Check context mapper

    //TODO: Ask about release of new version of openfeature dotnet sdk https://github.com/open-feature/dotnet-sdk/pull/231


}
