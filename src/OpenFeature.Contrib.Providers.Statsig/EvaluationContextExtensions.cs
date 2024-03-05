using OpenFeature.Model;
using Statsig;

namespace OpenFeature.Contrib.Providers.Statsig
{
    internal static class EvaluationContextExtensions
    {

        public static StatsigUser AsStatsigUser(this EvaluationContext evaluationContext)
        {
            if (evaluationContext == null)
                return null;

            var su = new StatsigUser();
            foreach (var item in evaluationContext.AsDictionary())
            {
                //TODO: Await release containing this https://github.com/open-feature/dotnet-sdk/pull/231 to use TargetingKey instead of UserId
                if (item.Key == "UserID")
                    su.UserID = item.Value.AsString;
                else
                    su.AddCustomProperty(item.Key, item.Value);
            }
            return su;
        }

    }
}
