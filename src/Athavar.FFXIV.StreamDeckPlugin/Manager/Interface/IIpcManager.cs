namespace Athavar.FFXIV.StreamDeckPlugin.Manager;

internal interface IIpcManager
{
    /// <summary>
    ///     Gets the penumbra api version.
    /// </summary>
    (int Breaking, int Features) PenumbraApiVersion { get; }

    /// <summary>
    ///     Gets a value indicating whether penumbra is enabled or not.
    /// </summary>
    bool PenumbraEnabled { get; }

    /// <summary>
    ///     Resolves a game file path with penumbra.
    /// </summary>
    /// <param name="path">THe path.</param>
    /// <returns>returns the resolved path.</returns>
    string ResolvePenumbraPath(string path);
}