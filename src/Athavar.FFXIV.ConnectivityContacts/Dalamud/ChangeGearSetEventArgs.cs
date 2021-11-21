// <copyright file="ChangeGearSetEventArgs.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.ConnectivityContacts.Dalamud;

using System.ComponentModel;
using System.Text.Json.Serialization;

/// <summary>
///     Data Payload for event to change gear-set.
/// </summary>
public class ChangeGearSetEventArgs
{
    /// <summary>
    ///     Event name.
    /// </summary>
    public const string EventName = "changegearset";

    /// <summary>
    ///     Gets or sets the slot number or name.
    /// </summary>
    [JsonPropertyName("slot")]
    [DefaultValue("")]
    public string? GearSlot { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the glamour plate number.
    /// </summary>
    [JsonPropertyName("glamourplate")]
    [DefaultValue(0)]
    public int GlamourPlate { get; set; }
}