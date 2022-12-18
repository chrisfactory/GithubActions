using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonFileExtractor.GitHubAction.Analyzers;

sealed class JsonFileAnalyzer
{
    readonly ILogger<JsonFileAnalyzer> _logger;

    public JsonFileAnalyzer(ILogger<JsonFileAnalyzer> logger) => _logger = logger;

    internal Task<IReadOnlyDictionary<string, string?>?> AnalyzeAsunc(string path, string[] properties, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();

        if (File.Exists(path))
            _logger.LogInformation($"Computing analytics on {path}.");
        else
        {
            _logger.LogWarning($"{path} doesn't exist.");
            return Task.FromResult(default(IReadOnlyDictionary<string, string?>));
        }

        var result = new Dictionary<string, string?>();

        string jsonData = File.ReadAllText(path);
        if (jsonData != null && !string.IsNullOrEmpty(jsonData))
        {
            try
            {
                var data = JsonConvert.DeserializeObject<JObject>(jsonData);
                if (data != null)
                {
                    foreach (var property in properties)
                    {
                        var prop = data.SelectToken(property);
                        if (prop != null)
                        {
                            result.Add(property, prop.ToString());
                        }
                        else
                        {
                            result.Add(property, null);
                            _logger.LogWarning($"{path}: the property {property} not found.");
                        }
                    }

                }
                else
                {
                    _logger.LogWarning($"{path} the file is empty");
                    return Task.FromResult(default(IReadOnlyDictionary<string, string?>));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{path}: Exception");
            }

        }
        else
        {
            _logger.LogWarning($"{path} the file is empty");
        }

        return Task.FromResult((IReadOnlyDictionary<string, string?>?)result);
    }
}
