using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace OrderManagement.Infrastructure.Logging
{
    [ProviderAlias("Loki")]
    public class LokiLoggerProvider : ILoggerProvider
    {
        private readonly HttpClient _httpClient;
        private readonly LokiLoggerConfiguration _config;
        private readonly ConcurrentDictionary<string, LokiLogger> _loggers = new ConcurrentDictionary<string, LokiLogger>();

        public LokiLoggerProvider(IOptionsMonitor<LokiLoggerConfiguration> config)
        {
            _config = config.CurrentValue;
            _httpClient = new HttpClient();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new LokiLogger(name, _config, _httpClient, new Dictionary<string, string>(_config.DefaultLabels)));
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
