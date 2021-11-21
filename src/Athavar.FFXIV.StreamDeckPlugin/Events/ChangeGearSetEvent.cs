// <copyright file="ChangeGearSetEvent.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.StreamDeckPlugin.Events;

using System.Threading;
using System.Threading.Tasks;
using Athavar.Common.Connectivity;
using Athavar.FFXIV.ConnectivityContacts;
using Athavar.FFXIV.ConnectivityContacts.Dalamud;
using Athavar.FFXIV.StreamDeckPlugin.Manager;
using Athavar.FFXIV.StreamDeckPlugin.Manager.Interface;

/// <summary>
///     Event to change the gear set.
/// </summary>
internal class ChangeGearSetEvent : DalamudSocketEventHandler<DalamudSocketEventArgs<ChangeGearSetEventArgs>>
{
    private const string ChatMessageFormat = "/gs change {0} {1}";
    private readonly IDalamudServices binding;
    private readonly IChatManager chatManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChangeGearSetEvent" /> class.
    /// </summary>
    /// <param name="dalamud"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="chatManager"><see cref="ChatManager" /> added by DI.</param>
    public ChangeGearSetEvent(IDalamudServices dalamud, IChatManager chatManager)
    {
        this.binding = dalamud;
        this.chatManager = chatManager;
    }

    /// <inheritdoc />
    public override string EventName => ChangeGearSetEventArgs.EventName;

    /// <inheritdoc />
    protected override Task HandleEventAsync(WebSocketConnection connection, DalamudSocketEventArgs<ChangeGearSetEventArgs> args, CancellationToken cancellationToken)
    {
        string GlamParameter(int? i) => i == 0 ? string.Empty : i?.ToString() ?? string.Empty;

        var payload = args.Payload;
        var slot = payload?.GearSlot;

        if (!string.IsNullOrWhiteSpace(slot) && this.binding.ClientState.IsLoggedIn)
        {
            this.chatManager.SendMessage(string.Format(ChatMessageFormat, slot, GlamParameter(payload?.GlamourPlate)));
        }

        return Task.CompletedTask;
    }
}