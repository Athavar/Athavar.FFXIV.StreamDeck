// <copyright file="SendChatMessageEvent.cs" company="Athavar">
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
///     Event to send a chat message.
/// </summary>
internal class SendChatMessageEvent : DalamudSocketEventHandler<DalamudSocketEventArgs<SendChatMessageArgs>>
{
    private readonly IDalamudServices binding;
    private readonly IChatManager chatManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SendChatMessageEvent" /> class.
    /// </summary>
    /// <param name="dalamud"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="chatManager"><see cref="ChatManager" /> added by DI.</param>
    public SendChatMessageEvent(IDalamudServices dalamud, IChatManager chatManager)
    {
        this.binding = dalamud;
        this.chatManager = chatManager;
    }

    /// <inheritdoc />
    public override string EventName => SendChatMessageArgs.EventName;

    /// <inheritdoc />
    protected override Task HandleEventAsync(WebSocketConnection connection, DalamudSocketEventArgs<SendChatMessageArgs> args, CancellationToken cancellationToken)
    {
        var message = args.Payload?.Message;

        if (message is not null && this.binding.ClientState.IsLoggedIn)
        {
            this.chatManager.SendMessage(message);
        }

        return Task.CompletedTask;
    }
}