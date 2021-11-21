// <copyright file="DalamudSocketEventArgs.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.ConnectivityContacts;

using System.ComponentModel;
using System.Text.Json.Serialization;

/// <summary>
///     Provides information about an event send from or to the Dalamud Plugin.
/// </summary>
public class DalamudSocketEventArgs
{
    /// <summary>
    ///     Gets the name of the event.
    /// </summary>
    [JsonPropertyName("event")]
    [DefaultValue("")]
    public string Event { get; init; } = string.Empty;
}