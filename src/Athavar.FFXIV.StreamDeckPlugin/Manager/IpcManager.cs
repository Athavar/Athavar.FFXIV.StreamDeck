namespace Athavar.FFXIV.StreamDeckPlugin.Manager;

using Dalamud.Plugin.Ipc;
using Microsoft.Extensions.Logging;

/// <summary>
///     Manage inter process communication.
/// </summary>
internal class IpcManager : IIpcManager
{
    private readonly ILogger logger;
    private readonly ICallGateSubscriber<int> penumbraApiVersionSubscriber;
    private readonly ICallGateSubscriber<string, string>? penumbraResolveDefaultSubscriber;

    /// <summary>
    ///     Initializes a new instance of the <see cref="IpcManager" /> class.
    /// </summary>
    /// <param name="logger"><see cref="ILogger{TCategoryName}" /> added by DI.</param>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    public IpcManager(ILogger<IpcManager> logger, IDalamudServices dalamudServices)
    {
        this.logger = logger;
        this.penumbraApiVersionSubscriber = dalamudServices.PluginInterface.GetIpcSubscriber<int>("Penumbra.ApiVersion");

        if (this.PenumbraApiVersion == 3)
        {
            this.penumbraResolveDefaultSubscriber = dalamudServices.PluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolveDefaultPath");
            this.PenumbraEnabled = true;
        }
    }

    /// <inheritdoc />
    public int PenumbraApiVersion
    {
        get
        {
            try
            {
                return this.penumbraApiVersionSubscriber.InvokeFunc();
            }
            catch
            {
                return 0;
            }
        }
    }

    /// <inheritdoc />
    public bool PenumbraEnabled { get; }

    /// <inheritdoc />
    public string ResolvePenumbraPath(string path)
    {
        if (!this.PenumbraEnabled || this.penumbraResolveDefaultSubscriber is null)
        {
            return path;
        }

        try
        {
            var resolved = this.penumbraResolveDefaultSubscriber.InvokeFunc(path);
            if (!resolved.Equals(path))
            {
                this.logger.LogDebug("Penumbra Path resolver: {Path} -> {Resolved}", path, resolved);
            }

            return resolved;
        }
        catch
        {
            return path;
        }
    }
}