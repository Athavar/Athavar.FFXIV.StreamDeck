// <copyright file="DalamudSocketEventArgs{TPayload}.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.ConnectivityContacts;

using System.ComponentModel;
using System.Text.Json.Serialization;

/// <summary>
///     Provides information about an event received from an Elgato Stream Deck.
/// </summary>
public class DalamudSocketEventArgs<TPayload> : DalamudSocketEventArgs
{
    /// <summary>
    ///     Gets or sets the main payload associated with the event.
    /// </summary>
    [JsonPropertyName("payload")]
    [DefaultValue(null)]
    public TPayload? Payload { get; init; }
}