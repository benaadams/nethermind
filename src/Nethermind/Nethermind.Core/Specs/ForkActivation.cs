using System;

namespace Nethermind.Core.Specs;

public readonly struct ForkActivation : IEquatable<ForkActivation>, IComparable<ForkActivation>
{
    public long BlockNumber { get; }
    public ulong? Timestamp { get; }

    public ForkActivation(long blockNumber, ulong? timestamp = null)
    {
        BlockNumber = blockNumber;
        Timestamp = timestamp;
    }
    public void Deconstruct(out long blockNumber, out ulong? timestamp)
    {
        blockNumber = BlockNumber;
        timestamp = Timestamp;
    }

    public static implicit operator ForkActivation(long blocknumber) => new(blocknumber);

    public static implicit operator ForkActivation((long, ulong) blocknumberAndTimestamp)
        => new(blocknumberAndTimestamp.Item1, blocknumberAndTimestamp.Item2);
    public static implicit operator ForkActivation((long, ulong?) blocknumberAndTimestamp)
        => new(blocknumberAndTimestamp.Item1, blocknumberAndTimestamp.Item2 ?? 0);

    public static implicit operator (long, ulong?)(ForkActivation forkActivation)
        => (forkActivation.BlockNumber, forkActivation.Timestamp);

    public bool Equals(ForkActivation other)
    {
        return BlockNumber == other.BlockNumber && Timestamp == other.Timestamp;
    }

    public override bool Equals(object? obj)
    {
        return obj is ForkActivation other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BlockNumber, Timestamp);
    }

    public override string ToString()
    {
        return $"{BlockNumber} {Timestamp}";
    }

    public int CompareTo(ForkActivation other)
    {
        return Timestamp is null || other.Timestamp is null
            ? BlockNumber.CompareTo(other.BlockNumber)
            : Timestamp.Value.CompareTo(other.Timestamp.Value);
    }

    public static bool operator ==(ForkActivation first, ForkActivation second)
    {
        return first.Equals(second);
    }

    public static bool operator !=(ForkActivation first, ForkActivation second)
    {
        return !first.Equals(second);
    }

    public static bool operator <(ForkActivation first, ForkActivation second)
    {
        return first.CompareTo(second) < 0;
    }

    public static bool operator >(ForkActivation first, ForkActivation second)
    {
        return first.CompareTo(second) > 0;
    }

    public static bool operator <=(ForkActivation first, ForkActivation second)
    {
        return first.CompareTo(second) <= 0;
    }

    public static bool operator >=(ForkActivation first, ForkActivation second)
    {
        return first.CompareTo(second) >= 0;
    }
}
