// <copyright file="PluginService.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.StreamDeckPlugin;

using System.Threading;
using System.Threading.Tasks;
using Athavar.Common.Connectivity;
using Athavar.FFXIV.ConnectivityContacts;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
///     Plugin Service.
/// </summary>
internal class PluginService : IHostedService
{
    private readonly StreamDeckAddressResolver addressResolver;
    private readonly IDalamudServices binding;
    private readonly Configuration configuration;
    private readonly IDalamudSocketEventManager eventManager;
    private readonly ILogger logger;
    private readonly PluginWindow pluginWindow;
    private readonly WebSocketServer webSocketServer;

    private readonly WindowSystem windowSystem;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginService" /> class.
    /// </summary>
    /// <param name="logger"><see cref="ILogger{TCategoryName}" /> added by DI.</param>
    /// <param name="appLifetime"><see cref="IHostApplicationLifetime" /> added by DI.</param>
    /// <param name="binding"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="addressResolver"><see cref="StreamDeckAddressResolver" /> added by DI.</param>
    /// <param name="pluginWindow"><see cref="PluginWindow" /> added by DI.</param>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="webSocketServer"><see cref="WebSocketServer" /> added by DI.</param>
    /// <param name="eventManager"><see cref="IDalamudSocketEventManager" /> added by DI.</param>
    public PluginService(
        ILogger<PluginService> logger,
        IHostApplicationLifetime appLifetime,
        IDalamudServices binding,
        StreamDeckAddressResolver addressResolver,
        PluginWindow pluginWindow,
        Configuration configuration,
        WebSocketServer webSocketServer,
        IDalamudSocketEventManager eventManager)
    {
        this.logger = logger;
        this.binding = binding;
        this.addressResolver = addressResolver;
        this.pluginWindow = pluginWindow;
        this.configuration = configuration;
        this.webSocketServer = webSocketServer;
        this.eventManager = eventManager;

        appLifetime.ApplicationStarted.Register(this.OnStarted);
        appLifetime.ApplicationStopping.Register(this.OnStopping);
        appLifetime.ApplicationStopped.Register(this.OnStopped);

        this.windowSystem = new WindowSystem("Athavar's StreamDeck integration");
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // 1.
        this.logger.LogDebug("Service Start");
        this.windowSystem.AddWindow(this.pluginWindow);
        this.addressResolver.Setup(this.binding.SigScanner);

        this.eventManager.LoadEvents(this.GetType().Assembly);

        this.binding.CommandManager.AddHandler(Plugin.CommandName, new CommandInfo(this.OnCommand)
                                                                   {
                                                                       HelpMessage =
                                                                           "Open the Configuration of Athavar's StreamDeck integration.",
                                                                   });

        this.webSocketServer.Prefix = this.configuration.Prefix;

        var dal = this.binding as DalamudServices;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        // 4.
        this.logger.LogDebug("Service Stop");

        return Task.CompletedTask;
    }

    private void OnStarted()
    {
        // 2.
        this.logger.LogDebug("Service Started");
        this.binding.PluginInterface.UiBuilder.Draw += this.windowSystem.Draw;
        this.binding.PluginInterface.UiBuilder.OpenConfigUi += this.OnOpenConfigUi;

        this.eventManager.AttachEvent(e => this.webSocketServer.MessageReceived += e);
        this.webSocketServer.ClientStatus += this.OnClientStatusChange;

        this.webSocketServer.StartListening();
    }

    private void OnStopping()
    {
        // 3.
        this.logger.LogDebug("Service Stopping");
        this.webSocketServer.StopListening();
        this.eventManager.AttachEvent(e => this.webSocketServer.MessageReceived -= e);

        this.binding.CommandManager.RemoveHandler(Plugin.CommandName);
        this.binding.PluginInterface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUi;
        this.binding.PluginInterface.UiBuilder.Draw -= this.windowSystem.Draw;
    }

    private void OnStopped()
    {
        // 5.
        this.logger.LogDebug("Service Stopped");

        // remove all remaining windows.
        this.windowSystem.RemoveAllWindows();
    }

    private void OnClientStatusChange(object? sender, WebSocketClientEventArgs e)
    {
        if (sender is not WebSocketServer)
        {
            return;
        }

        this.logger.LogDebug("WebSocketClient status change. state: {State}, origin: {Origin} ", e.State, e.Client.Origin ?? string.Empty);
    }

    private void OnOpenConfigUi()
    {
        this.pluginWindow.Toggle();
    }

    private void OnCommand(string command, string args)
    {
        this.pluginWindow.Toggle();
    }
}