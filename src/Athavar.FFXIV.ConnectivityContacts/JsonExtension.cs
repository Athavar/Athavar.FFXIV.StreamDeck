// <copyright file="JsonExtension.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.ConnectivityContacts;

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

internal static class JsonExtension
{
    public static T? ToObject<T>(this JsonElement element, JsonSerializerOptions? options = null)
    {
#if NET6_0_OR_GREATER
            return JsonSerializer.Deserialize<T>(element, options);
#else
        var bufferWriter = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(bufferWriter))
        {
            element.WriteTo(writer);
        }

        return JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan, options);
#endif
    }

    public static T? ToObject<T>(this JsonDocument document, JsonSerializerOptions? options = null)
    {
        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        return document.RootElement.ToObject<T>(options);
    }

    public static bool TryParseValue(ref Utf8JsonReader reader, [NotNullWhen(true)] out JsonElement? element)
    {
        var ret = JsonDocument.TryParseValue(ref reader, out var document);
        element = document?.RootElement;
        return ret;
    }
}