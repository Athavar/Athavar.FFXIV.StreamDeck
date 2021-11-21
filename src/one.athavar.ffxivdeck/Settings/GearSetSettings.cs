namespace one.athavar.ffxivdeck.Settings;

using System.ComponentModel;
using Newtonsoft.Json;
using one.athavar.ffxivdeck.Action;

/// <summary>
///     Settings for <see cref="GearSetAction" />.
/// </summary>
internal class GearSetSettings : BaseSettings
{
    /// <summary>
    ///     Gets or sets the slot number or name.
    /// </summary>
    [JsonProperty("slot")]
    [DefaultValue("1")]
    internal string GearSlot { get; set; } = "1";

    /// <summary>
    ///     Gets or sets the glamour plate number.
    /// </summary>
    [JsonProperty("glamourPlate")]
    [DefaultValue(0)]
    internal int GlamourPlate { get; set; }
}