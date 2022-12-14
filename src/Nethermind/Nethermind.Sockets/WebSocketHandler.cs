// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Nethermind.Logging;

namespace Nethermind.Sockets
{
    public class WebSocketHandler : ISocketHandler
    {
        private readonly WebSocket _webSocket;
        private readonly ILogger _logger;

        public WebSocketHandler(WebSocket webSocket, ILogManager logManager)
        {
            _webSocket = webSocket;
            _logger = logManager?.GetClassLogger() ?? throw new ArgumentNullException(nameof(logManager));
        }

        public ValueTask SendRawAsync(Memory<byte> data) =>
            _webSocket.State != WebSocketState.Open
                ? ValueTask.CompletedTask
                : _webSocket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);

        public async ValueTask<ReceiveResult> GetReceiveResult(Memory<byte> buffer)
        {
            ReceiveResult result = default;
            if (_webSocket.State == WebSocketState.Open)
            {
                ValueTask<ValueWebSocketReceiveResult> resultTask = _webSocket.ReceiveAsync(buffer, CancellationToken.None);
                try
                {
                    ValueWebSocketReceiveResult t = await resultTask;
                    result = new ReceiveResult
                    (
                        read: t.Count,
                        endOfMessage: t.EndOfMessage,
                        closed: t.MessageType == WebSocketMessageType.Close
                    );
                }
                catch (Exception innerException)
                {
                    while (innerException?.InnerException is not null)
                    {
                        innerException = innerException.InnerException;
                    }

                    if (innerException is SocketException socketException)
                    {
                        if (socketException.SocketErrorCode == SocketError.ConnectionReset)
                        {
                            if (_logger.IsTrace) _logger.Trace($"Client disconnected: {innerException.Message}.");
                        }
                        else
                        {
                            if (_logger.IsInfo) _logger.Info($"Not able to read from WebSockets ({socketException.SocketErrorCode}: {socketException.ErrorCode}). {innerException.Message}");
                        }
                    }
                    else if (innerException is WebSocketException webSocketException)
                    {
                        if (webSocketException.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                        {
                            if (_logger.IsTrace) _logger.Trace($"Client disconnected: {innerException.Message}.");
                        }
                        else
                        {
                            if (_logger.IsInfo) _logger.Info($"Not able to read from WebSockets ({webSocketException.WebSocketErrorCode}: {webSocketException.ErrorCode}). {innerException.Message}");
                        }
                    }
                    else
                    {
                        if (_logger.IsInfo) _logger.Info($"Not able to read from WebSockets. {innerException?.Message}");
                    }

                    result = new ReceiveResult(closed: true);
                }
            }

            return result;
        }

        public Task CloseAsync(ReceiveResult result)
        {
            if (_webSocket.State is WebSocketState.Open or WebSocketState.CloseSent)
            {
                return _webSocket.CloseAsync(_webSocket.CloseStatus ?? WebSocketCloseStatus.Empty,
                    _webSocket.CloseStatusDescription,
                    CancellationToken.None);
            }

            if (_webSocket.State is WebSocketState.CloseReceived)
            {
                return _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, _webSocket.CloseStatusDescription,
                    CancellationToken.None);
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _webSocket?.Dispose();
        }
    }
}
