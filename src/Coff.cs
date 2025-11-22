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
using System.Collections.Generic;

namespace Runic.FileFormats
{
    public partial class PortableExecutable
    {
        public enum MachineType : ushort
        {
            Unknown = 0x0,
            Alpha = 0x184,
            Alpha64 = 0x284,
            Am33 = 0x1d3,
            Amd64 = 0x8664,
            Arm = 0x1c0,
            Arm64 = 0xaa64,
            Armnt = 0x1c4,
            i386 = 0x14c
        }
        MachineType _machineType = MachineType.i386;
        public MachineType Machine { get { return _machineType; } set { _machineType = value; } }

        CoffImageFlags _imageFlags = CoffImageFlags.None;
        public enum CoffImageFlags : ushort
        {
            None = 0x0,
            RelocStripped = 0x0001,
            ExecutableImage = 0x0002,
            LargeAddressAware = 0x0020,
            Machine32Bits = 0x0100,
            Dll = 0x2000,
        }
        public CoffImageFlags ImageFlags { get { return _imageFlags; } set { _imageFlags = value; } }
        DateTime _timestamp = DateTime.UtcNow;
        public DateTime Timestamp { get { return _timestamp; } set { _timestamp = value; } }
        void ReadCoffHeader(System.IO.BinaryReader stream, out uint optionalHeaderSize)
        {
            _machineType = (MachineType)stream.ReadUInt16();
            _sections = new Section[stream.ReadUInt16()];
            _timestamp = ToDatetime(stream.ReadUInt32());

            stream.ReadUInt32(); // Ignored
            stream.ReadUInt32(); // Ignored

            optionalHeaderSize = stream.ReadUInt16();
            _imageFlags = (CoffImageFlags)stream.ReadUInt16();
        }
#if NET6_0_OR_GREATER
        void ReadCoffHeader(Span<byte> data, ref uint offset, out uint optionalHeaderSize)
        {
            _machineType = (MachineType)(BitConverterLE.ToUInt16(data, offset)); offset += 2;
            _sections = new Section[BitConverterLE.ToUInt16(data, offset)]; offset += 2;
            _timestamp = ToDatetime(BitConverterLE.ToUInt32(data, offset)); offset += 4;

            offset += 4; // Ignored for .NET
            offset += 4; // Ignored for .NET

            optionalHeaderSize = BitConverterLE.ToUInt16(data, offset); offset += 2;
            _imageFlags = (CoffImageFlags)(BitConverterLE.ToUInt16(data, offset)); offset += 2;
        }
#endif

        void WriteCoffHeader(System.IO.BinaryWriter stream)
        {
            // COFF Magic number
            stream.Write((ushort)_machineType);
            stream.Write((ushort)_sections.Length);
            ulong timestamp = ToTimestamp(_timestamp);
            stream.Write((uint)(timestamp & 0xFFFFFFFF));
            stream.Write((uint)(0));
            stream.Write((uint)(0));

            if (_isPE32Plus)
            {
                stream.Write((ushort)(PE32PlusHeaderSize)); // SizeOfOptionalHeader
            }
            else
            {
                stream.Write((ushort)(PE32HeaderSize)); // SizeOfOptionalHeader
            }
            stream.Write((ushort)(_imageFlags));
        }
    }
}
