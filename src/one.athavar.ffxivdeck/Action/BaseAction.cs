// <copyright file="BaseAction.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace one.athavar.ffxivdeck.Action;

using System.Net.Sockets;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using one.athavar.ffxivdeck.Settings;
using SharpDeck;
using SharpDeck.Enums;
using SharpDeck.Events.Received;

/// <summary>
///     abstract Action base.
/// </summary>
/// <typeparam name="T">Type of the setting.</typeparam>
internal abstract class BaseAction<T> : StreamDeckAction<T>
    where T : BaseSettings, new()
{
    protected readonly ILogger logger;
    protected readonly IDalamudDataManager dataManager;
    protected T? settings;

    public BaseAction(ILogger logger, IDalamudDataManager dataManager)
    {
        this.logger = logger;
        this.dataManager = dataManager;
    }

    /// <inheritdoc />
    protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
    {
        await base.OnKeyDown(args);

        if (!this.dataManager.IsConnected)
        {
            await this.ShowAlertAsync();
        }
    }

    /// <inheritdoc />
    protected override async Task OnSendToPlugin(ActionEventArgs<JObject> args)
    {
        try
        {
            await base.OnSendToPlugin(args);

            if (args.Payload.TryGetValue("piEvent", out var piEvent))
            {
                switch (piEvent.Value<string>())
                {
                    case "valueChanged":
                        this.settings = await this.GetSettingsAsync();
                        if (args.Payload.TryGetValue("value", out var value))
                        {
                            switch (value.ToString())
                            {
                                case "icon":
                                    await this.UpdateImage(CancellationToken.None);
                                    break;
                            }
                        }

                        break;
                }
            }
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Exception occurred in OnSendToPlugin");
        }
    }

    /// <inheritdoc />
    protected override void OnInit(ActionEventArgs<AppearancePayload> args, T baseSettings)
    {
        this.settings = baseSettings;
        _ = this.UpdateImage(CancellationToken.None);
    }

    protected override async Task OnPropertyInspectorDidAppear(ActionEventArgs args)
    {
        await base.OnPropertyInspectorDidAppear(args);

        // start loading in PropertyInspector
        await this.SendToPropertyInspectorAsync(null);
    }

    protected async Task UpdateImage(CancellationToken cancellationToken)
    {
        var iconId = this.settings?.Icon;
        if (iconId is null || iconId == 0)
        {
            return;
        }

        var icon = await this.dataManager.GetIconAsync(iconId.Value, cancellationToken);

        await this.SetImageSafeAsync(icon ?? string.Empty);
    }

    protected async Task SetImageSafeAsync(string base64Image, int? state = null)
    {
        try
        {
            await this.SetImageAsync(base64Image, TargetType.Both, state);
        }
        catch (SocketException)
        {
            // Ignore as we can't really do anything here
        }
        catch (WebSocketException)
        {
            // Ignore as we can't really do anything here
        }
        catch (ObjectDisposedException)
        {
            // Ignore
        }
    }
}