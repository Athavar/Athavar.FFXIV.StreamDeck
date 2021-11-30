// <copyright file="Plugin.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.StreamDeckPlugin;

using System;
using System.Threading;
using Athavar.Common.Connectivity;
using Athavar.FFXIV.ConnectivityContacts;
using Athavar.FFXIV.StreamDeckPlugin.Manager;
using Athavar.FFXIV.StreamDeckPlugin.Manager.Interface;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
///     Main plugin implementation.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global; Class is the entry point, called by Dalamud.
public class Plugin : IDalamudPlugin
{
    /// <summary>
    ///     prefix of the command.
    /// </summary>
    internal const string CommandName = "/stream";

    /// <summary>
    ///     The Plugin name.
    /// </summary>
    internal const string PluginName = "StreamDeck Integration";

    private readonly IHostLifetime hostLifetime;

    private readonly DalamudPluginInterface pluginInterface;

    private readonly IHost host;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Plugin" /> class.
    /// </summary>
    /// <param name="pluginInterface">Dalamud plugin interface.</param>
    public Plugin(
        [RequiredVersion("1.0")]
        DalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
        this.host = Host.CreateDefaultBuilder().ConfigureLogging(this.ConfigureLogging)
                        .ConfigureServices(this.ConfigureServices)
                        .Build();

        this.hostLifetime = this.host.Services.GetRequiredService<IHostLifetime>();

        this.host.Start();
    }

    /// <inheritdoc />
    public string Name => PluginName;

    /// <inheritdoc />
    public void Dispose()
    {
        var task = this.hostLifetime.StopAsync(CancellationToken.None);
        task.Wait();

        task = this.host.StopAsync(CancellationToken.None);
        task.Wait();

        this.host.Dispose();
    }

    /// <summary>
    ///     Try to catch all exception.
    /// </summary>
    /// <param name="action">Action that can throw exception.</param>
    internal static void CatchCrash(Action action)
    {
        try
        {
            action.Invoke();
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Don't crash the game");
        }
    }

    private void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
    {
        builder.AddDalamudLogger();
        builder.AddDebug();
        builder.Configure(options =>
        {
            options.ActivityTrackingOptions =
                ActivityTrackingOptions.SpanId |
                ActivityTrackingOptions.TraceId |
                ActivityTrackingOptions.ParentId;
        });
        builder.SetMinimumLevel(LogLevel.Debug);
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection service)
    {
        service.AddSingleton(this.pluginInterface);
        service.AddSingleton<IDalamudServices, DalamudServices>();
        service.AddSingleton<StreamDeckAddressResolver>();
        service.AddSingleton<PluginWindow>();
        service.AddSingleton(o =>
        {
            var pi = o.GetRequiredService<IDalamudServices>().PluginInterface;
            var c = (Configuration?)pi.GetPluginConfig() ?? new Configuration();
            c.SetPi(this.pluginInterface);
            return c;
        });

        service.AddSingleton<WebSocketServer>();
        service.AddSingleton<IDalamudSocketEventManager, DalamudSocketEventManager>();

        service.AddSingleton<IChatManager, ChatManager>();
        service.AddSingleton<IIconManager, IconManager>();
        service.AddSingleton<IIpcManager, IpcManager>();

        service.AddHostedService<PluginService>();
    }
}