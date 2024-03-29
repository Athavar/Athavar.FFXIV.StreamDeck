﻿// <copyright file="DalamudServices.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.StreamDeckPlugin;

using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;

/// <summary>
///     Contains services from dalamud.
/// </summary>
internal class DalamudServices : IDalamudServices
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DalamudServices" /> class.
    /// </summary>
    /// <param name="pluginInterface"><see cref="DalamudPluginInterface" /> used to inject the other values.</param>
    public DalamudServices(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Inject(this);
    }

    /// <inheritdoc />
    [PluginService]
    [RequiredVersion("1.0")]
    public SigScanner SigScanner { get; init; } = null!;

    /// <inheritdoc />
    [PluginService]
    [RequiredVersion("1.0")]
    public DalamudPluginInterface PluginInterface { get; init; } = null!;

    /// <inheritdoc />
    [PluginService]
    [RequiredVersion("1.0")]
    public CommandManager CommandManager { get; init; } = null!;

    /// <inheritdoc />
    [PluginService]
    [RequiredVersion("1.0")]
    public DataManager DataManager { get; init; } = null!;

    /// <inheritdoc />
    [PluginService]
    [RequiredVersion("1.0")]
    public ClientState ClientState { get; init; } = null!;

    /// <inheritdoc />
    [PluginService]
    [RequiredVersion("1.0")]
    public ChatGui ChatGui { get; init; } = null!;

    /// <inheritdoc />
    [PluginService]
    [RequiredVersion("1.0")]
    public ChatHandlers ChatHandlers { get; init; } = null!;

    /// <inheritdoc />
    [PluginService]
    [RequiredVersion("1.0")]
    public Framework Framework { get; init; } = null!;

    /*[PluginService]
    [RequiredVersion("1.0")]
    public GameNetwork GameNetwork { get; private set; } = null!;*/

    /*[PluginService]
    [RequiredVersion("1.0")]
    public Condition Condition { get; private set; } = null!;*/

    /*[PluginService]
    [RequiredVersion("1.0")]
    public KeyState KeyState { get; private set; } = null!;*/

    /*[PluginService]
    [RequiredVersion("1.0")]
    public GameGui GameGui { get; private set; } = null!;*/

    /*[PluginService]
    [RequiredVersion("1.0")]
    public FlyTextGui FlyTexts { get; private set; } = null!;*/

    /*[PluginService]
    [RequiredVersion("1.0")]
    public ToastGui ToastGui { get; private set; } = null!;*/

    /*[PluginService]
    [RequiredVersion("1.0")]
    public JobGauges Gauges { get; private set; } = null!;*/

    /*[PluginService]
    [RequiredVersion("1.0")]
    public PartyFinderGui PartyFinder { get; private set; } = null!;*/

    /*[PluginService]
    [RequiredVersion("1.0")]
    public BuddyList Buddies { get; private set; } = null!;*/

    /*[PluginService]
    [RequiredVersion("1.0")]
    public PartyList PartyList { get; private set; } = null!;*/

    /*[PluginService]
    [RequiredVersion("1.0")]
    public TargetManager TargetManager { get; private set; } = null!;*/

    /*[PluginService]
    [RequiredVersion("1.0")]
    public ObjectTable ObjectTable { get; private set; } = null!;*/

    /*[PluginService]
    [RequiredVersion("1.0")]
    public FateTable FateTable { get; private set; } = null!;*/

    /*[PluginService]
    [RequiredVersion("1.0")]
    public LibcFunction LibC { get; private set; } = null!;*/
}