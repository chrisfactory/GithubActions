
using System;
using System.Collections.Immutable;
using System.Reflection;
//using System.Text.Json;
//using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonFileExtractor.GitHubAction.Analyzers;

sealed class JsonFileAnalyzer
{
    readonly ILogger<JsonFileAnalyzer> _logger;

    public JsonFileAnalyzer(ILogger<JsonFileAnalyzer> logger) => _logger = logger;

    internal Task<IReadOnlyDictionary<string, object?>?> AnalyzeAsunc(string path, string properties, string propertiesAlias, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();

        if (File.Exists(path))
            _logger.LogInformation($"File found: {path}");
        else
        {
            _logger.LogWarning($"{path} doesn't exist.");
            return Task.FromResult(default(IReadOnlyDictionary<string, object?>));
        }

        var alias = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(propertiesAlias))
        {
            foreach (var pMap in propertiesAlias.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var map = pMap.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (map == null || map.Length != 2)
                    _logger.LogWarning($"{pMap}: the property alias not mapped.");
                else
                    alias.Add(map[0], map[1]);

            }
        }
        var result = new Dictionary<string, object?>();
        if (string.IsNullOrWhiteSpace(properties))
            return Task.FromResult(default(IReadOnlyDictionary<string, object?>));

        _logger.LogInformation($"Properties: {properties}");
        string jsonData = File.ReadAllText(path);
        if (jsonData != null && !string.IsNullOrEmpty(jsonData))
        {
            try
            {
                var settings = new JsonSerializerSettings { Formatting = Formatting.None };
                var data = JsonConvert.DeserializeObject<JObject>(jsonData, settings);


                if (data != null)
                {
                    foreach (string property in properties.Split(';', StringSplitOptions.RemoveEmptyEntries))
                    {
                        string formatedProperty;
                        var prop = data.SelectToken(property);
                        if (alias.ContainsKey(property))
                            formatedProperty = alias[property];
                        else
                            formatedProperty = $"{property.Replace('.', '-')}";
                        if (prop != null)
                        {
                            switch (prop.Type)
                            {
                                case JTokenType.None:
                                    result.Add(formatedProperty, prop);
                                    break;
                                case JTokenType.Object:
                                    result.Add(formatedProperty, JsonConvert.SerializeObject(prop.ToObject<object>(), Formatting.None));
                                    break;
                                case JTokenType.Array:
                                    result.Add(formatedProperty, JsonConvert.SerializeObject(prop.ToObject<Array>(), Formatting.None));
                                    break;
                                case JTokenType.Integer:
                                    result.Add(formatedProperty, prop.ToObject<int>());
                                    break;
                                case JTokenType.Float:
                                    result.Add(formatedProperty, prop.ToObject<float>());
                                    break;
                                case JTokenType.String:
                                    result.Add(formatedProperty, prop.ToObject<string>());
                                    break;
                                case JTokenType.Boolean:
                                    result.Add(formatedProperty, prop.ToObject<bool>());
                                    break;
                                case JTokenType.Null:
                                    result.Add(formatedProperty, null);
                                    break;
                                case JTokenType.Date:
                                    result.Add(formatedProperty, prop.ToObject<DateTime>());
                                    break;
                                case JTokenType.Raw:
                                    result.Add(formatedProperty, prop);
                                    break;
                                case JTokenType.Bytes:
                                    result.Add(formatedProperty, prop.ToObject<byte[]>());
                                    break;
                                case JTokenType.Guid:
                                    result.Add(formatedProperty, prop.ToObject<Guid>());
                                    break;
                                case JTokenType.Uri:
                                    result.Add(formatedProperty, prop.ToObject<Uri>());
                                    break;
                                case JTokenType.TimeSpan:
                                    result.Add(formatedProperty, prop.ToObject<TimeSpan>());
                                    break;

                                case JTokenType.Undefined:
                                case JTokenType.Constructor:
                                case JTokenType.Property:
                                case JTokenType.Comment:
                                default:
                                    throw new InvalidOperationException(prop.Type.ToString());

                            }

                        }
                        else
                        {
                            result.Add(formatedProperty, null);
                            _logger.LogWarning($"{path}: the property {formatedProperty} not found.");
                        }
                    }

                }
                else
                {
                    _logger.LogWarning($"{path} the file is empty");
                    return Task.FromResult(default(IReadOnlyDictionary<string, object?>));
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


        foreach (var item in result)
            _logger.LogInformation($"add property: {item.Key}={item.Value}");
        return Task.FromResult((IReadOnlyDictionary<string, object?>?)result);
    }
}
