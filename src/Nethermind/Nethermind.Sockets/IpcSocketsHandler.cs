// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Nethermind.Sockets
{
    public class IpcSocketsHandler : ISocketHandler
    {
        private readonly Socket _socket;

        public IpcSocketsHandler(Socket socket)
        {
            _socket = socket;
        }

        public async ValueTask SendRawAsync(Memory<byte> data)
        {
            if (_socket.Connected)
            {
                await _socket.SendAsync(data, SocketFlags.None);
            }
        }

        public async ValueTask<ReceiveResult> GetReceiveResult(Memory<byte> buffer)
        {
            ReceiveResult result = default;
            if (_socket.Connected)
            {
                int read = await _socket.ReceiveAsync(buffer, SocketFlags.None);
                result = new ReceiveResult
                (
                    read: read,
                    endOfMessage: read < buffer.Length || _socket.Available == 0,
                    closed: read == 0
                );
            }

            return result;
        }

        public Task CloseAsync(ReceiveResult result)
        {
            return Task.Factory.StartNew(_socket.Close);
        }

        public void Dispose()
        {
            _socket?.Dispose();
        }
    }
}
