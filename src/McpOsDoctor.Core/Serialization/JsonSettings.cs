using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace McpOsDoctor.Core.Serialization;

/// <summary>
/// Shared Newtonsoft.Json serialization settings for the MCP OS Doctor domain types.
/// Uses camelCase properties, string enums, and omits null values.
/// </summary>
public static class JsonSettings
{
    /// <summary>
    /// Default serializer settings used for all tool responses and error objects.
    /// </summary>
    public static JsonSerializerSettings Default { get; } = CreateDefault();

    private static JsonSerializerSettings CreateDefault()
    {
        return new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            Converters = { new StringEnumConverter(new CamelCaseNamingStrategy()) }
        };
    }
}