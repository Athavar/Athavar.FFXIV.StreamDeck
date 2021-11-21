// <copyright file="BaseSettings.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace one.athavar.ffxivdeck.Settings;

using System.ComponentModel;
using Newtonsoft.Json;

internal abstract class BaseSettings
{
    [JsonProperty("icon")]
    [DefaultValue(0)]
    public uint Icon { get; set; }
}