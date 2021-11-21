// <copyright file="ChatSettings.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace one.athavar.ffxivdeck.Settings;

using System.ComponentModel;
using Newtonsoft.Json;
using one.athavar.ffxivdeck.Action;

/// <summary>
///     Settings for <see cref="ChatAction" />.
/// </summary>
internal class ChatSettings : BaseSettings
{
    /// <summary>
    ///     Gets or sets the chat message.
    /// </summary>
    [JsonProperty("message")]
    [DefaultValue("")]
    internal string Message { get; set; } = string.Empty;
}