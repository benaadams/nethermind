﻿using System.Text.Json;
using FastEnumUtility;
using Nethermind.Api;
using Nethermind.Api.Extensions;
using Nethermind.Blockchain.Find;
using Nethermind.Db;
using Nethermind.Evm.Tracing.ParityStyle;
using Nethermind.JsonRpc.Modules;
using Nethermind.JsonRpc.Modules.Trace;
using Nethermind.Logging;

namespace Nethermind.JsonRpc.TraceStore;

public class TraceStorePlugin : INethermindPlugin
{
    private const string DbName = "TraceStore";
    private INethermindApi _api = null!;
    private ITraceStoreConfig _config = null!;
    private IJsonRpcConfig _jsonRpcConfig = null!;
    private IDbWithSpan _db = null!;
    private TraceStorePruner? _pruner;
    private ILogManager _logManager = null!;
    public string Name => DbName;
    public string Description => "Allows to serve traces without the block state, by saving historical traces to DB.";
    public string Author => "Nethermind";
    private bool Enabled => _config.Enabled && _jsonRpcConfig.Enabled;

    public Task Init(INethermindApi nethermindApi)
    {
        _api = nethermindApi;
        _logManager = _api.LogManager;
        _config = _api.Config<ITraceStoreConfig>();
        _jsonRpcConfig = _api.Config<IJsonRpcConfig>();

        if (Enabled)
        {
            // Setup DB
            _db = (IDbWithSpan)_api.RocksDbFactory!.CreateDb(new RocksDbSettings(DbName, DbName.ToLower()));
            _api.DbProvider!.RegisterDb(DbName,  _db);

            // Setup tracing
            ParityLikeBlockTracer parityTracer = new(_config.TraceTypes);
            DbPersistingBlockTracer<ParityLikeTxTrace,ParityLikeTxTracer> dbPersistingTracer =
                new(parityTracer, _db, static t => TraceSerializer.Serialize(t), _logManager);
            _api.BlockchainProcessor!.Tracers.Add(dbPersistingTracer);

            //Setup pruning if configured
            if (_config.BlocksToKeep != 0)
            {
                _pruner = new TraceStorePruner(_api.BlockTree!, _db, _config.BlocksToKeep, _logManager);
            }
        }

        return Task.CompletedTask;
    }

    public Task InitNetworkProtocol()
    {
        // Potentially we could add protocol for syncing traces.
        return Task.CompletedTask;
    }

    public Task InitRpcModules()
    {
        if (Enabled)
        {
            IRpcModuleProvider apiRpcModuleProvider = _api.RpcModuleProvider!;
            if (apiRpcModuleProvider.GetPool(ModuleType.Trace) is IRpcModulePool<ITraceRpcModule> traceModulePool)
            {
                TraceStoreModuleFactory traceModuleFactory = new(traceModulePool.Factory, _db, _api.BlockTree!, _api.ReceiptFinder!, _logManager);
                apiRpcModuleProvider.RegisterBoundedByCpuCount(traceModuleFactory, _jsonRpcConfig.Timeout);
            }
        }

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _pruner?.Dispose();
        _db.Dispose();
        return default;
    }
}