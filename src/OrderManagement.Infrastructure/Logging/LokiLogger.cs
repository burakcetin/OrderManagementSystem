using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;

namespace OrderManagement.Infrastructure.Logging
{
    public class LokiLogger : ILogger
    {
        private readonly string _name;
        private readonly LokiLoggerConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ConcurrentQueue<LokiLogEntry> _logQueue = new ConcurrentQueue<LokiLogEntry>();
        private readonly Timer _flushTimer;
        private readonly Dictionary<string, string> _labels;

        public LokiLogger(string name, LokiLoggerConfiguration config, HttpClient httpClient, Dictionary<string, string> labels)
        {
            _name = name;
            _config = config;
            _httpClient = httpClient;
            _labels = labels ?? new Dictionary<string, string>();

            // Add category name as a label
            if (!_labels.ContainsKey("logger"))
                _labels.Add("logger", name);

          
            _flushTimer = new Timer(FlushLogs, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _config.MinLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);

            if (exception != null)
                message += $"\nException: {exception.Message}\nStackTrace: {exception.StackTrace}";

            // Create log entry
            var entry = new LokiLogEntry
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000000, // Convert to nanoseconds
                Message = message,
                Level = logLevel.ToString()
            };

      
            _logQueue.Enqueue(entry);

           
            if (_logQueue.Count >= _config.BatchSize)
            {
                Task.Run(() => FlushLogs(null));
            }
        }

        private async void FlushLogs(object? state)
        {
            if (_logQueue.IsEmpty)
                return;

            var entries = new List<LokiLogEntry>();
            while (_logQueue.TryDequeue(out var entry) && entries.Count < _config.BatchSize)
            {
                entries.Add(entry);
            }

            if (entries.Count == 0)
                return;

            try
            {
                // Create Loki Push API request
                var streams = new List<LokiStream>
                {
                    new LokiStream
                    {
                        Stream = _labels,
                        Values = entries.Select(e => new[] { e.Timestamp.ToString(), $"{e.Level}: {e.Message}" }).ToArray()
                    }
                };

                var lokiRequest = new LokiPushRequest { Streams = streams };

                // Send to Loki
                var response = await _httpClient.PostAsJsonAsync($"{_config.LokiUrl}/loki/api/v1/push", lokiRequest);

                if (!response.IsSuccessStatusCode)
                {
                    // Log failure to console
                    var content = await response.Content.ReadAsStringAsync();
                    Console.Error.WriteLine($"Failed to send logs to Loki: {response.StatusCode}, {content}");
                }
            }
            catch (Exception ex)
            {
                // Log failure to console
                Console.Error.WriteLine($"Error sending logs to Loki: {ex.Message}");
            }
        }

        private class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();

            public void Dispose() { }
        }
    }

    public class LokiLogEntry
    {
        public long Timestamp { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
    }

    public class LokiStream
    {
        public Dictionary<string, string> Stream { get; set; } = new Dictionary<string, string>();
        public string[][] Values { get; set; } = Array.Empty<string[]>();
    }

    public class LokiPushRequest
    {
        public List<LokiStream> Streams { get; set; } = new List<LokiStream>();
    }
}
