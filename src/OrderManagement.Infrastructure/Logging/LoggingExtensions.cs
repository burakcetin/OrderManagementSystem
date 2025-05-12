using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrderManagement.Infrastructure.Logging
{
    public static class LoggingExtensions
    {
        public static ILoggingBuilder AddLoki(this ILoggingBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LokiLoggerProvider>());
            return builder;
        }

        public static ILoggingBuilder AddLoki(this ILoggingBuilder builder, Action<LokiLoggerConfiguration> configure)
        {
            builder.AddLoki();
            builder.Services.Configure(configure);
            return builder;
        }
    }
}
