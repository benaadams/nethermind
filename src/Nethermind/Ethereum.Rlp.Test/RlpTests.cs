// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ethereum.Test.Base;
using Nethermind.Core;
using Nethermind.Core.Extensions;
using Nethermind.Serialization.Rlp;
using NUnit.Framework;

namespace Ethereum.Rlp.Test
{
    [Parallelizable(ParallelScope.All)]
    [TestFixture]
    public class RlpTests
    {
        private class RlpTestJson
        {
            public object In { get; set; }
            public string Out { get; set; }
        }

        [DebuggerDisplay("{Name}")]
        public class RlpTest
        {
            public RlpTest(string name, object input, string output)
            {
                Name = name;
                Input = input;
                Output = output;
            }

            public string Name { get; }
            public object Input { get; }
            public string Output { get; }

            public override string ToString()
            {
                return $"{Name} exp: {Output}";
            }
        }

        private static IEnumerable<RlpTest> LoadValidTests()
        {
            return LoadTests("rlptest.json");
        }

        private static IEnumerable<RlpTest> LoadRandomTests()
        {
            return LoadTests("example.json");
        }

        private static IEnumerable<RlpTest> LoadInvalidTests()
        {
            return LoadTests("invalidRLPTest.json");
        }

        private static IEnumerable<RlpTest> LoadTests(string testFileName)
        {
            return TestLoader.LoadFromFile<Dictionary<string, RlpTestJson>, RlpTest>(
                testFileName,
                c => c.Select(p => new RlpTest(p.Key, p.Value.In, p.Value.Out)));
        }

        //[TestCaseSource(nameof(LoadValidTests))]
        //public void Test(RlpTest test)
        //{
        //    object input = TestLoader.PrepareInput(test.Input);

        //    Nethermind.Serialization.Rlp.Rlp serialized = Nethermind.Serialization.Rlp.Encode(input);
        //    string serializedHex = serialized.ToString(false);

        //    object deserialized = Nethermind.Serialization.Rlp.Rlp.Decode(serialized);
        //    Nethermind.Serialization.Rlp.Rlp serializedAgain = Nethermind.Serialization.Rlp.Encode(deserialized);
        //    string serializedAgainHex = serializedAgain.ToString(false);

        //    Assert.AreEqual(test.Output, serializedHex);
        //    Assert.AreEqual(serializedHex, serializedAgainHex);
        //}

        //[TestCaseSource(nameof(LoadInvalidTests))]
        //public void TestInvalid(RlpTest test)
        //{
        //    Nethermind.Serialization.Rlp.Rlp invalidBytes = new Nethermind.Serialization.Rlp.Rlp(Hex.ToBytes(test.Output));
        //    Assert.Throws<RlpException>(
        //        () => Nethermind.Serialization.Rlp.Rlp.Decode(invalidBytes, RlpBehaviors.None));
        //}

        //[TestCaseSource(nameof(LoadRandomTests))]
        //public void TestRandom(RlpTest test)
        //{
        //    Nethermind.Serialization.Rlp.Rlp validBytes = new Nethermind.Serialization.Rlp.Rlp(Hex.ToBytes(test.Output));
        //    Nethermind.Serialization.Rlp.Rlp.Decode(validBytes);
        //}

        [Test]
        public void TestEmpty()
        {
            Assert.AreEqual(Nethermind.Serialization.Rlp.Rlp.OfEmptyByteArray, Nethermind.Serialization.Rlp.Rlp.Encode(new byte[0]));
            Assert.AreEqual(Nethermind.Serialization.Rlp.Rlp.OfEmptySequence, Nethermind.Serialization.Rlp.Rlp.Encode(new Nethermind.Serialization.Rlp.Rlp[0]));
        }

        [Test]
        public void TestCast()
        {
            byte[] expected = new byte[] { 1 };
            Assert.AreEqual(expected, Nethermind.Serialization.Rlp.Rlp.Encode((byte)1).Bytes, "byte");
            Assert.AreEqual(expected, Nethermind.Serialization.Rlp.Rlp.Encode((short)1).Bytes, "short");
            Assert.AreEqual(expected, Nethermind.Serialization.Rlp.Rlp.Encode((ushort)1).Bytes, "ushort");
            Assert.AreEqual(expected, Nethermind.Serialization.Rlp.Rlp.Encode(1).Bytes, "int");
            Assert.AreEqual(expected, Nethermind.Serialization.Rlp.Rlp.Encode(1U).Bytes, "uint bytes");
            Assert.AreEqual(expected, Nethermind.Serialization.Rlp.Rlp.Encode(1L).Bytes, "long bytes");
            Assert.AreEqual(expected, Nethermind.Serialization.Rlp.Rlp.Encode(1UL).Bytes, "ulong bytes");

            byte[] expectedNonce = new byte[] { 136, 0, 0, 0, 0, 0, 0, 0, 1 };
            Assert.AreEqual(expectedNonce, Nethermind.Serialization.Rlp.Rlp.EncodeNonce(1UL).Bytes, "nonce bytes");
        }

        [Test]
        public void TestNonce()
        {
            byte[] expected = { 136, 0, 0, 0, 0, 0, 0, 0, 42 };
            Assert.AreEqual(expected, Nethermind.Serialization.Rlp.Rlp.EncodeNonce(42UL).Bytes);
        }

        //[Ignore("placeholder for various rlp tests")]
        //[Test]
        //public void VariousTests()
        //{
        //    List<object> objects = new List<object>();
        //    objects.Add(0);

        //    byte[] result = Nethermind.Serialization.Rlp.Encode(objects).Bytes;


        //    List<byte[]> bytes = new List<byte[]>();
        //    bytes.Add(Nethermind.Serialization.Rlp.Encode(0).Bytes);

        //    byte[] resultBytes = Nethermind.Serialization.Rlp.Encode(bytes).Bytes;

        //    List<object> bytesRlp = new List<object>();
        //    bytesRlp.Add(Nethermind.Serialization.Rlp.Encode(0));

        //    byte[] resultRlp = Nethermind.Serialization.Rlp.Encode(bytesRlp).Bytes;

        //    Assert.AreEqual(resultRlp, result);
        //    Assert.AreEqual(result, resultBytes);
        //}

        [Ignore("only use when testing various perf changes")]
        [Test]
        public void PerfTest()
        {
            const int iterations = 40000;
            byte[] bytes = Bytes.FromHexString("0xf91267f901fba085366996aa23e67a63326459bcfff5ef437d21eeb3cd3a4e6ed6d2a1f2b0a4dfa01dcc4de8dec75d7aab85b567b6ccd41ad312451b948a7413f0a142fd40d49347948888f1f195afa192cfee860698584c030f4c9db1a0da2a45001f2fc1335a5e9cbce68d9e49db6fb20f71281343aba9951e1f518366a04627ca6cb9d890cc76a1335f907f3e9b5952ddeb23674843216733922561ed76a0813df8950a2895f8d2d8025204ce2a4dc16ff80fb125a63a52a78a179dbb690db901000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000083020000018401cb799e8312343f8459d6632d80a0010c08b98cc235529978d149495bfa417269d664462cc8c450d82e523905235c886b732e69295dcfc7f91065f9106280018312343f8064b910146060604052604051602080611014833960806040818152925160016000818155818055600160a060020a03331660038190558152610102909452938320939093556201518042046101075582917f102d25c49d33fcdb8976a3f2744e0785c98d9e43b88364859e6aec4ae82eff5c91a250610f958061007f6000396000f300606060405236156100b95760e060020a6000350463173825d9811461010b5780632f54bf6e146101675780634123cb6b1461018f5780635c52c2f5146101985780637065cb48146101c9578063746c9171146101fd578063797af62714610206578063b20d30a914610219578063b61d27f61461024d578063b75c7dc61461026e578063ba51a6df1461029e578063c2cf7326146102d2578063cbf0b0c014610312578063f00d4b5d14610346578063f1736d861461037f575b61038960003411156101095760408051600160a060020a033316815234602082015281517fe1fffcc4923d04b559f4d29a8bfc6cda04eb5b0d3c460751c2402c5c5cc9109c929181900390910190a15b565b610389600435600060003643604051808484808284375050509091019081526040519081900360200190209050610693815b600160a060020a0333166000908152610102602052604081205481808083811415610c1357610d6c565b61038b6004355b600160a060020a03811660009081526101026020526040812054115b919050565b61038b60015481565b610389600036436040518084848082843750505090910190815260405190819003602001902090506107e58161013d565b6103896004356000364360405180848480828437505050909101908152604051908190036020019020905061060b8161013d565b61038b60005481565b61038b6004355b600081610a4b8161013d565b610389600435600036436040518084848082843750505090910190815260405190819003602001902090506107d98161013d565b61038b6004803590602480359160443591820191013560006108043361016e565b610389600435600160a060020a033316600090815261010260205260408120549080808381141561039d5761041f565b610389600435600036436040518084848082843750505090910190815260405190819003602001902090506107528161013d565b61038b600435602435600082815261010360209081526040808320600160a060020a0385168452610102909252822054829081818114156107ab576107cf565b610389600435600036436040518084848082843750505090910190815260405190819003602001902090506107f38161013d565b6103896004356024356000600036436040518084848082843750505090910190815260405190819003602001902090506104ac8161013d565b61038b6101055481565b005b60408051918252519081900360200190f35b5050506000828152610103602052604081206001810154600284900a929083168190111561041f5781546001838101805492909101845590849003905560408051600160a060020a03331681526020810187905281517fc7fb647e59b18047309aa15aad418e5d7ca96d173ad704f1031a2c3d7591734b929181900390910190a15b5050505050565b600160a060020a03831660028361010081101561000257508301819055600160a060020a03851660008181526101026020908152604080832083905584835291829020869055815192835282019290925281517fb532073b38c83145e3e5135377a08bf9aab55bc0fd7c1179cd4fb995d2a5159c929181900390910190a15b505b505050565b156104a5576104ba8361016e565b156104c557506104a7565b600160a060020a0384166000908152610102602052604081205492508214156104ee57506104a7565b6104265b6101045460005b81811015610eba57610104805461010891600091849081101561000257600080516020610f7583398151915201548252506020918252604081208054600160a060020a0319168155600181018290556002810180548382559083528383209193610f3f92601f9290920104810190610a33565b60018054810190819055600160a060020a038316906002906101008110156100025790900160005081905550600160005054610102600050600084600160a060020a03168152602001908152602001600020600050819055507f994a936646fe87ffe4f1e469d3d6aa417d6b855598397f323de5b449f765f0c3826040518082600160a060020a0316815260200191505060405180910390a15b505b50565b15610606576106198261016e565b156106245750610608565b61062c6104f2565b60015460fa90106106415761063f610656565b505b60015460fa901061056c5750610608565b6107105b600060015b600154811015610a47575b600154811080156106865750600281610100811015610002570154600014155b15610d7557600101610666565b156104a757600160a060020a0383166000908152610102602052604081205492508214156106c15750610606565b60016001600050540360006000505411156106dc5750610606565b600060028361010081101561000257508301819055600160a060020a038416815261010260205260408120556106526104f2565b5060408051600160a060020a038516815290517f58619076adf5bb0943d100ef88d52d7c3fd691b19d3a9071b555b651fbf418da9181900360200190a1505050565b15610606576001548211156107675750610608565b60008290556107746104f2565b6040805183815290517facbdb084c721332ac59f9b8e392196c9eb0e4932862da8eb9beaf0dad4f550da9181900360200190a15050565b506001830154600282900a908116600014156107ca57600094506107cf565b600194505b5050505092915050565b15610606575061010555565b156106085760006101065550565b156106065781600160a060020a0316ff5b15610a2357610818846000610e4f3361016e565b156108d4577f92ca3a80853e6663fa31fa10b99225f18d4902939b4c53a9caae9043f6efd00433858786866040518086600160a060020a0316815260200185815260200184600160a060020a031681526020018060200182810382528484828181526020019250808284378201915050965050505050505060405180910390a184600160a060020a03168484846040518083838082843750505090810191506000908083038185876185025a03f15060009350610a2392505050565b6000364360405180848480828437505050909101908152604051908190036020019020915061090490508161020d565b158015610927575060008181526101086020526040812054600160a060020a0316145b15610a235760008181526101086020908152604082208054600160a060020a03191688178155600181018790556002018054858255818452928290209092601f01919091048101908490868215610a2b579182015b82811115610a2b57823582600050559160200191906001019061097c565b50600050507f1733cbb53659d713b79580f79f3f9ff215f78a7c7aa45890f3b89fc5cddfbf328133868887876040518087815260200186600160a060020a0316815260200185815260200184600160a060020a03168152602001806020018281038252848482818152602001925080828437820191505097505050505050505060405180910390a15b949350505050565b5061099a9291505b80821115610a475760008155600101610a33565b5090565b15610c005760008381526101086020526040812054600160a060020a031614610c0057604080516000918220805460018201546002929092018054600160a060020a0392909216949293909291819084908015610acd57820191906000526020600020905b815481529060010190602001808311610ab057829003601f168201915b50509250505060006040518083038185876185025a03f1505050600084815261010860209081526040805181842080546001820154600160a060020a033381811686529685018c905294840181905293166060830181905260a06080840181815260029390930180549185018290527fe7c957c06e9a662c1a6c77366179f5b702b97651dc28eee7d5bf1dff6e40bb4a985095968b969294929390929160c083019085908015610ba257820191906000526020600020905b815481529060010190602001808311610b8557829003601f168201915b505097505050505050505060405180910390a160008381526101086020908152604082208054600160a060020a031916815560018101839055600281018054848255908452828420919392610c0692601f9290920104810190610a33565b50919050565b505050600191505061018a565b6000868152610103602052604081208054909450909250821415610c9c578154835560018381018390556101048054918201808255828015829011610c6b57818360005260206000209182019101610c6b9190610a33565b50505060028401819055610104805488929081101561000257600091909152600080516020610f7583398151915201555b506001820154600284900a90811660001415610d6c5760408051600160a060020a03331681526020810188905281517fe1c52dc63b719ade82e8bea94cc41a0d5d28e4aaf536adb5e9cccc9ff8c1aeda929181900390910190a1825460019011610d59576000868152610103602052604090206002015461010480549091908110156100025760406000908120600080516020610f758339815191529290920181905580825560018083018290556002909201559550610d6c9050565b8254600019018355600183018054821790555b50505050919050565b5b60018054118015610d9857506001546002906101008110156100025701546000145b15610dac5760018054600019019055610d76565b60015481108015610dcf5750600154600290610100811015610002570154600014155b8015610de957506002816101008110156100025701546000145b15610e4a57600154600290610100811015610002578101549082610100811015610002578101919091558190610102906000908361010081101561000257810154825260209290925260408120929092556001546101008110156100025701555b61065b565b1561018a5761010754610e655b62015180420490565b1115610e7e57600061010655610e79610e5c565b610107555b6101065480830110801590610e9c5750610106546101055490830111155b15610eb25750610106805482019055600161018a565b50600061018a565b6106066101045460005b81811015610f4a5761010480548290811015610002576000918252600080516020610f75833981519152015414610f3757610104805461010391600091849081101561000257600080516020610f7583398151915201548252506020919091526040812081815560018101829055600201555b600101610ec4565b5050506001016104f9565b61010480546000808355919091526104a790600080516020610f7583398151915290810190610a3356004c0be60200faa20559308cb7b5a1bb3255c16cb1cab91f525b5ae7a03d02fabe1ba0391c21cb3127c5b49fdea2512f7f78cde18ff1937f96ee11c747135179acc1e1a02fee8b30c0caa7d8bca1ee5c045a59d4be1f9440446e98f32a9345bbc03ae50ec0");

            Block block = null;
            Block perfBlock = null;
            for (int i = 0; i < iterations; i++)
            {
                block = Nethermind.Serialization.Rlp.Rlp.Decode<Block>(new Nethermind.Serialization.Rlp.Rlp(bytes));
                perfBlock = Nethermind.Serialization.Rlp.Rlp.Decode<Block>(bytes?.AsRlpStream());
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                block = Nethermind.Serialization.Rlp.Rlp.Decode<Block>(new Nethermind.Serialization.Rlp.Rlp(bytes));
            }

            stopwatch.Stop();
            Console.WriteLine($"1st: {stopwatch.ElapsedMilliseconds}");

            stopwatch.Restart();
            for (int i = 0; i < iterations; i++)
            {
                perfBlock = Nethermind.Serialization.Rlp.Rlp.Decode<Block>(bytes?.AsRlpStream());
            }

            stopwatch.Stop();
            Console.WriteLine($"2nd: {stopwatch.ElapsedMilliseconds}");

            if (block == null || perfBlock == null || block.Number != perfBlock.Number)
            {
                throw new Exception();
            }
        }
    }
}
