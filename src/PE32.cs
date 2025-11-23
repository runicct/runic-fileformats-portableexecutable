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

namespace Runic.FileFormats
{
    public partial class PortableExecutable
    {
        void ReadPE32Header(System.IO.BinaryReader stream)
        {
            byte linkerMajor = stream.ReadByte();
            byte linkerMinor = stream.ReadByte();
            _linkerVersion = new Version(linkerMajor, linkerMinor);
            uint totalTextSectionSize = stream.ReadUInt32();
            uint totalInitializedDataSectionSize = stream.ReadUInt32();
            uint totalUninitializedDataSectionSize = stream.ReadUInt32();
            _entryPointRelativeVirtualAddress = stream.ReadUInt32();
            uint baseOfCode = stream.ReadUInt32();
            uint baseOfData = stream.ReadUInt32();

            // Windows specific field
            _imageBase = stream.ReadUInt32();
            _sectionAlignment = stream.ReadUInt32();
            _fileAlignment = stream.ReadUInt32();

            ushort majorOperatingSystemVersion = stream.ReadUInt16();
            ushort minorOperatingSystemVersion = stream.ReadUInt16();
            _operatingSystemVersion = new Version(majorOperatingSystemVersion, minorOperatingSystemVersion);
            ushort majorImageVersion = stream.ReadUInt16();
            ushort minorImageVersion = stream.ReadUInt16();
            _imageVersion = new Version(majorImageVersion, minorImageVersion);
            ushort majorSubsystemVersion = stream.ReadUInt16();
            ushort minorSubsystemVersion = stream.ReadUInt16();
            _subsystemVersion = new Version(majorSubsystemVersion, minorSubsystemVersion);
            stream.ReadUInt32();
            uint sizeOfImage = stream.ReadUInt32();
            uint sizeOfHeadersAligned = stream.ReadUInt32();
            uint checksum = stream.ReadUInt32();
            _subsystem = (ImageSubsystem)stream.ReadUInt16();
            _dllCharacteristicsFlags = (DllCharacteristics)stream.ReadUInt16();
            _stackReserveSize = stream.ReadUInt32();
            _stackCommitSize = stream.ReadUInt32();
            _heapReserveSize = stream.ReadUInt32();
            _heapCommitSize = stream.ReadUInt32();
            stream.ReadUInt32(); // Reserved
        }

#if NET6_0_OR_GREATER
        void ReadPE32Header(Span<byte> data, ref uint offset)
        {
            byte linkerMajor = data[(int)offset]; offset++;
            byte linkerMinor = data[(int)offset]; offset++;
            _linkerVersion = new Version(linkerMajor, linkerMinor);
            uint totalTextSectionSize = BitConverterLE.ToUInt32(data, offset); offset += 4;
            uint totalInitializedDataSectionSize = BitConverterLE.ToUInt32(data, offset); offset += 4;
            uint totalUninitializedDataSectionSize = BitConverterLE.ToUInt32(data, offset); offset += 4;
            _entryPointRelativeVirtualAddress = BitConverterLE.ToUInt32(data, offset); offset += 4;
            uint baseOfCode = BitConverterLE.ToUInt32(data, offset); offset += 4;
            uint baseOfData = BitConverterLE.ToUInt32(data, offset); offset += 4;

            // Windows specific field
            _imageBase = BitConverterLE.ToUInt32(data, offset); offset += 4;
            _sectionAlignment = BitConverterLE.ToUInt32(data, offset); offset += 4;
            _fileAlignment = BitConverterLE.ToUInt32(data, offset); offset += 4;

            ushort majorOperatingSystemVersion = BitConverterLE.ToUInt16(data, offset); offset += 2;
            ushort minorOperatingSystemVersion = BitConverterLE.ToUInt16(data, offset); offset += 2;
            _operatingSystemVersion = new Version(majorOperatingSystemVersion, minorOperatingSystemVersion);
            ushort majorImageVersion = BitConverterLE.ToUInt16(data, offset); offset += 2;
            ushort minorImageVersion = BitConverterLE.ToUInt16(data, offset); offset += 2;
            _imageVersion = new Version(majorImageVersion, minorImageVersion);
            ushort majorSubsystemVersion = BitConverterLE.ToUInt16(data, offset); offset += 2;
            ushort minorSubsystemVersion = BitConverterLE.ToUInt16(data, offset); offset += 2;
            _subsystemVersion = new Version(majorSubsystemVersion, minorSubsystemVersion);
            offset += 4;
            uint sizeOfImage = BitConverterLE.ToUInt32(data, offset); offset += 4;
            uint sizeOfHeadersAligned = BitConverterLE.ToUInt32(data, offset); offset += 4;
            uint checksum = BitConverterLE.ToUInt32(data, offset); offset += 4;
            _subsystem = (ImageSubsystem)BitConverterLE.ToUInt16(data, offset); offset += 2;
            _dllCharacteristicsFlags = (DllCharacteristics)BitConverterLE.ToUInt16(data, offset); offset += 2;
            _stackReserveSize = BitConverterLE.ToUInt32(data, offset); offset += 4;
            _stackCommitSize = BitConverterLE.ToUInt32(data, offset); offset += 4;
            _heapReserveSize = BitConverterLE.ToUInt32(data, offset); offset += 4;
            _heapCommitSize = BitConverterLE.ToUInt32(data, offset); offset += 4;
            offset += 4; // Reserved
        }
#endif
        void WritePE32Header(System.IO.BinaryWriter stream)
        {
            stream.Write((byte)_linkerVersion.Major);
            stream.Write((byte)_linkerVersion.Minor);
            uint totalTextSectionSize = 0;
            uint totalInitializedDataSize = 0;
            uint totalUninitializedDataSize = 0;
            if (_sections != null)
            {
                for (int n = 0; n < _sections.Length; n++)
                {
                    uint paddedSectionSize = (uint)_sections[n].Size;
                    paddedSectionSize = ((paddedSectionSize + _fileAlignment - 1) / _fileAlignment) * _fileAlignment;
                    if ((_sections[n].Characteristics & Section.Flag.Code) != 0)
                    {
                        totalTextSectionSize += paddedSectionSize;
                    }
                    if ((_sections[n].Characteristics & Section.Flag.InitializedData) != 0)
                    {
                        totalInitializedDataSize += paddedSectionSize;
                    }
                    if ((_sections[n].Characteristics & Section.Flag.UninitializedData) != 0)
                    {
                        totalUninitializedDataSize += paddedSectionSize;
                    }
                }
            }
            stream.Write((uint)totalTextSectionSize);
            stream.Write((uint)totalInitializedDataSize);
            stream.Write((uint)totalUninitializedDataSize);
            stream.Write(_entryPointRelativeVirtualAddress);
            stream.Write((int)BaseOfCode); // BaseOfCode
            stream.Write((int)BaseOfData); // BaseOfCode
            stream.Write((uint)_imageBase);
            stream.Write(_sectionAlignment); // SectionAlignment
            stream.Write(_fileAlignment); // FileAlignment
            stream.Write((short)_operatingSystemVersion.Major); // MajorOperatingSystemVersion
            stream.Write((short)_operatingSystemVersion.Minor); // MinorOperatingSystemVersion
            stream.Write((short)_imageVersion.Major); // MajorImageVersion
            stream.Write((short)_imageVersion.Minor); // MinorImageVersion
            stream.Write((short)_subsystemVersion.Major); // MajorSubsystemVersion
            stream.Write((short)_subsystemVersion.Minor); // MinorSubsystemVersion
            stream.Write((int)0); // Reserved


            stream.Write((uint)SizeOfImage); // SizeOfImage
            stream.Write((uint)SizeOfHeadersAligned); // SizeOfHeaders
            stream.Write((int)_checksum); // CheckSum
            stream.Write((short)_subsystem); // Subsystem
            stream.Write((ushort)_dllCharacteristicsFlags); // DllCharacteristicsDllCharacteristics
            stream.Write((uint)_stackReserveSize); // SizeOfStackReserve
            stream.Write((uint)_stackCommitSize); // SizeOfStackCommit
            stream.Write((uint)_heapReserveSize); // SizeOfHeapReserve
            stream.Write((uint)_heapCommitSize); // SizeOfHeapCommit
            stream.Write((int)0); // Reserved
        }
    }
}
