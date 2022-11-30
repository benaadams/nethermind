// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Core;
using Nethermind.Core.Specs;
using Nethermind.Int256;

namespace Nethermind.Specs
{
    public class TestSpecProvider : ISpecProvider
    {
        private ForkActivation? _theMergeBlock = null;

        public TestSpecProvider(IReleaseSpec initialSpecToReturn)
        {
            SpecToReturn = initialSpecToReturn;
            GenesisSpec = initialSpecToReturn;
        }
        public TestSpecProvider(IReleaseSpec initialSpecToReturn, IReleaseSpec finalSpecToReturn)
        {
            SpecToReturn = finalSpecToReturn;
            GenesisSpec = initialSpecToReturn;
        }

        public TestSpecProvider(IReleaseSpec initialSpecToReturn, IReleaseSpec finalSpecToReturn)
        {
            SpecToReturn = finalSpecToReturn;
            GenesisSpec = initialSpecToReturn;
        }

        public void UpdateMergeTransitionInfo(long? blockNumber, UInt256? terminalTotalDifficulty = null)
        {
            if (blockNumber is not null)
                _theMergeBlock = blockNumber;
            if (terminalTotalDifficulty is not null)
                TerminalTotalDifficulty = terminalTotalDifficulty;
        }

        public ForkActivation? MergeBlockNumber => _theMergeBlock;
        public UInt256? TerminalTotalDifficulty { get; set; }

        public IReleaseSpec GenesisSpec { get; set; }

        public IReleaseSpec GetSpec(ForkActivation forkActivation) => SpecToReturn;

        public IReleaseSpec SpecToReturn { get; set; }

        public long? DaoBlockNumber { get; set; }
        public ulong ChainId { get; set; }
        public ForkActivation[] TransitionBlocks { get; set; } = new ForkActivation[] { 0 };
        public bool AllowTestChainOverride { get; set; } = true;

        private TestSpecProvider() { }

        public static readonly TestSpecProvider Instance = new();
    }
}
