using System.Text.Json;
using System.Text.Json.Serialization;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace Teams.Common.Extensions;

/// <summary>Extension methods for serialization to/deserialization from JSON.</summary>
public static class JsonSerializationExtensions
{
    /// <summary>Default options applied when serializing objects.</summary>
    private static readonly JsonSerializerOptions DefaultSerializationOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Parses the text representing a JSON string into an instance of the specified type.</summary>
    /// <param name="json">The JSON text to parse.</param>
    /// <typeparam name="T">The target type of the JSON value.</typeparam>
    /// <returns>A <typeparamref name="T"/> representation of the JSON value.</returns>
    /// <exception cref="ArgumentNullException">The JSON is null</exception>
    /// <exception cref="JsonException">The JSON is invalid. -or- TValue is not compatible with the JSON. -or- There is remaining data in the string beyond a single JSON value.</exception>
    /// <exception cref="NotSupportedException">There is no compatible <see cref="JsonConverter"/> for TValue or its serializable members</exception>
    public static T? Deserialize<T>(this string? json)
        => json.Deserialize<T>(DefaultSerializationOptions);

    /// <summary>Parses the text representing a JSON string into an instance of the specified type.</summary>
    /// <param name="json">The JSON text to parse.</param>
    /// <param name="options">Options to control the behavior during parsing.</param>
    /// <typeparam name="T">The target type of the JSON value.</typeparam>
    /// <returns>A <typeparamref name="T"/> representation of the JSON value.</returns>
    /// <exception cref="ArgumentNullException">The JSON is null</exception>
    /// <exception cref="JsonException">The JSON is invalid. -or- TValue is not compatible with the JSON. -or- There is remaining data in the string beyond a single JSON value.</exception>
    /// <exception cref="NotSupportedException">There is no compatible <see cref="JsonConverter"/> for TValue or its serializable members</exception>
    public static T? Deserialize<T>(this string? json, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<T>(json ?? "null", options);

    /// <summary>Converts the value of a type specified by a generic type parameter into a JSON string.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the value.</returns>
    /// <exception cref="NotSupportedException">There is no compatible <see cref="JsonConverter"/> for TValue oSr its serializable members</exception>
    public static string Serialize<T>(this T? value)
        => value.Serialize(DefaultSerializationOptions);

    /// <summary>Converts the value of a type specified by a generic type parameter into a JSON string.</summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="options">Options to control serialization behavior.</param>
    /// <returns>A JSON string representation of the value.</returns>
    /// <exception cref="NotSupportedException">There is no compatible <see cref="JsonConverter"/> for TValue or its serializable members</exception>
    public static string Serialize<T>(this T? value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(value, options);
}