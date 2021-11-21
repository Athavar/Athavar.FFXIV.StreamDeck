// <copyright file="GetDataEvent.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace one.athavar.ffxivdeck.Events;

using Athavar.Common.Connectivity;
using Athavar.FFXIV.ConnectivityContacts;
using Athavar.FFXIV.ConnectivityContacts.Dalamud;

/// <summary>
///     Handle the event of receiving new data.
/// </summary>
internal class GetDataEvent : DalamudSocketEventHandler<DalamudSocketEventArgs<SendDataArgs>>
{
    private readonly IDalamudDataManager dataManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetDataEvent" /> class.
    /// </summary>
    /// <param name="dataManager"><see cref="IDalamudDataManager" /> added by DI.</param>
    public GetDataEvent(IDalamudDataManager dataManager) => this.dataManager = dataManager;

    /// <inheritdoc />
    public override string EventName => SendDataArgs.EventName;

    /// <inheritdoc />
    protected override Task HandleEventAsync(WebSocketConnection connection, DalamudSocketEventArgs<SendDataArgs> args, CancellationToken cancellationToken)
    {
        var data = args.Payload;
        if (data is not null)
        {
            this.dataManager.ReceiveNewData(data);
        }

        return Task.CompletedTask;
    }
}