//  Copyright (c) 2018 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
// 

using System;

namespace Nethermind.Trie.Pruning
{
    public class TriePersistedEventArgs : EventArgs
    {
        public TriePersistedEventArgs(long blockNumber, bool isReorganizationBoundary = false)
        {
            BlockNumber = blockNumber;
            IsReorganizationBoundary = isReorganizationBoundary;
        }

        public long BlockNumber { get; }
        
        /// <summary>
        /// Tells whether the reorg can go before this.
        /// </summary>
        public bool IsReorganizationBoundary { get; }
    }
    
    /// <summary>
    /// Tells which number is safe to mark as a checkpoint if it was persisted before.
    /// </summary>
    public class ReorgBoundaryReached : EventArgs
    {
        public ReorgBoundaryReached(long blockNumber)
        {
            BlockNumber = blockNumber;
        }

        public long BlockNumber { get; }
    }
}