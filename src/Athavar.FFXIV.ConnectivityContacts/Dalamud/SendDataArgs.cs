// <copyright file="SendDataArgs.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.ConnectivityContacts.Dalamud;

using System.ComponentModel;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

/// <summary>
///     Data Payload for event to send data.
/// </summary>
public class SendDataArgs
{
    /// <summary>
    ///     Event name.
    /// </summary>
    public const string EventName = "senddata";

    /// <summary>
    ///     Id property name.
    /// </summary>
    public const string IdName = "id";

    /// <summary>
    ///     icon property name.
    /// </summary>
    public const string IconName = "icon";

    /// <summary>
    ///     Gets the category of the data sent.
    /// </summary>
    [JsonPropertyName("category")]
    [DefaultValue(DataCategory.None)]
    public DataCategory? Category { get; init; }

    /// <summary>
    ///     Gets the data sent.
    /// </summary>
    [JsonPropertyName("data")]
    [DefaultValue(null)]
    public JsonNode? Data { get; init; }
}