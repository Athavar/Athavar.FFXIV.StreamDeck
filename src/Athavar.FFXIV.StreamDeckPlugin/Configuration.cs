// <copyright file="Configuration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.StreamDeckPlugin;

using System;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Newtonsoft.Json;

/// <summary>
///     Configuration of the plugin.
/// </summary>
internal class Configuration : IPluginConfiguration, IDisposable
{
    [JsonIgnore]
    private DalamudPluginInterface? pi;

    /// <summary>
    ///     Gets or sets the port of the WebSocket server.
    /// </summary>
    public string Prefix { get; set; } = "http://127.0.0.1:50605/";

    /// <inheritdoc />
    public int Version { get; set; } = 1;

    /// <inheritdoc />
    public void Dispose()
    {
        this.Save();
    }

    /// <summary>
    ///     Setup <see cref="DalamudPluginInterface" />.
    /// </summary>
    /// <param name="interface">The <see cref="DalamudPluginInterface" />.</param>
    internal void SetPi(DalamudPluginInterface @interface)
    {
        this.pi = @interface;
    }

    /// <summary>
    ///     Save the configuration.
    /// </summary>
    internal void Save()
    {
        this.pi?.SavePluginConfig(this);
    }
}