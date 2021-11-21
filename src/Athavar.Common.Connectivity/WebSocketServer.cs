// <copyright file="WebSocketServer.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.Common.Connectivity;

using System.Net;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;

public class WebSocketServer : IDisposable
{
    private readonly ILogger? logger;
    private readonly HttpListener httpListener;
    private string prefix = string.Empty;

    private CancellationTokenSource? ctx;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebSocketServer" /> class.
    /// </summary>
    public WebSocketServer() => this.httpListener = new HttpListener();

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebSocketServer" /> class.
    /// </summary>
    public WebSocketServer(ILogger<WebSocketServer> logger)
        : this() =>
        this.logger = logger;

    /// <summary>
    ///     Occurs when a message is received.
    /// </summary>
    public event EventHandler<WebSocketMessageEventArgs>? MessageReceived;

    /// <summary>
    ///     Occurs when a new client connected or a client close the connection.
    /// </summary>
    public event EventHandler<WebSocketClientEventArgs>? ClientStatus;

    /// <summary>
    ///     Gets a value indication if the server is listen on a port or not.
    /// </summary>
    public bool IsStarted => this.httpListener.IsListening;

    public string Prefix
    {
        get => this.prefix;
        set
        {
            this.prefix = value;
            this.httpListener.Prefixes.Clear();
            this.httpListener.Prefixes.Add(this.Prefix);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.ctx?.Cancel();

        if (this.httpListener.IsListening)
        {
            this.StopListening();
        }

        this.ctx?.Dispose();

        ((IDisposable)this.httpListener).Dispose();
    }

    /// <summary>
    ///     Start the <see cref="WebSocketServer" />.
    /// </summary>
    public void StartListening()
    {
        if (this.httpListener.IsListening)
        {
            return;
        }

        this.logger?.LogDebug("Start Listening");
        this.httpListener.Start();
        _ = this.ListenConnection();
    }

    public void StopListening()
    {
        if (!this.httpListener.IsListening)
        {
            return;
        }

        this.logger?.LogDebug("Stop Listening");
        this.httpListener.Stop();
        this.ctx?.Cancel();
    }

    private async Task ListenConnection()
    {
        using var ctx = new CancellationTokenSource();
        this.ctx = ctx;
        try
        {
            while (this.httpListener.IsListening)
            {
                var context = await this.httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var webSocketContext = await context.AcceptWebSocketAsync(WebSocketConnection.SubProtocol);

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var webSocket = webSocketContext.WebSocket;

                            using WebSocketConnection webSocketConnection = new(webSocket, webSocketContext.Origin);
                            webSocketConnection.MessageReceived += this.HandlePackage;

                            webSocketConnection.Start();
                            this.ClientStatus?.Invoke(this, new WebSocketClientEventArgs(ConnectionState.New, webSocketConnection));

                            await webSocketConnection.WaitDisconnectAsync(ctx.Token);
                            this.ClientStatus?.Invoke(this, new WebSocketClientEventArgs(ConnectionState.Close, webSocketConnection));

                            webSocketConnection.MessageReceived -= this.HandlePackage;
                        }
                        catch (WebSocketException)
                        {
                        }
                    });
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
        }
        catch (HttpListenerException)
        {
        }

        this.ctx = null;
    }

    private void HandlePackage(object? connection, WebSocketMessageEventArgs package)
    {
        if (connection is not WebSocketConnection webSocketConnection)
        {
            return;
        }

        this.MessageReceived?.Invoke(connection, package);
    }
}