namespace one.athavar.ffxivdeck.Action;

using Microsoft.Extensions.Logging;
using one.athavar.ffxivdeck.Settings;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;

/// <summary>
///     Action to change gear set in ffxiv.
/// </summary>
[StreamDeckAction("GearSet", "one.athavar.ffxivdeck.gearset")]
internal class GearSetAction : BaseAction<GearSetSettings>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GearSetAction" /> class.
    /// </summary>
    /// <param name="logger"><see cref="ILogger{TCategoryName}" /> added by DI.</param>
    /// <param name="dataManager"><see cref="IDalamudDataManager" /> added by DI.</param>
    public GearSetAction(ILogger<GearSetAction> logger, IDalamudDataManager dataManager)
        : base(logger, dataManager)
    {
    }

    /// <inheritdoc />
    protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
    {
        await this.OnKeyUp(args);

        if (this.settings is null)
        {
            return;
        }

        await this.dataManager.ChangeGearSetAsync(this.settings, CancellationToken.None);
    }
}