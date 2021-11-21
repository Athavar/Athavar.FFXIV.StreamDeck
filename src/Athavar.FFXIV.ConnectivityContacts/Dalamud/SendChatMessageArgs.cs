// <copyright file="SendChatMessageArgs.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.ConnectivityContacts.Dalamud;

using System.ComponentModel;
using System.Text.Json.Serialization;

/// <summary>
///     Data Payload for event to send a chat message.
/// </summary>
public class SendChatMessageArgs
{
    /// <summary>
    ///     Event name.
    /// </summary>
    public const string EventName = "sendchatmassage";

    /// <summary>
    ///     Gets the text to be send in the chat.
    /// </summary>
    [JsonPropertyName("message")]
    [DefaultValue("")]
    public string? Message { get; init; }
}