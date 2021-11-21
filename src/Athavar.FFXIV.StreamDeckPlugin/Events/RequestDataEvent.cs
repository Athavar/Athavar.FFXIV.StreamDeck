// <copyright file="RequestDataEvent.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.StreamDeckPlugin.Events;

using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Athavar.Common.Connectivity;
using Athavar.FFXIV.ConnectivityContacts;
using Athavar.FFXIV.ConnectivityContacts.Dalamud;
using Athavar.FFXIV.ConnectivityContacts.Extensions;
using Athavar.FFXIV.StreamDeckPlugin.Manager.Interface;

/// <summary>
///     Represent the send data event.
/// </summary>
internal class RequestDataEvent : DalamudSocketEventHandler<DalamudSocketEventArgs<RequestDataArgs>>
{
    private readonly IIconManager iconManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequestDataEvent" /> class.
    /// </summary>
    /// <param name="iconManager"><see cref="IIconManager" /> added by DI.</param>
    public RequestDataEvent(IIconManager iconManager) => this.iconManager = iconManager;

    /// <inheritdoc />
    public override string EventName => RequestDataArgs.EventName;

    /// <inheritdoc />
    protected override Task HandleEventAsync(WebSocketConnection connection, DalamudSocketEventArgs<RequestDataArgs> args, CancellationToken cancellationToken)
    {
        var payload = args.Payload;
        if (payload is null || payload.Category is null)
        {
            return Task.CompletedTask;
        }

        switch (payload.Category)
        {
            case DataCategory.Icon:
                if (uint.TryParse(args.Payload?.Id, out var iconId))
                {
                    return this.SendIcon(connection, iconId, cancellationToken);
                }

                break;
        }

        return Task.CompletedTask;
    }

    private Task SendIcon(WebSocketConnection connection, uint iconId, CancellationToken cancellationToken)
    {
        string? icon = null;
        if (this.iconManager.IconExists(iconId))
        {
            icon = this.iconManager.GetBase64(iconId, true);
        }

        JsonObject o = new()
                       {
                           [SendDataArgs.IdName] = iconId,
                           [SendDataArgs.IconName] = icon,
                       };

        return this.SendDalamudSocketEventAsync(connection, DataCategory.Icon, o, cancellationToken);
    }

    private Task SendDalamudSocketEventAsync(WebSocketConnection connection, DataCategory category, JsonNode data, CancellationToken cancellationToken)
        => connection.SendDalamudSocketEventAsync(SendDataArgs.EventName, new SendDataArgs { Category = category, Data = data }, cancellationToken);
}