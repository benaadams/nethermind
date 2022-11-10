﻿//  Copyright (c) 2021 Demerzel Solutions Limited
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

using System.IO.Compression;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Db;
using Nethermind.Evm.Tracing;
using Nethermind.Evm.Tracing.ParityStyle;
using Nethermind.Int256;
using Nethermind.Logging;
using Nethermind.Serialization.Json;

namespace Nethermind.JsonRpc.TraceStore;

/// <summary>
/// Tracer that can store traces of decorated tracer in database
/// </summary>
/// <typeparam name="TTrace">Trace type</typeparam>
/// <typeparam name="TTracer">Transaction tracer type</typeparam>
public class DbPersistingBlockTracer<TTrace, TTracer> : IBlockTracer where TTracer : class, ITxTracer
{
    private readonly IDb _db;
    private readonly Func<IReadOnlyCollection<TTrace>, byte[]> _serialization;
    private readonly bool _verifySerialized;
    private readonly IBlockTracer _blockTracer;
    private readonly BlockTracerBase<TTrace, TTracer> _tracerWithResults;
    private Keccak _currentBlockHash = null!;
    private long _currentBlockNumber;
    private readonly ILogger _logger;

    /// <summary>
    /// Creates the tracer
    /// </summary>
    /// <param name="blockTracer">Internal, actual tracer that does the tracing</param>
    /// <param name="db">Database</param>
    /// <param name="serialization">Method for serialization</param>
    /// <param name="logManager"></param>
    /// <param name="verifySerialized"></param>
    public DbPersistingBlockTracer(BlockTracerBase<TTrace, TTracer> blockTracer,
        IDb db,
        Func<IReadOnlyCollection<TTrace>, byte[]> serialization,
        ILogManager logManager,
        bool verifySerialized = false)
    {
        _db = db;
        _serialization = serialization;
        _verifySerialized = verifySerialized;
        _blockTracer = _tracerWithResults = blockTracer;
        _logger = logManager.GetClassLogger<DbPersistingBlockTracer<TTrace, TTracer>>();
    }

    public bool IsTracingRewards => _blockTracer.IsTracingRewards;

    public void ReportReward(Address author, string rewardType, UInt256 rewardValue) =>
        _blockTracer.ReportReward(author, rewardType, rewardValue);

    public void StartNewBlockTrace(Block block)
    {
        _currentBlockHash = block.Hash!;
        _currentBlockNumber = block.Number;
        _blockTracer.StartNewBlockTrace(block);
    }

    public ITxTracer StartNewTxTrace(Transaction? tx) => _blockTracer.StartNewTxTrace(tx);

    public void EndTxTrace() => _blockTracer.EndTxTrace();

    public void EndBlockTrace()
    {
        _blockTracer.EndBlockTrace();
        IReadOnlyCollection<TTrace> result = _tracerWithResults.BuildResult();
        byte[] tracesSerialized = _serialization(result);
        Keccak currentBlockHash = _currentBlockHash;
        long currentBlockNumber = _currentBlockNumber;
        _db.Set(currentBlockHash, tracesSerialized);
        if (_logger.IsTrace) _logger.Trace($"Saved traces for block {currentBlockNumber} ({currentBlockHash}) with size {tracesSerialized.Length} bytes for {result.Count} traces.");

        if (_verifySerialized)
        {
            Task.Run(() =>
            {
                try
                {
                    using GZipStream compressionStream2 = new(new MemoryStream(tracesSerialized), CompressionMode.Decompress);
                    new EthereumJsonSerializer().Deserialize<List<ParityLikeTxTrace>>(compressionStream2);
                }
                catch (Exception e)
                {
                    string tracesWrittenToPath = Path.Combine(Path.GetTempPath(), $"{currentBlockNumber}.json");
                    if (_logger.IsError) _logger.Error($"Can't deserialize trace logs for block {currentBlockNumber} ({currentBlockHash}), size {tracesSerialized.Length}, dump: {tracesWrittenToPath}", e);
                    File.WriteAllBytes(tracesWrittenToPath, tracesSerialized);
                }
            });
        }
    }
}
