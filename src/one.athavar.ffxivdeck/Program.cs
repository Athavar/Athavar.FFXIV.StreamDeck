// <copyright file="Program.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace one.athavar.ffxivdeck;

using Athavar.FFXIV.ConnectivityContacts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using one.athavar.ffxivdeck.Connectivity;
using one.athavar.ffxivdeck.Services;

/// <summary>
///     The plugin.
/// </summary>
public class Program
{
    /// <summary>
    ///     Defines the entry point of the application.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public static Task Main(string[] args)
    {
#if DEBUG
         // Debugger.Launch();
#endif

        try
        {
            return new HostBuilder().ConfigureServices(ConfigureServices).ConfigureLogging(ConfigureLogging).Build().RunAsync();
        }
        catch
        {
        }

        return Task.CompletedTask;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDalamudSocketEventManager, DalamudSocketEventManager>();
        services.AddSingleton<IDalamudWebSocketConnection, DalamudWebSocketConnection>();
        services.AddSingleton<IDalamudDataManager, DalamudDataManager>();
        services.AddHostedService<PluginService>();
    }

    private static void ConfigureLogging(ILoggingBuilder builder)
    {
        builder.AddConsole();
        builder.AddDebug();
        builder.Configure(options =>
        {
            options.ActivityTrackingOptions =
                ActivityTrackingOptions.SpanId |
                ActivityTrackingOptions.TraceId |
                ActivityTrackingOptions.ParentId;
        });
    }
}