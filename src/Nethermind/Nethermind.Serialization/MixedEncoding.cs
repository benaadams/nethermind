using System;
using System.Data;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Extensions;
using Nethermind.Merkleization;
using Nethermind.Serialization.Rlp;

namespace Nethermind.Serialization
{
    public static class MixedEncoding
    {
        /// <summary>
        /// Detected encoding and use it to deserialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">Byte representing Transaction</param>
        /// <param name="rlpBehaviors">Decoding flags</param>
        /// <returns>Deserialized Transaction</returns>
        public static T Decode<T>(Span<byte> data, RlpBehaviors rlpBehaviors) where T : Transaction
        {
            TxType detectedType;
            if ((rlpBehaviors & RlpBehaviors.SkipTypedWrapping) == RlpBehaviors.SkipTypedWrapping)
            {
                detectedType = (TxType)data[0];
            }
            else
            { 
                detectedType = TxType.EIP1559;
            }

            switch (detectedType)
            {
                case TxType.Blob:
                    int offset = (rlpBehaviors & RlpBehaviors.SkipTypedWrapping) == RlpBehaviors.SkipTypedWrapping ? 1 : 0;
                    if ((rlpBehaviors & RlpBehaviors.NetworkWrapper) == RlpBehaviors.NetworkWrapper)
                    {
                        T result = (T)Ssz.Ssz.DecodeBlobTransactionNetworkWrapper(data.Slice(offset));
                        Rlp.Rlp rlpEncoded = Rlp.Rlp.Encode<T>(result, rlpBehaviors);
                        result.Hash = Keccak.Compute(rlpEncoded.Bytes);
                        return result;
                    }
                    else
                    {
                        T result = (T)Ssz.Ssz.DecodeSignedBlobTransaction(data.Slice(offset));
                        Rlp.Rlp rlpEncoded = Rlp.Rlp.Encode<T>(result, rlpBehaviors);
                        var str = rlpEncoded.Bytes.ToHexString();   
                        result.Hash = Keccak.Compute(rlpEncoded.Bytes);
                        return result;
                    }
                default:
                    return Rlp.Rlp.Decode<T>(data, rlpBehaviors);
            }
        }

        /// <summary>
        /// Detected encoding and use it to deserialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">Byte representing Transaction</param>
        /// <param name="rlpBehaviors">Decoding flags</param>
        /// <returns>Deserialized Transaction</returns>
        public static Span<byte> Encode<T>(T transaction, RlpBehaviors rlpBehaviors) where T : Transaction
        {
            switch (transaction.Type)
            {
                case TxType.Blob:
                    if ((rlpBehaviors & RlpBehaviors.NetworkWrapper) == RlpBehaviors.NetworkWrapper)
                    {
                        Span<byte> encodedTx = new byte[Ssz.Ssz.BlobTransactionNetworkWrapperLength(transaction) + 1];
                        encodedTx[0] = (byte)TxType.Blob;
                        Ssz.Ssz.EncodeSignedWrapper(encodedTx[1..], transaction);
                        return encodedTx;
                    }
                    else
                    {
                        Span<byte> encodedTx = new byte[Ssz.Ssz.SignedBlobTransactionLength(transaction) + 1];
                        encodedTx[0] = (byte)TxType.Blob;
                        Ssz.Ssz.EncodeSigned(encodedTx[1..], transaction);
                        return encodedTx;
                    }
                default:
                    return Rlp.Rlp.Encode<T>(transaction, rlpBehaviors).Bytes;
            }
        }

        public static T[] DecodeArray<T>(RlpStream rlpStream, RlpBehaviors rlpBehaviors = RlpBehaviors.None) where T : Transaction
        {
            int checkPosition = rlpStream.ReadSequenceLength() + rlpStream.Position;
            T[] result = new T[rlpStream.ReadNumberOfItemsRemaining(checkPosition)];
            for (int i = 0; i < result.Length; i++)
            {
                Span<byte> data = rlpStream.DecodeByteArray();
                result[i] = Decode<T>(data, rlpBehaviors);
            }

            return result;
        }

        /// <summary>
        /// Detected encoding and use it to deserialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">Byte representing Transaction</param>
        /// <param name="rlpBehaviors">Decoding flags</param>
        /// <returns>Deserialized Transaction</returns>
        public static Span<byte> EncodeForSigning<T>(T transaction, bool isEip155Enabled, ulong chainIdValue) where T : Transaction
        {
            switch (transaction.Type)
            {
                case TxType.Blob:
                    Merkle.Ize(out Dirichlet.Numerics.UInt256 merkleRoot, transaction);
                    Span<byte> data = new byte[33];
                    data[0] = (byte)TxType.Blob;
                    merkleRoot.ToLittleEndian(data[1..]);
                    return data;
                default:
                    return Rlp.Rlp.Encode(transaction, true, isEip155Enabled, chainIdValue).Bytes;
            }
        }

        /// <summary>
        /// Detected encoding and use it to deserialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">Byte representing Transaction</param>
        /// <param name="rlpBehaviors">Decoding flags</param>
        /// <returns>Deserialized Transaction</returns>
        public static Keccak ComputeHash<T>(T transaction) where T : Transaction
        {
            Span<byte> data;
            switch (transaction.Type)
            {
                case TxType.Blob:
                    Rlp.Rlp rlpEncoded = Rlp.Rlp.Encode<T>(transaction);
                    data = rlpEncoded.Bytes;
                    break;
                default:
                    data = Rlp.Rlp.Encode(transaction).Bytes;
                    break;
            }

            return Keccak.Compute(data);
        }
    }
}
