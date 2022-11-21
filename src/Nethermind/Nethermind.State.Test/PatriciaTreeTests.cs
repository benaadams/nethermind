// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Test.Builders;
using Nethermind.Db;
using Nethermind.Int256;
using Nethermind.Logging;
using Nethermind.State;
using Nethermind.Trie.Pruning;
using NUnit.Framework;

namespace Nethermind.Store.Test
{
    [TestFixture, Parallelizable(ParallelScope.All)]
    public class PatriciaTreeTests
    {
        [Test]
        public void Create_commit_change_balance_get()
        {
            Account account = new(1);
            StateTree stateTree = new();
            stateTree.Set(TestItem.AddressA, account);
            stateTree.Commit(0);

            account = account.WithChangedBalance(2);
            stateTree.Set(TestItem.AddressA, account);
            stateTree.Commit(0);

            Account accountRestored = stateTree.Get(TestItem.AddressA);
            Assert.AreEqual((UInt256)2, accountRestored.Balance);
        }

        [Test]
        public void Create_create_commit_change_balance_get()
        {
            Account account = new(1);
            StateTree stateTree = new();
            stateTree.Set(TestItem.AddressA, account);
            stateTree.Set(TestItem.AddressB, account);
            stateTree.Commit(0);

            account = account.WithChangedBalance(2);
            stateTree.Set(TestItem.AddressA, account);
            stateTree.Commit(0);

            Account accountRestored = stateTree.Get(TestItem.AddressA);
            Assert.AreEqual((UInt256)2, accountRestored.Balance);
        }

        [Test]
        public void Create_commit_reset_change_balance_get()
        {
            MemDb db = new();
            Account account = new(1);
            StateTree stateTree = new(new TrieStore(db, LimboLogs.Instance), LimboLogs.Instance);
            stateTree.Set(TestItem.AddressA, account);
            stateTree.Commit(0);

            Keccak rootHash = stateTree.RootHash;
            stateTree.RootHash = null;

            stateTree.RootHash = rootHash;
            stateTree.Get(TestItem.AddressA);
            account = account.WithChangedBalance(2);
            stateTree.Set(TestItem.AddressA, account);
            stateTree.Commit(0);

            Assert.AreEqual(2, db.Keys.Count);
        }
    }
}
