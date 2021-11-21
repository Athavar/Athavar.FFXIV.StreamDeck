// <copyright file="WebSocketMessageEventArgs.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.Common.Connectivity;

using System.ComponentModel;
using System.Text.Json.Serialization;

/// <summary>
///     Provides information about a message received by a web socket.
/// </summary>
public class WebSocketMessageEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WebSocketMessageEventArgs" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public WebSocketMessageEventArgs(string message)
        => this.Message = message;

    /// <summary>
    ///     Gets the message.
    /// </summary>
    [JsonPropertyName("message")]
    [DefaultValue("")]
    public string Message { get; }
}