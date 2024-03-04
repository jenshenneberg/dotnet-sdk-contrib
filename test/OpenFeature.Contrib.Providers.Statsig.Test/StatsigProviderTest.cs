using AutoFixture.Xunit2;
using OpenFeature.Constant;
using OpenFeature.Model;
using Statsig.Server;
using System.Threading.Tasks;
using Xunit;

namespace OpenFeature.Contrib.Providers.Statsig.Test;

public class StatsigProviderTest
{
    [Fact]
    public async Task StatsigProvider_Initialized_HasCorrectStatusAsync()
    {
        var statsigProvider =
            new StatsigProvider("secret-", new StatsigProviderOptions() { LocalMode = true });

        Assert.Equal(ProviderStatus.NotReady, statsigProvider.GetStatus());
        await statsigProvider.Initialize(null);

        Assert.Equal(ProviderStatus.Ready, statsigProvider.GetStatus());

    }

    [Theory]
    [InlineAutoData(true, true)]
    [InlineAutoData(false, false)]
    public async Task GetBooleanValue_ForFeatureWithContext(bool flagValue, bool expectedValue, string userId, string flagName)
    {
        var statsigProvider = new StatsigProvider("secret-", new StatsigProviderOptions() { LocalMode = true });
        await statsigProvider.Initialize(null);
        var ec = EvaluationContext.Builder().Set("UserID", userId).Build();
        StatsigServer.OverrideGate(flagName, flagValue, userId);
        Assert.Equal(expectedValue, statsigProvider.ResolveBooleanValue(flagName, false, ec).Result.Value);
    }

    //TODO Check java implementation
    [Theory]
    [InlineAutoData(true, false)]
    [InlineAutoData(false, false)]
    public async Task GetBooleanValue_ForFeatureNoContext(bool flagValue, bool expectedValue, string flagName)
    {
        var statsigProvider = new StatsigProvider("secret-", new StatsigProviderOptions() { LocalMode = true });
        await statsigProvider.Initialize(null);
        StatsigServer.OverrideGate(flagName, flagValue);
        Assert.Equal(expectedValue, statsigProvider.ResolveBooleanValue(flagName, false).Result.Value);
    }

    //TODO: Test for default value true throws exception

    //TODO: Check context mapper

    //TODO: Ask about release of new version of statsig dotnet sdk


}
