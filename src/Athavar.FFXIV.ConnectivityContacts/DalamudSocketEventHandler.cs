// <copyright file="DalamudSocketEventHandler.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.ConnectivityContacts;

using System.Text.Json;
using Athavar.Common.Connectivity;

/// <summary>
///     Abstraction to handles a specific event.
/// </summary>
public abstract class DalamudSocketEventHandler
{
    /// <summary>
    ///     Gets the event name.
    /// </summary>
    public abstract string EventName { get; }

    /// <summary>
    ///     Handles the event.
    /// </summary>
    /// <param name="connection">The connection receiving the event.</param>
    /// <param name="element">The event data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    public abstract Task HandleEventAsync(WebSocketConnection connection, JsonElement element, CancellationToken cancellationToken);
}