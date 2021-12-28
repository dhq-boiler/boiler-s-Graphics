using NLog;
using Unity.Builder;
using Unity.Extension;
using Unity.Strategies;

namespace boilersGraphics.Extensions
{
    public class LogResolvesUnityContainerExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Context.Strategies.Add(new LoggingStrategy(), UnityBuildStage.PreCreation);
        }

        private class LoggingStrategy : BuilderStrategy
        {
            public LoggingStrategy()
            {
            }

            public override void PreBuildUp(ref BuilderContext context)
            {
                // Be aware that for Singleton Resolving this log message will only be logged once, when the Singleton is first resolved. After that, there is no buildup and it is just returned from a cache.

                var registrationType = context.RegistrationType;
                var registrationName = context.Name;
                var resolvedType = context.Type;

                var registrationNameWithParenthesesOrNothing = string.IsNullOrEmpty(registrationName) ? "" : $"({registrationName})";
                LogManager.GetCurrentClassLogger().Debug($"Resolving [{registrationType}{registrationNameWithParenthesesOrNothing}] => [{resolvedType}]");
            }
        }
    }
}
