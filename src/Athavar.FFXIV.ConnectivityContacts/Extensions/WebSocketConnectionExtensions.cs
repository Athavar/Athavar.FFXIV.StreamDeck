// <copyright file="WebSocketConnectionExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.ConnectivityContacts.Extensions;

using Athavar.Common.Connectivity;

/// <summary>
///     Extends the class <see cref="WebSocketConnection" />.
/// </summary>
public static class WebSocketConnectionExtensions
{
    /// <summary>
    ///     Sends the value to or from the Dalamud Plugin asynchronously.
    /// </summary>
    /// <typeparam name="T">The Type of the payload.</typeparam>
    /// <param name="connection">The websocket connection.</param>
    /// <param name="eventName">The event value.</param>
    /// <param name="value">The value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task of sending the value.</returns>
    public static Task SendDalamudSocketEventAsync<T>(this WebSocketConnection connection, string eventName, T value, CancellationToken cancellationToken)
        where T : class =>
        connection.SendAsync(new DalamudSocketEventArgs<T> { Event = eventName, Payload = value }, cancellationToken);

    /// <summary>
    ///     Sends the value to or from the Dalamud Plugin asynchronously.
    /// </summary>
    /// <typeparam name="T">The Event data to be sent.</typeparam>
    /// <param name="connection">The websocket connection.</param>
    /// <param name="value">The value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task of sending the value.</returns>
    public static Task SendAsync<T>(this WebSocketConnection connection, DalamudSocketEventArgs<T> value, CancellationToken cancellationToken)
        where T : class =>
        connection.SendJsonAsync(value, cancellationToken) ?? Task.CompletedTask;
}