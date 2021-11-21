// <copyright file="ChatAction.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace one.athavar.ffxivdeck.Action;

using Microsoft.Extensions.Logging;
using one.athavar.ffxivdeck.Settings;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;

/// <summary>
///     Action to send messages to ffxiv.
/// </summary>
[StreamDeckAction("Chat", "one.athavar.ffxivdeck.chat")]
internal class ChatAction : BaseAction<ChatSettings>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChatAction" /> class.
    /// </summary>
    /// <param name="logger"><see cref="ILogger{TCategoryName}" /> added by DI.</param>
    /// <param name="dataManager"><see cref="IDalamudDataManager" /> added by DI.</param>
    public ChatAction(ILogger<ChatAction> logger, IDalamudDataManager dataManager)
        : base(logger, dataManager)
    {
    }

    /// <inheritdoc />
    protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
    {
        await base.OnKeyDown(args);

        if (string.IsNullOrWhiteSpace(this.settings?.Message))
        {
            return;
        }

        await this.dataManager.SendChatMessageAsync(this.settings.Message, CancellationToken.None);
    }
}