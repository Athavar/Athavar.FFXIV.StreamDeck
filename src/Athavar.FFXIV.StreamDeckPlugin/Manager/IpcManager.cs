namespace Athavar.FFXIV.StreamDeckPlugin.Manager;

using System;
using Dalamud.Logging;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using Microsoft.Extensions.Logging;

/// <summary>
///     Manage inter process communication.
/// </summary>
internal class IpcManager : IIpcManager
{
    private readonly ILogger logger;
    private readonly ICallGateSubscriber<(int Breaking, int Features)> penumbraApiVersionSubscriber;
    private readonly ICallGateSubscriber<string, string>? penumbraResolveDefaultSubscriber;

    /// <summary>
    ///     Initializes a new instance of the <see cref="IpcManager" /> class.
    /// </summary>
    /// <param name="logger"><see cref="ILogger{TCategoryName}" /> added by DI.</param>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    public IpcManager(ILogger<IpcManager> logger, IDalamudServices dalamudServices)
    {
        this.logger = logger;
        this.penumbraApiVersionSubscriber = dalamudServices.PluginInterface.GetIpcSubscriber<(int Breaking, int Features)>("Penumbra.ApiVersions");

        if (!dalamudServices.PluginInterface.PluginNames.Contains("Penumbra") || this.PenumbraApiVersion.Breaking == 4)
        {
            return;
        }

        this.penumbraResolveDefaultSubscriber = dalamudServices.PluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolveDefaultPath");
        this.PenumbraEnabled = true;
    }

    /// <inheritdoc />
    public (int Breaking, int Features) PenumbraApiVersion
    {
        get
        {
            try
            {
                return this.penumbraApiVersionSubscriber.InvokeFunc();
            }
            catch (IpcNotReadyError)
            {
                return (0, 0);
            }
            catch
            {
                return (-1, -1);
            }
        }
    }

    /// <inheritdoc />
    public bool PenumbraEnabled { get; set; }

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
        catch (IpcNotReadyError)
        {
            return path;
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Failed while try to use Penumbra IPC. Disable integration");
            this.PenumbraEnabled = false;
            return path;
        }
    }
}