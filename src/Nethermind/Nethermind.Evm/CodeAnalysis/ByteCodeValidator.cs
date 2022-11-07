using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethermind.Logging;
using Nethermind.Core.Attributes;
using Nethermind.Core.Specs;
using Org.BouncyCastle.Crypto.Agreement.Srp;
using Nethermind.Specs;

namespace Nethermind.Evm.CodeAnalysis
{
    internal static class ByteCodeValidator
    {
        private static EvmObjectFormat? EofFormatChecker = new EvmObjectFormat();
        public static void Initialize(ILogger logger = null)
        {
            if (EofFormatChecker is not null)
                EofFormatChecker = new EvmObjectFormat(logger);
        }

        public static bool HasEOFMagic(this ReadOnlySpan<byte> code) => EofFormatChecker.HasEOFFormat(code);
        public static bool ValidateByteCode(ReadOnlySpan<byte> code, IReleaseSpec _spec, out EofHeader header)
        {
            if (code.HasEOFMagic())
            {
                return EofFormatChecker.ValidateInstructions(code, out header, _spec);
            }

            header = null;
            return !CodeDepositHandler.CodeIsInvalid(_spec, code.ToArray());
        }
        public static bool ValidateByteCode(this ReadOnlySpan<byte> code, IReleaseSpec _spec)
                => ValidateByteCode(code, _spec, out _);

        public static bool IsEOFCode(ReadOnlySpan<byte> machineCode, IReleaseSpec _spec, out EofHeader header)
            => EofFormatChecker.ExtractHeader(machineCode, _spec, out header);


        public static bool IsEOFCode(ReadOnlySpan<byte> machineCode, out EofHeader header)
            => EofFormatChecker.ExtractHeader(machineCode, MainnetSpecProvider.Instance.GetSpec(MainnetSpecProvider.ShanghaiActivation), out header); // maybe Cancum spec ? 
    }
}
