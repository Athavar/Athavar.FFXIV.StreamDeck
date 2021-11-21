// <copyright file="DalamudDataManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace one.athavar.ffxivdeck.Services;

using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json.Nodes;
using Athavar.FFXIV.ConnectivityContacts.Dalamud;
using one.athavar.ffxivdeck.Connectivity;
using one.athavar.ffxivdeck.Settings;

internal class DalamudDataManager : IDalamudDataManager
{
    private readonly TimeSpan timeout = TimeSpan.FromSeconds(60);
    private readonly ConcurrentDictionary<uint, string?> iconCache = new();
    private readonly IDalamudWebSocketConnection connection;

    private EventHandler<SendDataArgs>? didReceivedData;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DalamudDataManager" /> class.
    /// </summary>
    /// <param name="connection"><see cref="IDalamudDataManager" /> added by DI.</param>
    public DalamudDataManager(IDalamudWebSocketConnection connection) => this.connection = connection;

    /// <inheritdoc />
    public bool IsConnected => this.connection.InConnected;

    /// <inheritdoc />
    public void ReceiveNewData(SendDataArgs args)
    {
        this.didReceivedData?.Invoke(null, args);
    }

    /// <inheritdoc />
    public Task<string?> GetIconAsync(uint icon, CancellationToken cancellationToken)
    {
        if (this.iconCache.TryGetValue(icon, out var base64String))
        {
            return Task.FromResult(base64String);
        }

        bool OnDataReceive(JsonObject data, TaskCompletionSource<string?> completionSource)
        {
            if (!data.TryGetPropertyValue(SendDataArgs.IdName, out var idValue) || idValue is null || idValue.GetValue<uint>() != icon || !data.TryGetPropertyValue(SendDataArgs.IconName, out var iconValue))
            {
                return false;
            }

            var iconString = iconValue?.GetValue<string>();
            if (completionSource.TrySetResult(iconString))
            {
                this.iconCache.TryAdd(icon, iconString);
                return true;
            }

            return false;
        }

        return this.WaitGetData<string>(DataCategory.Icon, token => this.RequestIcon(icon, token), OnDataReceive, cancellationToken);
    }

    /// <inheritdoc />
    public Task SendChatMessageAsync(string message, CancellationToken cancellationToken)
        => this.connection.SendChatMessageAsync(message, cancellationToken);

    /// <inheritdoc />
    public Task ChangeGearSetAsync(GearSetSettings settings, CancellationToken cancellationToken)
        => this.connection.SendChangeGearSetRequest(settings, cancellationToken);

    private async Task<T?> WaitGetData<T>(DataCategory category, Func<CancellationToken, Task> request, Func<JsonObject, TaskCompletionSource<T?>, bool> onDataReceive, CancellationToken cancellationToken)
    {
        // connect token and set timeout.
        using var ctx = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        ctx.CancelAfter(this.timeout);

        var taskSource = new TaskCompletionSource<T?>();

        // Declare the local function handler that sets the task result.
        void Handler(object? sender, SendDataArgs e)
        {
            if (e.Category != category || e.Data is null)
            {
                return;
            }

            var data = e.Data.AsObject();

            if (onDataReceive(data, taskSource))
            {
                this.didReceivedData -= Handler;
            }
        }

        // Register the cancellation.
        ctx.Token.Register(() => { taskSource.TrySetCanceled(); });

        // Listen for receiving events, and trigger a request.
        this.didReceivedData += Handler;
        T? value;
        try
        {
            await request(ctx.Token);
            value = await taskSource.Task;
            ctx.TryReset();
        }
        catch (OperationCanceledException)
        {
            value = default;
        }
        finally
        {
            this.didReceivedData -= Handler;
        }

        return value;
    }

    private Task RequestIcon(uint icon, CancellationToken cancellationToken)
        => this.connection.SendDataRequestAsync(
            new RequestDataArgs
            {
                Category = DataCategory.Icon,
                Id = icon.ToString(CultureInfo.InvariantCulture),
            },
            cancellationToken);
}