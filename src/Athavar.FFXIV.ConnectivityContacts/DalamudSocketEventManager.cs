// <copyright file="DalamudSocketEventManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.ConnectivityContacts;

using System.Reflection;
using System.Text;
using System.Text.Json;
using Athavar.Common.Connectivity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class DalamudSocketEventManager : IDalamudSocketEventManager
{
    private readonly TimeSpan timeout = TimeSpan.FromSeconds(60);
    private readonly ILogger logger;
    private readonly IServiceProvider serviceProvider;

    public DalamudSocketEventManager(ILogger<DalamudSocketEventManager> logger, IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
        this.Events = new Dictionary<string, DalamudSocketEventHandler>();
    }

    private Dictionary<string, DalamudSocketEventHandler> Events { get; }

    public void LoadEvents(Assembly assembly) =>
        this.PopulateEvents(assembly);

    public void AttachEvent(Action<EventHandler<WebSocketMessageEventArgs>> @event)
    {
        @event(this.OnMessageReceived);
    }

    private async void OnMessageReceived(object? sender, WebSocketMessageEventArgs args)
    {
        try
        {
            if (sender is not WebSocketConnection connection)
            {
                return;
            }

            this.logger.LogDebug("OnMessageReceived {Message}", args.Message);

            JsonElement ParseMessage(string message)
            {
                Utf8JsonReader jsonReader = new(Encoding.UTF8.GetBytes(message));

                // attempt to parse the original message
#if NET6_0_OR_GREATER
            if (!JsonElement.TryParseValue(ref jsonReader, out var element) || element.Value.ValueKind != JsonValueKind.Object)
#else
                if (!JsonExtension.TryParseValue(ref jsonReader, out var element) || element.Value.ValueKind != JsonValueKind.Object)
#endif
                {
                    throw new ArgumentException("Unable to parse Json Message.");
                }

                return element.Value;
            }

            var jsonObject = ParseMessage(args.Message);
            var eventErgs = jsonObject.ToObject<DalamudSocketEventArgs>();

            if (eventErgs?.Event is null)
            {
                throw new ArgumentException("Unable to parse public event from message");
            }

            if (this.Events.TryGetValue(eventErgs.Event.ToLowerInvariant(), out var handler))
            {
                using CancellationTokenSource ctx = new();
                ctx.CancelAfter(this.timeout);
                await handler.HandleEventAsync(connection, jsonObject, ctx.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException ex)
        {
            this.logger.LogError(ex, "Timeout during OnMessageReceived");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during OnMessageReceived");
        }
    }

    private void PopulateEvents(Assembly eventAssembly)
    {
        var baseType = typeof(DalamudSocketEventHandler);
        var eventTypes = eventAssembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(baseType));

        foreach (var eventType in eventTypes)
        {
            this.logger.LogDebug("Activate Event {EventName}", eventType.Name);
            var instance = ActivatorUtilities.CreateInstance(this.serviceProvider, eventType);

            if (instance is DalamudSocketEventHandler eventInstance)
            {
                this.Events.TryAdd(eventInstance.EventName.ToLowerInvariant(), eventInstance);
            }
        }
    }
}