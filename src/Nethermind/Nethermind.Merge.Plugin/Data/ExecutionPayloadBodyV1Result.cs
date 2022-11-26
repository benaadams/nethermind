// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only 

using System;
using Nethermind.Core;
using Nethermind.Serialization;
using Nethermind.Serialization.Rlp;

namespace Nethermind.Merge.Plugin.Data
{

    public class ExecutionPayloadBodyV1Result
    {
        public ExecutionPayloadBodyV1Result(Transaction[] transactions)
        {
            Transactions = new byte[transactions.Length][];
            for (int i = 0; i < Transactions.Length; i++)
            {
                Transactions[i] = MixedEncoding.Encode(transactions[i], RlpBehaviors.SkipTypedWrapping).ToArray();
            }
        }

        public byte[][] Transactions { get; set; }
    }
}
