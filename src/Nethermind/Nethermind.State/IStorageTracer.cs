// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Core;

namespace Nethermind.State
{
    public interface IStorageTracer
    {
        bool IsTracingStorage { get; }
        void ReportStorageChange(StorageCell storageCell, byte[] before, byte[] after);
        void ReportStorageRead(StorageCell storageCell);
    }
}
