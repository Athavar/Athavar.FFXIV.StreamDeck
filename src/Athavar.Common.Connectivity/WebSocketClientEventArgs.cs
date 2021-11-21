// <copyright file="WebSocketClientEventArgs.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.Common.Connectivity;

/// <summary>
///     Provides information about the state of a connection.
/// </summary>
public class WebSocketClientEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WebSocketMessageEventArgs" /> class.
    /// </summary>
    /// <param name="client">The message.</param>
    public WebSocketClientEventArgs(ConnectionState state, WebSocketConnection client)
    {
        this.State = state;
        this.Client = client;
    }

    /// <summary>
    ///     Gets the message.
    /// </summary>
    public ConnectionState State { get; }

    /// <summary>
    ///     Gets the client object.
    /// </summary>
    public WebSocketConnection Client { get; }
}

public enum ConnectionState
{
    New,
    Close,
}