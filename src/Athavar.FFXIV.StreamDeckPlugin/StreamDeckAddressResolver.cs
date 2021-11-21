// <copyright file="StreamDeckAddressResolver.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.StreamDeckPlugin;

using System;
using Dalamud.Game;

/// <summary>
///     Stream Deck address resolver.
/// </summary>
internal class StreamDeckAddressResolver : BaseAddressResolver
{
    private const string SendChatSignature = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9";

    /// <summary>
    ///     Gets the address of the SelectYesNo addon's OnSetup method.
    /// </summary>
    public IntPtr SendChatAddress { get; private set; }

    /// <inheritdoc />
    protected override void Setup64Bit(SigScanner scanner)
    {
        this.SendChatAddress = scanner.ScanText(SendChatSignature);
    }
}