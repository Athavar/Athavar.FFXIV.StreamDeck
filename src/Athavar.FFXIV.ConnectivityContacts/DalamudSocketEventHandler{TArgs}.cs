// <copyright file="DalamudSocketEventHandler{TArgs}.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.ConnectivityContacts;

using System.Text.Json;
using Athavar.Common.Connectivity;

/// <summary>
///     Abstraction to handles a specific event.
/// </summary>
/// <typeparam name="T">Defines the payload of the event.</typeparam>
public abstract class DalamudSocketEventHandler<T> : DalamudSocketEventHandler
    where T : DalamudSocketEventArgs
{
    /// <inheritdoc />
    public override Task HandleEventAsync(WebSocketConnection connection, JsonElement element, CancellationToken cancellationToken)
    {
        var args = element.ToObject<T>();

        if (args is not null)
        {
            return this.HandleEventAsync(connection, args, cancellationToken);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handles the event.
    /// </summary>
    /// <param name="connection">The connection receiving the event.</param>
    /// <param name="args">The event data as specific object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    protected abstract Task HandleEventAsync(WebSocketConnection connection, T args, CancellationToken cancellationToken);
}