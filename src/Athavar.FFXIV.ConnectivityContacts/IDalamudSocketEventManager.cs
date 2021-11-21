// <copyright file="IDalamudSocketEventManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.ConnectivityContacts;

using System.Reflection;
using Athavar.Common.Connectivity;

public interface IDalamudSocketEventManager
{
    /// <summary>
    ///     Init/Load events from <see cref="Assembly" />.
    /// </summary>
    /// <param name="assembly">The <see cref="Assembly" /> containing the <see cref="DalamudSocketEventHandler{T}" />.</param>
    void LoadEvents(Assembly assembly);

    void AttachEvent(Action<EventHandler<WebSocketMessageEventArgs>> @event);
}