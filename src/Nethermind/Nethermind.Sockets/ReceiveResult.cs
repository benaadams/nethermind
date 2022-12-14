// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nethermind.Sockets
{
    public readonly struct ReceiveResult
    {
        public readonly int Read;
        public readonly bool EndOfMessage;
        public readonly bool Closed;

        public ReceiveResult(int read, bool endOfMessage, bool closed)
        {
            Read = read;
            EndOfMessage = endOfMessage;
            Closed = closed;
        }

        public ReceiveResult(bool closed)
        {
            Read = 0;
            EndOfMessage = true;
            Closed = closed;
        }
    }
}
