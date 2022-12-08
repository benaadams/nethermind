// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only 

using System;
using System.Collections;
using System.Threading;
using Nethermind.Core.Extensions;
using Nethermind.Core.Specs;

namespace Nethermind.Evm.CodeAnalysis
{
    public class CodeDataAnalyzer : ICodeInfoAnalyzer
    {
        internal class DataAnalysisResult
        {
            public byte[]? _codeBitmap;
        }

        private DataAnalysisResult[] _analysisResults;
        public byte[] MachineCode { get; set; }
        private IReleaseSpec _releaseSpec;
        public EofHeader Header { get; set; }

        public CodeDataAnalyzer(byte[] code, EofHeader? header, IReleaseSpec spec)
        {
            MachineCode = code;
            Header = header;
            _releaseSpec = spec;
            _analysisResults = new DataAnalysisResult[Header?.CodeSize?.Length ?? 1];
        }

        public bool ValidateJump(int destination, bool isSubroutine, int sectionId = 0)
        {
            var (sectionStart, sectionSize) = Header is null ? (0, MachineCode.Length) : Header[sectionId];
            var codeSection = MachineCode.Slice(sectionStart, sectionSize);

            if (_analysisResults[sectionId] is null)
            {
                _analysisResults[sectionId] = new DataAnalysisResult
                {
                    _codeBitmap = CodeDataAnalyzerHelper.CreateCodeBitmap(codeSection, _releaseSpec)
                };
            }

            var codeBitmap = _analysisResults[sectionId]._codeBitmap;

            if (destination < 0 || destination >= codeSection.Length)
            {
                return false;
            }

            if (!CodeDataAnalyzerHelper.IsCodeSegment(codeBitmap, destination))
            {
                return false;
            }

            if (_releaseSpec.SubroutinesEnabled && isSubroutine)
            {
                return codeSection[destination] == 0x5c;
            }

            return codeSection[destination] == 0x5b;
        }
    }

    public static class CodeDataAnalyzerHelper
    {
        private const UInt16 Set2BitsMask = 0b1100_0000_0000_0000;
        private const UInt16 Set3BitsMask = 0b1110_0000_0000_0000;
        private const UInt16 Set4BitsMask = 0b1111_0000_0000_0000;
        private const UInt16 Set5BitsMask = 0b1111_1000_0000_0000;
        private const UInt16 Set6BitsMask = 0b1111_1100_0000_0000;
        private const UInt16 Set7BitsMask = 0b1111_1110_0000_0000;

        private static readonly byte[] _lookup = new byte[8] { 0x80, 0x40, 0x20, 0x10, 0x8, 0x4, 0x2, 0x1, };

        /// <summary>
        /// Collects data locations in code.
        /// An unset bit means the byte is an opcode, a set bit means it's data.
        /// </summary>
        public static byte[] CreateCodeBitmap(byte[] code, IReleaseSpec spec)
        {
            // The bitmap is 4 bytes longer than necessary, in case the code
            // ends with a PUSH32, the algorithm will push zeroes onto the
            // bitvector outside the bounds of the actual code.
            byte[] bitvec = new byte[(code.Length / 8) + 1 + 4];

            byte push1 = (byte)Instruction.PUSH1;
            byte push32 = (byte)Instruction.PUSH32;

            byte rjump = (byte)Instruction.RJUMP;
            byte rjumpi = (byte)Instruction.RJUMPI;
            byte rjumpv = (byte)Instruction.RJUMPV;

            byte callf = (byte)Instruction.CALLF;
            byte jumpf = (byte)Instruction.JUMPF;

            for (int pc = 0; pc < code.Length;)
            {
                byte op = code[pc];
                pc++;

                if ((op < push1 || op > push32) && (op != rjump && op != rjumpv && op != rjumpi) && (op != callf))
                {
                    continue;
                }

                if ((!spec.StaticRelativeJumpsEnabled && (op == rjump || op == rjumpi || op == rjumpv))
                || (!spec.FunctionSections && (op == callf || op == jumpf)))
                {
                    continue;
                }

                int numbits;
                switch ((Instruction)op)
                {
                    case Instruction.RJUMP:
                    case Instruction.JUMPF:
                    case Instruction.CALLF:
                    case Instruction.RJUMPI:
                        numbits = 2;
                        break;
                    case Instruction.RJUMPV:
                        byte count = code[pc];
                        numbits = count * 2 + 1;
                        break;
                    default:
                        numbits = op - push1 + 1;
                        break;
                }

                if (numbits >= 8)
                {
                    for (; numbits >= 16; numbits -= 16)
                    {
                        bitvec.Set16(pc);
                        pc += 16;
                    }

                    for (; numbits >= 8; numbits -= 8)
                    {
                        bitvec.Set8(pc);
                        pc += 8;
                    }
                }

                switch (numbits)
                {
                    case 1:
                        bitvec.Set1(pc);
                        pc += 1;
                        break;
                    case 2:
                        bitvec.SetN(pc, Set2BitsMask);
                        pc += 2;
                        break;
                    case 3:
                        bitvec.SetN(pc, Set3BitsMask);
                        pc += 3;
                        break;
                    case 4:
                        bitvec.SetN(pc, Set4BitsMask);
                        pc += 4;
                        break;
                    case 5:
                        bitvec.SetN(pc, Set5BitsMask);
                        pc += 5;
                        break;
                    case 6:
                        bitvec.SetN(pc, Set6BitsMask);
                        pc += 6;
                        break;
                    case 7:
                        bitvec.SetN(pc, Set7BitsMask);
                        pc += 7;
                        break;
                }
            }

            return bitvec;
        }

        /// <summary>
        /// Checks if the position is in a code segment.
        /// </summary>
        public static bool IsCodeSegment(byte[] bitvec, int pos)
        {
            return (bitvec[pos / 8] & (0x80 >> (pos % 8))) == 0;
        }

        private static void Set1(this byte[] bitvec, int pos)
        {
            bitvec[pos / 8] |= _lookup[pos % 8];
        }

        private static void SetN(this byte[] bitvec, int pos, UInt16 flag)
        {
            ushort a = (ushort)(flag >> (pos % 8));
            bitvec[pos / 8] |= (byte)(a >> 8);
            byte b = (byte)a;
            if (b != 0)
            {
                //	If the bit-setting affects the neighbouring byte, we can assign - no need to OR it,
                //	since it's the first write to that byte
                bitvec[pos / 8 + 1] = b;
            }
        }

        private static void Set8(this byte[] bitvec, int pos)
        {
            byte a = (byte)(0xFF >> (pos % 8));
            bitvec[pos / 8] |= a;
            bitvec[pos / 8 + 1] = (byte)~a;
        }

        private static void Set16(this byte[] bitvec, int pos)
        {
            byte a = (byte)(0xFF >> (pos % 8));
            bitvec[pos / 8] |= a;
            bitvec[pos / 8 + 1] = 0xFF;
            bitvec[pos / 8 + 2] = (byte)~a;
        }
    }
}
