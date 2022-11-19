// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Microsoft.VisualBasic;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Int256;
using Nethermind.Serialization.Rlp;

namespace Nethermind.Crypto
{
    public static class BlockHeaderExtensions
    {
        private static readonly HeaderDecoder _headerDecoder = new();

        public static Keccak CalculateHash(this BlockHeader header, RlpBehaviors behaviors = RlpBehaviors.None)
        {
            KeccakRlpStream stream = new();
            _headerDecoder.Encode(stream, header, behaviors);


            var a = stream.GetHash();
            var b = a.ToString();

            var stream2 = new RlpStream(new byte[2048]);
            _headerDecoder.Encode(stream2, header, behaviors);
            if (header.Number > 0)
            {

            }
            return a;
        }

        public static Keccak CalculateHash(this Block block, RlpBehaviors behaviors = RlpBehaviors.None) => CalculateHash(block.Header, behaviors);

        public static Keccak GetOrCalculateHash(this BlockHeader header) => header.Hash ?? header.CalculateHash();

        public static Keccak GetOrCalculateHash(this Block block) => block.Hash ?? block.CalculateHash();

        public static bool IsNonZeroTotalDifficulty(this Block block) => block.TotalDifficulty is not null && block.TotalDifficulty != UInt256.Zero;
        public static bool IsNonZeroTotalDifficulty(this BlockHeader header) => header.TotalDifficulty is not null && header.TotalDifficulty != UInt256.Zero;
    }
}
