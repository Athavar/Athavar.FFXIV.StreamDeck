// <copyright file="DalamudBinding.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.StreamDeckPlugin;

using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Plugin;

internal interface IDalamudServices
{
    ChatGui ChatGui { get; }

    ChatHandlers ChatHandlers { get; }

    ClientState ClientState { get; }

    SigScanner SigScanner { get; }

    CommandManager CommandManager { get; }

    DataManager DataManager { get; }

    DalamudPluginInterface PluginInterface { get; }

    public Framework Framework { get; }
}