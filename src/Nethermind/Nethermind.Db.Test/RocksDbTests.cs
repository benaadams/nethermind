using FluentAssertions;
using Nethermind.Db.Rocks;
using NUnit.Framework;

namespace Nethermind.Db.Test;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public static class RocksDbTests
{
    [Test]
    public static void Should_have_required_version() =>
        DbOnTheRocks.GetRocksDbVersion().Should().Be("7.7.3", "Unexpected RocksDB version");
}
