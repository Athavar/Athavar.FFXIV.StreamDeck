// <copyright file="DalamudWebSocketConnection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace one.athavar.ffxivdeck.Connectivity;

using System.Text.Json;
using Athavar.Common.Connectivity;
using Athavar.FFXIV.ConnectivityContacts;
using Athavar.FFXIV.ConnectivityContacts.Dalamud;
using Athavar.FFXIV.ConnectivityContacts.Extensions;
using Microsoft.Extensions.Logging;
using one.athavar.ffxivdeck.Settings;

/// <summary>
///     The <see cref="WebSocket" /> connection to Dalamud Plugin.
/// </summary>
internal sealed class DalamudWebSocketConnection : IDalamudWebSocketConnection
{
    private readonly ILogger logger;
    private readonly IDalamudSocketEventManager eventManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DalamudWebSocketConnection" /> class.
    /// </summary>
    /// <param name="logger"><see cref="ILogger{TCategoryName}" /> added by DI.</param>
    /// <param name="eventManager"><see cref="IDalamudDataManager" /> added by DI.</param>
    public DalamudWebSocketConnection(ILogger<DalamudWebSocketConnection> logger, IDalamudSocketEventManager eventManager)
    {
        this.logger = logger;
        this.eventManager = eventManager;
    }

    /// <inheritdoc />
    public bool InConnected => this.WebSocket?.IsConnected ?? false;

    /// <inheritdoc />
    public int Port { get; set; }

    /// <summary>
    ///     Gets the default JSON settings.
    /// </summary>
    private JsonSerializerOptions JsonSettings { get; } = new()
                                                          {
                                                              PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                              WriteIndented = false,
                                                          };

    /// <summary>
    ///     Gets or sets the web socket.
    /// </summary>
    private WebSocketConnection? WebSocket { get; set; }

    /// <inheritdoc />
    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Connecting to Stream Deck");
        this.WebSocket = new WebSocketConnection($"ws://127.0.0.1:{this.Port}/", this.JsonSettings);
        this.eventManager.AttachEvent(e => this.WebSocket.MessageReceived += e);

        await this.WebSocket.ConnectAsync(cancellationToken);
        this.logger.LogTrace("Connected to Dalamud Plugin;");

        await this.WebSocket.WaitDisconnectAsync(cancellationToken);

        // cleanup
        this.eventManager.AttachEvent(e => this.WebSocket.MessageReceived -= e);
        var socket = this.WebSocket;
        this.WebSocket = null;
        socket.Dispose();
    }

    /// <inheritdoc />
    public Task SendChatMessageAsync(string message, CancellationToken cancellationToken)
        => this.SendDalamudSocketEventAsync(SendChatMessageArgs.EventName, new SendChatMessageArgs { Message = message }, cancellationToken);

    /// <inheritdoc />
    public Task SendDataRequestAsync(RequestDataArgs request, CancellationToken cancellationToken)
        => this.SendDalamudSocketEventAsync(RequestDataArgs.EventName, request, cancellationToken);

    /// <inheritdoc />
    public Task SendChangeGearSetRequest(GearSetSettings settings, CancellationToken cancellationToken)
        => this.SendDalamudSocketEventAsync(
            ChangeGearSetEventArgs.EventName,
            new ChangeGearSetEventArgs
            {
                GearSlot = settings.GearSlot,
                GlamourPlate = settings.GlamourPlate,
            },
            cancellationToken);

    private Task SendDalamudSocketEventAsync<T>(string eventName, T value, CancellationToken cancellationToken)
        where T : class =>
        this.WebSocket?.SendAsync(
            new DalamudSocketEventArgs<T>
            {
                Event = eventName,
                Payload = value,
            },
            cancellationToken) ?? Task.CompletedTask;
}