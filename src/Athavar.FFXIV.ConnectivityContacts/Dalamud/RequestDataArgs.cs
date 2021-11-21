// <copyright file="RequestDataArgs.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.ConnectivityContacts.Dalamud;

using System.ComponentModel;
using System.Text.Json.Serialization;

/// <summary>
///     Data Payload for event to request data.
/// </summary>
public class RequestDataArgs
{
    /// <summary>
    ///     Event name.
    /// </summary>
    public const string EventName = "requestdata";

    /// <summary>
    ///     Gets category of the requested data.
    /// </summary>
    [JsonPropertyName("category")]
    [DefaultValue(DataCategory.None)]
    public DataCategory? Category { get; init; }

    /// <summary>
    ///     Gets id of the requested data.
    /// </summary>
    [JsonPropertyName("Id")]
    [DefaultValue(null)]
    public string? Id { get; init; }
}