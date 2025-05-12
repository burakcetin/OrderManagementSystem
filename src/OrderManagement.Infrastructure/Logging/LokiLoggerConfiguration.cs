using Microsoft.Extensions.Logging;

namespace OrderManagement.Infrastructure.Logging
{
    public class LokiLoggerConfiguration
    {
        public string LokiUrl { get; set; } = "http://localhost:3100";
        public LogLevel MinLevel { get; set; } = LogLevel.Information;
        public int BatchSize { get; set; } = 100;
        public Dictionary<string, string> DefaultLabels { get; set; } = new Dictionary<string, string>();
    }
}
