﻿// <copyright file="WebSocketConnection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.Common.Connectivity;

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

/// <summary>
///     Provides a light-weight wrapper for <see cref="ClientWebSocket" />.
/// </summary>
public class WebSocketConnection : IDisposable
{
    internal const string SubProtocol = "json";

    /// <summary>
    ///     The buffer size.
    /// </summary>
    private const int BUFFER_SIZE = 1024 * 1024;

    /// <summary>
    ///     The process synchronize root.
    /// </summary>
    private readonly SemaphoreSlim _syncRoot = new(1);

    /// <summary>
    ///     Receive process for incomming data ist started.
    /// </summary>
    private CancellationTokenSource? ctx;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebSocketConnection" /> class.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <param name="uri">The Origin.</param>
    /// <param name="jsonSettings">The JSON settings.</param>
    public WebSocketConnection(WebSocket socket, string origin, JsonSerializerOptions? jsonSettings = null)
    {
        this.WebSocket = socket;
        this.Origin = origin;
        this.JsonSettings = jsonSettings;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebSocketConnection" /> class.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <param name="jsonSettings">The JSON settings.</param>
    public WebSocketConnection(string uri, JsonSerializerOptions? jsonSettings = null)
    {
        this.JsonSettings = jsonSettings;
        this.Uri = new Uri(uri);
    }

    /// <summary>
    ///     Occurs when a message is received.
    /// </summary>
    public event EventHandler<WebSocketMessageEventArgs>? MessageReceived;

    /// <summary>
    ///     Gets the JSON settings.
    /// </summary>
    public JsonSerializerOptions? JsonSettings { get; }

    /// <summary>
    ///     Gets the URI.
    /// </summary>
    public Uri? Uri { get; }

    /// <summary>
    ///     Gets the Origin from the Header.
    /// </summary>
    public string? Origin { get; }

    /// <summary>
    ///     Gets the Origin from the Header.
    /// </summary>
    public bool IsConnected => this.WebSocket?.State == WebSocketState.Open;

    /// <summary>
    ///     Gets or sets the encoding.
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>
    ///     Gets the connection task completion source.
    /// </summary>
    private TaskCompletionSource<bool> ConnectionTaskCompletionSource { get; } = new();

    /// <summary>
    ///     Gets or sets the web socket.
    /// </summary>
    private WebSocket? WebSocket { get; set; }

    /// <summary>
    ///     Connects the web socket.
    /// </summary>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (this.Uri is null)
        {
            throw new ArgumentNullException(nameof(this.Uri));
        }

        if (this.WebSocket is null)
        {
            // Connect the web socket.
            ClientWebSocket webSocket;
            this.WebSocket = webSocket = new ClientWebSocket();
            webSocket.Options.AddSubProtocol(SubProtocol);

            await webSocket.ConnectAsync(this.Uri, cancellationToken);

            // Asynchronously listen for messages.
            this.Start();
        }
    }

    /// <summary>
    ///     Start Asynchronously listen of incomming data on the web socket.
    /// </summary>
    public void Start()
    {
        if (this.ctx is not null)
        {
            return;
        }

        this.ctx = new CancellationTokenSource();
        var cancellationToken = this.ctx.Token;

        // Asynchronously listen for messages.
        _ = Task.Factory.StartNew(async () =>
            {
                await this.ReceiveAsync(cancellationToken);
                this.ConnectionTaskCompletionSource.TrySetResult(true);
            },
            cancellationToken,
            TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously,
            TaskScheduler.Default);
    }

    /// <summary>
    ///     Disconnects the web socket.
    /// </summary>
    public async Task DisconnectAsync()
    {
        this.ctx?.Cancel();

        if (this.WebSocket != null)
        {
            var socket = this.WebSocket;
            this.WebSocket = null;

            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
            }

            socket.Dispose();
            this.ConnectionTaskCompletionSource?.TrySetResult(true);
        }

        this.ctx?.Dispose();
    }

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _ = this.DisconnectAsync();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Sends the specified message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SendAsync(string message, CancellationToken cancellationToken)
    {
        if (this.WebSocket == null)
        {
            return;
        }

        try
        {
            await this._syncRoot.WaitAsync();

            var buffer = this.Encoding.GetBytes(message);
            await this.WebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationToken);
        }
        finally
        {
            this._syncRoot.Release();
        }
    }

    /// <summary>
    ///     Serializes the value, and sends the message asynchronously.
    /// </summary>
    /// <param name="value">The value to serialize and send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task SendJsonAsync<T>(T value, CancellationToken cancellationToken)
        where T : class
    {
        if (this.WebSocket is null || this.WebSocket.State != WebSocketState.Open)
        {
            return Task.CompletedTask;
        }

        var json = JsonSerializer.Serialize(value, this.JsonSettings);
        return this.SendAsync(json, cancellationToken);
    }

    /// <summary>
    ///     Waits the underlying connection to disconnect asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task of the live connection.</returns>
    public Task WaitDisconnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => this.ConnectionTaskCompletionSource.TrySetCanceled(), false);
        return this.ConnectionTaskCompletionSource.Task;
    }

    /// <summary>
    ///     Receive data as an asynchronous operation.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task<WebSocketCloseStatus> ReceiveAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[BUFFER_SIZE];
        var textBuffer = new StringBuilder(BUFFER_SIZE);

        try
        {
            while (this.WebSocket?.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                // await a message
                var result = await this.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result == null)
                {
                    continue;
                }

                if (result.MessageType == WebSocketMessageType.Close || result.CloseStatus != null && result.CloseStatus.HasValue && result.CloseStatus.Value != WebSocketCloseStatus.Empty)
                {
                    // Stop listening, and return the close status.
                    return result.CloseStatus.GetValueOrDefault();
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Append to the text buffer, and determine if the message has finished
                    textBuffer.Append(this.Encoding.GetString(buffer, 0, result.Count));
                    if (result.EndOfMessage)
                    {
                        this.MessageReceived?.Invoke(this, new WebSocketMessageEventArgs(textBuffer.ToString()));
                        textBuffer.Clear();
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            return WebSocketCloseStatus.NormalClosure;
        }
        catch (Exception)
        {
            return WebSocketCloseStatus.InternalServerError;
        }

        return WebSocketCloseStatus.NormalClosure;
    }
}