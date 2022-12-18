 
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

    internal Task<IReadOnlyDictionary<string, object?>?> AnalyzeAsunc(string path, string properties, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();

        if (File.Exists(path))
            _logger.LogInformation($"File found: {path}");
        else
        {
            _logger.LogWarning($"{path} doesn't exist.");
            return Task.FromResult(default(IReadOnlyDictionary<string, object?>));
        }

        var result = new Dictionary<string, object?>();
        if(string .IsNullOrWhiteSpace(properties))
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
                    foreach (var property in properties.Split(';'))
                    {
                        var prop = data.SelectToken(property);
                        if (prop != null)
                        {
                            switch (prop.Type)
                            {
                                case JTokenType.None:
                                    result.Add(property, prop);
                                    break;
                                case JTokenType.Object:
                                    result.Add(property, JsonConvert.SerializeObject(prop.ToObject<object>(), Formatting.None));
                                    break;
                                case JTokenType.Array:
                                    result.Add(property, JsonConvert.SerializeObject(prop.ToObject<Array>(), Formatting.None));
                                    break;
                                case JTokenType.Integer:
                                    result.Add(property, prop.ToObject<int>());
                                    break;
                                case JTokenType.Float:
                                    result.Add(property, prop.ToObject<float>());
                                    break;
                                case JTokenType.String: 
                                    result.Add(property, prop.ToObject<string>());
                                    break;
                                case JTokenType.Boolean:
                                    result.Add(property, prop.ToObject<bool>());
                                    break;
                                case JTokenType.Null:
                                    result.Add(property, null);
                                    break; 
                                case JTokenType.Date:
                                    result.Add(property, prop.ToObject<DateTime>());
                                    break;
                                case JTokenType.Raw:
                                    result.Add(property, prop);
                                    break;
                                case JTokenType.Bytes:
                                    result.Add(property, prop.ToObject<byte[]>());
                                    break;
                                case JTokenType.Guid:
                                    result.Add(property, prop.ToObject<Guid>());
                                    break;
                                case JTokenType.Uri:
                                    result.Add(property, prop.ToObject<Uri>());
                                    break;
                                case JTokenType.TimeSpan:
                                    result.Add(property, prop.ToObject<TimeSpan>());
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
                            result.Add(property, null);
                            _logger.LogWarning($"{path}: the property {property} not found.");
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

        return Task.FromResult((IReadOnlyDictionary<string, object?>?)result);
    }
}
