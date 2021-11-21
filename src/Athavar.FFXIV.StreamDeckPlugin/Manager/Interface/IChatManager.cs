// <copyright file="IChatManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.StreamDeckPlugin.Manager.Interface;

/// <summary>
///     Print messages to the in-game chat.
/// </summary>
internal interface IChatManager
{
    /// <summary>
    ///     Print a normal message.
    /// </summary>
    /// <param name="message">The message to print.</param>
    void PrintMessage(string message);

    /// <summary>
    ///     Print an error message.
    /// </summary>
    /// <param name="message">The message to print.</param>
    void PrintError(string message);

    /// <summary>
    ///     Process a command through the chat box.
    /// </summary>
    /// <param name="message">Message to send.</param>
    void SendMessage(string message);
}