namespace one.athavar.ffxivdeck;

using Athavar.FFXIV.ConnectivityContacts.Dalamud;
using one.athavar.ffxivdeck.Settings;

internal interface IDalamudDataManager
{
    /// <summary>
    ///     Gets a value indicating whether the deck is connected with Dalamud plugin or not.
    /// </summary>
    public bool IsConnected { get; }

    /// <summary>
    ///     Give the <see cref="IDalamudDataManager" /> new data for processing.
    /// </summary>
    /// <param name="args">The new data.</param>
    void ReceiveNewData(SendDataArgs args);

    Task<string?> GetIconAsync(uint icon, CancellationToken cancellationToken);

    Task SendChatMessageAsync(string message, CancellationToken cancellationToken);

    /// <summary>
    ///     Sends request for changing the equipped gear-set to the Dalamud Plugin asynchronously.
    /// </summary>
    /// <param name="settings">The settings containing the gear-set data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task of sending the request.</returns>
    Task ChangeGearSetAsync(GearSetSettings settings, CancellationToken cancellationToken);
}