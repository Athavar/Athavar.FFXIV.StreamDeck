// <copyright file="StartService.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace one.athavar.ffxivdeck.Services;

using System.Reflection;
using Athavar.FFXIV.ConnectivityContacts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using one.athavar.ffxivdeck.Connectivity;
using SharpDeck;

internal class PluginService : IHostedService
{
    private readonly ILogger logger;
    private readonly IServiceProvider serviceProvider;
    private readonly IDalamudSocketEventManager eventManager;
    private readonly IDalamudWebSocketConnection connection;

    private readonly CancellationTokenSource CancellationTokenSource;

    public PluginService(ILogger<PluginService> logger, IHostApplicationLifetime appLifetime, IServiceProvider serviceProvider, IDalamudSocketEventManager eventManager, IDalamudWebSocketConnection connection)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
        this.eventManager = eventManager;
        this.connection = connection;

        this.CancellationTokenSource = new CancellationTokenSource();

        appLifetime.ApplicationStarted.Register(this.OnStarted);
        appLifetime.ApplicationStopping.Register(this.OnStopping);
        appLifetime.ApplicationStopped.Register(this.OnStopped);
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.connection.Port = 50605;

        var args = Environment.GetCommandLineArgs();
        this.logger.LogInformation("Initialize with args: {Args}", string.Join("|", args));

        var plugin = StreamDeckPlugin.Create(args[1..], Assembly.GetAssembly(this.GetType())).WithServiceProvider(this.serviceProvider);
        plugin.OnRegistered(a => a.WillAppear += (sender, eventArgs) => { Console.WriteLine("Test"); });
        _ = Task.Run(async () =>
        {
            await plugin.RunAsync(CancellationToken.None); // continuously listens until the connection closes
        });

        this.eventManager.LoadEvents(this.GetType().Assembly);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void OnStarted()
    {
        _ = this.ConnectionDalamudWebSocket();
    }

    private void OnStopping()
    {
        this.CancellationTokenSource.Cancel();
    }

    private void OnStopped()
    {
        this.CancellationTokenSource.Dispose();
    }

    private async Task ConnectionDalamudWebSocket()
    {
        while (!this.CancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                await this.connection.ConnectAsync(this.CancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
            }

            await Task.Delay(1000);
        }
    }
}