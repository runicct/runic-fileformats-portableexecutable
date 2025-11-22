/*
* MIT License
* 
* Copyright (c) 2025 Runic Compiler Toolkit Contributors
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System;
using System.IO;

namespace Runic.FileFormats
{
    public partial class PortableExecutable
    {
        static class DosHeader
        {
            static readonly byte[] _peDosHeader = new Byte[]
            {
                0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00,
                0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00,
                0x0E, 0x1F, 0xBA, 0x0E, 0x00, 0xB4, 0x09, 0xCD, 0x21, 0xB8, 0x01, 0x4C, 0xCD, 0x21, 0x54, 0x68,
                0x69, 0x73, 0x20, 0x70, 0x72, 0x6F, 0x67, 0x72, 0x61, 0x6D, 0x20, 0x63, 0x61, 0x6E, 0x6E, 0x6F,
                0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 0x6E, 0x20, 0x69, 0x6E, 0x20, 0x44, 0x4F, 0x53, 0x20,
                0x6D, 0x6F, 0x64, 0x65, 0x2E, 0x0D, 0x0D, 0x0A, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };
            public static uint DOSHeaderAndDOSStubSize { get { return (uint)_peDosHeader.Length; } }
            public static void WriteDOSHeader(System.IO.BinaryWriter stream)
            {
                stream.Write(_peDosHeader, 0, _peDosHeader.Length);
            }
            public static void ReadDOSHeader(System.IO.BinaryReader stream)
            {
                ushort magic = stream.ReadUInt16(); // First 2 Bytes are 'MZ'
                if (magic != 0x5A4D) { return; }
                // Skip all the way to the COFF Header address
                stream.BaseStream.Seek(0x3A, SeekOrigin.Current);
                uint coffHeaderAddress = stream.ReadUInt32();
                stream.BaseStream.Seek(coffHeaderAddress - 0x40, SeekOrigin.Current);
            }
#if NET6_0_OR_GREATER
            public static void ReadDOSHeader(Span<byte> data, ref uint offset)
            {
                ushort magic = BitConverterLE.ToUInt16(data, offset); offset += 2; // First 2 Bytes are 'MZ'
                if (magic != 0x5A4D) { return; }
                offset += 0x3A; // Skip to the COFF Header address
                uint coffHeaderAddress = BitConverterLE.ToUInt32(data, offset); offset += 4;
                offset += coffHeaderAddress - 0x40;
            }
#endif
        }
    }
}
