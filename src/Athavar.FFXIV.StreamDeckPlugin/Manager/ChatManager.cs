﻿// <copyright file="ChatManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.StreamDeckPlugin.Manager;

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using Athavar.FFXIV.StreamDeckPlugin;
using Athavar.FFXIV.StreamDeckPlugin.Helper;
using Athavar.FFXIV.StreamDeckPlugin.Manager.Interface;
using Dalamud.Game;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Manager that handles displaying output in the chat box.
/// </summary>
internal class ChatManager : IDisposable, IChatManager
{
    private readonly Channel<SeString> chatBoxMessages = Channel.CreateUnbounded<SeString>();
    private readonly IDalamudServices dalamud;
    private readonly ProcessChatBoxDelegate processChatBox;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChatManager" /> class.
    /// </summary>
    /// <param name="dalamud"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="addressResolver"><see cref="StreamDeckAddressResolver" /> added by DI.</param>
    public ChatManager(IDalamudServices dalamud, StreamDeckAddressResolver addressResolver)
    {
        this.dalamud = dalamud;

        this.processChatBox =
            Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(addressResolver.SendChatAddress);

        this.dalamud.Framework.Update += this.FrameworkUpdate;
    }

    private unsafe delegate void ProcessChatBoxDelegate(UIModule* uiModule, IntPtr message, IntPtr unused, byte a4);

    /// <inheritdoc />
    public void Dispose()
    {
        this.dalamud.Framework.Update -= this.FrameworkUpdate;
        this.chatBoxMessages.Writer.Complete();
    }

    /// <inheritdoc/>
    public void PrintInformationMessage(string message) => this.dalamud.ChatGui.Print($"[{Plugin.PluginName}] {message}");

    /// <inheritdoc/>
    public void PrintInformationMessage(SeString message)
    {
        message.Payloads.Insert(0, new TextPayload($"[{Plugin.PluginName}] "));
        this.dalamud.ChatGui.Print(message);
    }

    /// <inheritdoc/>
    public void PrintErrorMessage(string message) => this.dalamud.ChatGui.PrintError($"[{Plugin.PluginName}] {message}");

    /// <inheritdoc/>
    public void PrintErrorMessage(SeString message)
    {
        message.Payloads.Insert(0, new TextPayload($"[{Plugin.PluginName}] "));
        this.dalamud.ChatGui.PrintError(message);
    }

    /// <inheritdoc/>
    public async void SendMessage(string message) => await this.chatBoxMessages.Writer.WriteAsync(SeStringHelper.Parse(message));

    /// <inheritdoc/>
    public async void SendMessage(SeString message) => await this.chatBoxMessages.Writer.WriteAsync(message);

    private void FrameworkUpdate(Framework framework)
    {
        if (this.chatBoxMessages.Reader.TryRead(out var message))
        {
            this.SendMessageInternal(message);
        }
    }

    private unsafe void SendMessageInternal(string message)
    {
        var framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance();
        var uiModule = framework->GetUiModule();

        using var payload = new ChatPayload(Encoding.UTF8.GetBytes(message));
        var payloadPtr = Marshal.AllocHGlobal(400);
        Marshal.StructureToPtr(payload, payloadPtr, false);

        this.processChatBox(uiModule, payloadPtr, IntPtr.Zero, 0);

        Marshal.FreeHGlobal(payloadPtr);
    }

    private unsafe void SendMessageInternal(SeString message)
    {
        var messagePayload = message.Encode();
        if (messagePayload.Length > 500)
        {
            this.PrintErrorMessage($"Message exceeds byte size of 500({messagePayload.Length})");
            return;
        }

        var framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance();
        var uiModule = framework->GetUiModule();

        using var chatPayload = new ChatPayload(messagePayload);
        var chatPayloadPtr = Marshal.AllocHGlobal(Marshal.SizeOf<ChatPayload>());
        Marshal.StructureToPtr(chatPayload, chatPayloadPtr, false);

        this.processChatBox(uiModule, chatPayloadPtr, IntPtr.Zero, 0);

        Marshal.FreeHGlobal(chatPayloadPtr);
    }

    [StructLayout(LayoutKind.Explicit)]
    private readonly struct ChatPayload : IDisposable
    {
        [FieldOffset(0)]
        private readonly IntPtr textPtr;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        [FieldOffset(16)]
        private readonly ulong textLen;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        [FieldOffset(8)]
        private readonly ulong unk1;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        [FieldOffset(24)]
        private readonly ulong unk2;

        internal ChatPayload(byte[] payload)
        {
            this.textPtr = Marshal.AllocHGlobal(payload.Length + 30);

            Marshal.Copy(payload, 0, this.textPtr, payload.Length);
            Marshal.WriteByte(this.textPtr + payload.Length, 0);

            this.textLen = (ulong)(payload.Length + 1);

            this.unk1 = 64;
            this.unk2 = 0;
        }

        public void Dispose() => Marshal.FreeHGlobal(this.textPtr);
    }
}