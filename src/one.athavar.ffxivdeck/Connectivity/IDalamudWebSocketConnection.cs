// <copyright file="IDalamudWebSocketConnection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace one.athavar.ffxivdeck.Connectivity;

using System.Net.WebSockets;
using Athavar.FFXIV.ConnectivityContacts.Dalamud;
using one.athavar.ffxivdeck.Settings;

internal interface IDalamudWebSocketConnection
{
    /// <summary>
    ///     Gets a value indicating whether the <see cref="WebSocket" /> is connected or not.
    /// </summary>
    bool InConnected { get; }

    /// <summary>
    ///     Gets or sets the port used by the <see cref="WebSocket" />.
    /// </summary>
    int Port { get; set; }

    /// <summary>
    ///     Start the connection to the Dalamud Plugin  asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task of sending the request.</returns>
    Task ConnectAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Sends request for chatmessage to the Dalamud Plugin asynchronously.
    /// </summary>
    /// <param name="message">The chat message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task of sending the request.</returns>
    Task SendChatMessageAsync(string message, CancellationToken cancellationToken);

    /// <summary>
    ///     Sends request for data to the Dalamud Plugin asynchronously.
    /// </summary>
    /// <param name="request">The request for the data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task of sending the request.</returns>
    Task SendDataRequestAsync(RequestDataArgs request, CancellationToken cancellationToken);

    /// <summary>
    ///     Sends request for data to the Dalamud Plugin asynchronously.
    /// </summary>
    /// <param name="settings">The settings containing the change data..</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task of sending the request.</returns>
    Task SendChangeGearSetRequest(GearSetSettings settings, CancellationToken cancellationToken);
}