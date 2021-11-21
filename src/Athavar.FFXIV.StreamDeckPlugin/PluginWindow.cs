// <copyright file="PluginWindow.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.StreamDeckPlugin;

using System;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Reflection;
using Athavar.Common.Connectivity;
using Athavar.FFXIV.StreamDeckPlugin.Manager.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;

/// <summary>
///     UI Window of the Plugin.
/// </summary>
internal class PluginWindow : Window
{
    private readonly IIconManager iconManager;
    private readonly Configuration configuration;
    private readonly WebSocketServer socketServer;

    private int iconId;
    private string iconOutputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginWindow" /> class.
    /// </summary>
    /// <param name="iconManager"><see cref="IIconManager" /> added by DI.</param>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="socketServer"><see cref="WebSocketServer" /> added by DI.</param>
    public PluginWindow(IIconManager iconManager, Configuration configuration, WebSocketServer socketServer)
        : base($"{Plugin.PluginName} {Assembly.GetCallingAssembly().GetName().Version}")
    {
        this.iconManager = iconManager;
        this.configuration = configuration;
        this.socketServer = socketServer;

        this.Size = new Vector2(525, 600);
        this.SizeCondition = ImGuiCond.FirstUseEver;
        this.RespectCloseHotkey = true;

#if DEBUG
        this.Toggle();
#endif
    }

    /// <inheritdoc />
    public override void PreDraw()
    {
        ImGui.PushStyleColor(ImGuiCol.ResizeGrip, 0);
    }

    /// <inheritdoc />
    public override void PostDraw()
    {
        ImGui.PopStyleColor();
    }

    /// <inheritdoc />
    public override void Draw()
    {
        Plugin.CatchCrash(() =>
        {
            var change = false;

            using (var group = ImGuiRaii.NewGroup())
            {
                // Port setting
                ImGui.TextUnformatted("Prefix:");
                ImGui.AlignTextToFramePadding();
                ImGui.SameLine();
                var value = this.configuration.Prefix;

                if (ImGui.InputText("##prefix", ref value, 100, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    this.configuration.Prefix = value;
                    this.socketServer.Prefix = value;

                    change = true;
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Change the Port of the WebSockets.");
                }

                if (ImGui.Button(this.socketServer.IsStarted ? "Stop" : "Start"))
                {
                    if (this.socketServer.IsStarted)
                    {
                        this.socketServer.StopListening();
                    }
                    else
                    {
                        this.socketServer.StartListening();
                    }
                }
            }

            ImGui.Spacing();

            using (var group = ImGuiRaii.NewGroup())
            {
                ImGui.TextUnformatted("IconId:");
                ImGui.AlignTextToFramePadding();
                ImGui.SameLine();
                ImGui.InputInt("##icon", ref this.iconId);

                ImGui.TextUnformatted("Output Folder:");
                ImGui.AlignTextToFramePadding();
                ImGui.SameLine();
                ImGui.InputText("##iconOutPutDirectory", ref this.iconOutputDirectory, 256, ImGuiInputTextFlags.EnterReturnsTrue);

                if (ImGui.Button("Save icon as file."))
                {
                    this.iconManager.SaveAsFile((uint)this.iconId, true, new FileInfo(Path.Combine(this.iconOutputDirectory, $"icon{this.iconId}.png")), ImageFormat.Jpeg);
                }
            }

            if (change)
            {
                this.configuration.Save();
            }

            ImGui.End();
        });
    }
}