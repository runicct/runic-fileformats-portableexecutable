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
using System.IO;

namespace Runic.FileFormats
{
    public partial class PortableExecutable
    {
        Version _linkerVersion = new Version(1, 0);
        public Version LinkerVersion { get { return _linkerVersion; } set { _linkerVersion = value; } }
        Version _operatingSystemVersion = new Version(4, 0);
        public Version OperatingSystemVersion { get { return _operatingSystemVersion; } set { _operatingSystemVersion = value; } }
        Version _imageVersion = new Version(1, 0);
        public Version ImageVersion { get { return _imageVersion; } set { _imageVersion = value; } }
        Version _subsystemVersion = new Version(4, 0);
        public Version SubsystemVersion { get { return _subsystemVersion; } set { _subsystemVersion = value; } }
        uint _checksum = 0;
        public uint Checksum { get { return _checksum; } set { _checksum = value; } }
        public enum ImageSubsystem
        {
            Unknown = 0,
            Native = 1,
            WindowsGui = 2,
            WindowsCui = 3,
            PosixCui = 7,
            WindowsCeGui = 9,
            EfiApplication = 10,
            EfiBootServiceDriver = 11,
            EfiRuntimeDriver = 12,
            EfiRom = 13,
            Xbox = 14,
            WindowsBootApplication = 14,
        }
        ImageSubsystem _subsystem = ImageSubsystem.WindowsCui;
        public ImageSubsystem Subsystem { get { return _subsystem; } set { _subsystem = value; } }
        public enum DllCharacteristics : ushort
        {
            None = 0x0,
            HighEntropyVa = 0x0020,
            DynamicBase = 0x0040,
            NxCompat = 0x0100,
            NoSeh = 0x0400,
            TerminalServerAware = 0x8000,
        }

        DllCharacteristics _dllCharacteristicsFlags = DllCharacteristics.HighEntropyVa | DllCharacteristics.DynamicBase | DllCharacteristics.NxCompat | DllCharacteristics.NoSeh | DllCharacteristics.TerminalServerAware;
        public DllCharacteristics Characteristics { get { return _dllCharacteristicsFlags; } set { _dllCharacteristicsFlags = value; } }
        ulong _stackReserveSize = 0x100000;
        public ulong StackReserveSize { get { return _stackReserveSize; } set { _stackReserveSize = value; } }
        ulong _stackCommitSize = 0x1000;
        public ulong StackCommitSize { get { return _stackCommitSize; } set { _stackCommitSize = value; } }
        ulong _heapReserveSize = 0x100000;
        public ulong HeapReserveSize { get { return _heapReserveSize; } set { _heapReserveSize = value; } }
        ulong _heapCommitSize = 0x1000;
        public ulong HeapCommitSize { get { return _heapCommitSize; } set { _heapCommitSize = value; } }
        public static uint PE32PlusHeaderSize { get { return 240; } }
        public static uint PE32HeaderSize { get { return 224; } }
        bool _isPE32Plus;
        public bool IsPE32Plus { get { return _isPE32Plus; } set { _isPE32Plus = value; } }
        uint _entryPointRelativeVirtualAddress = 0;
        public uint EntryPointRelativeVirtualAddress
        {
            get { return _entryPointRelativeVirtualAddress; }
            set { _entryPointRelativeVirtualAddress = value; }
        }
        ulong _imageBase = 0x10000000;
        public ulong ImageBase { get { return _imageBase; } set { _imageBase = value; } }
        public static uint COFFHeaderSize { get { return 20; } }

        uint SizeOfHeaders
        {
            get
            {
                return (uint)(DosHeader.DOSHeaderAndDOSStubSize + 4 /* COFF Signature */ + COFFHeaderSize + (_isPE32Plus ? PE32PlusHeaderSize : PE32HeaderSize) + SectionTableSize);
            }
        }
        uint SizeOfHeadersAligned
        {
            get
            {
                return ((SizeOfHeaders + _fileAlignment - 1) / _fileAlignment) * _fileAlignment;
            }
        }
        void ReadSectionData(System.IO.BinaryReader stream)
        {
            for (int n = 0; n < _sections.Length; n++)
            {
#if NET6_0_OR_GREATER
                ImportedSection? importedSection = _sections[n] as ImportedSection;
#else
                ImportedSection importedSection = _sections[n] as ImportedSection;
#endif
                if (importedSection != null && importedSection.SizeOnDisk > 0)
                {
                    stream.BaseStream.Seek(importedSection.AddressOnDisk, SeekOrigin.Begin);
                    byte[] data = new byte[importedSection.SizeOnDisk];
                    stream.BaseStream.Read(data, 0, data.Length);
                    importedSection.SetData(data);
                }
            }
        }
        void ReadPE32OrPE32PlusHeader(System.IO.BinaryReader stream, uint optionalHeaderSize)
        {
            ushort magik = stream.ReadUInt16();
            if (magik == 0x20b)
            {
                _isPE32Plus = true;
                if (optionalHeaderSize < PE32PlusHeaderSize) { throw new Exception("Invalid PE File: The optional header is too small for a PE32+"); }
                ReadPE32PlusHeader(stream);
            }
            else if (magik == 0x10b)
            {
                if (optionalHeaderSize < PE32HeaderSize) { throw new Exception("Invalid PE File: The optional header is too small for a PE32"); }
                ReadPE32Header(stream);
            }
            else { throw new Exception("Invalid PE File: Unknown magic number expected 0x20B (PE32+) or 0x10B (PE32)"); }
            DecodeDirectories(stream);
        }
        public static void Load(System.IO.BinaryReader stream)
        {
            DosHeader.ReadDOSHeader(stream);
            uint peCoffMagicNumber = stream.ReadUInt32();
            if (peCoffMagicNumber != 0x00004550) { throw new Exception("Invalid PE File: Missing the magic number after the DOS Stub"); }
            PortableExecutable portableExecutable = new PortableExecutable();
            uint optionalHeaderSize = 0;
            portableExecutable.ReadCoffHeader(stream, out optionalHeaderSize);
            if (optionalHeaderSize < 2) { throw new Exception("Invalid PE File: The optional header is too small"); }
            portableExecutable.ReadPE32OrPE32PlusHeader(stream, optionalHeaderSize);
            portableExecutable.ReadSectionTable(stream);
            portableExecutable.ReadSectionData(stream);
        }


#if NET6_0_OR_GREATER
        void ReadSectionData(Span<byte> data)
        {
            for (int n = 0; n < _sections.Length; n++)
            {
                ImportedSection? importedSection = _sections[n] as ImportedSection;
                if (importedSection != null && importedSection.SizeOnDisk > 0)
                {
                    byte[] copyData = new byte[importedSection.SizeOnDisk];
                    for (int x = 0; x < importedSection.SizeOnDisk; x++)
                    {
                        copyData[x] = data[(int)(importedSection.AddressOnDisk + x)];
                    }
                    importedSection.SetData(copyData);
                }
            }
        }
        void ReadPE32OrPE32PlusHeader(Span<byte> data, ref uint offset, uint optionalHeaderSize)
        {
            ushort magik = BitConverterLE.ToUInt16(data, offset); offset += 2;
            if (magik == 0x20b)
            {
                _isPE32Plus = true;
                if (optionalHeaderSize < PE32PlusHeaderSize) { throw new Exception("Invalid PE File: The optional header is too small for a PE32+"); }
                ReadPE32PlusHeader(data, ref offset);
            }
            else if (magik == 0x10b)
            {
                if (optionalHeaderSize < PE32HeaderSize) { throw new Exception("Invalid PE File: The optional header is too small for a PE32"); }
                ReadPE32Header(data, ref offset);
            }
            else { throw new Exception("Invalid PE File"); }
            DecodeDirectories(data, ref offset);
        }
        public static void Load(Span<byte> data, uint offset)
        {
            DosHeader.ReadDOSHeader(data, ref offset);
            uint peCoffMagicNumber = BitConverterLE.ToUInt32(data, offset); offset += 4;
            if (peCoffMagicNumber != 0x00004550) { throw new Exception("Invalid PE File: Missing the magic number after the DOS Stub"); }
            PortableExecutable portableExecutable = new PortableExecutable();
            uint optionalHeaderSize = 0;
            portableExecutable.ReadCoffHeader(data, ref offset, out optionalHeaderSize);
            if (optionalHeaderSize < 2) { throw new Exception("Invalid PE File: The optional header is too small"); }
            portableExecutable.ReadPE32OrPE32PlusHeader(data, ref offset, optionalHeaderSize);
            portableExecutable.ReadSectionTable(data, ref offset);
            portableExecutable.ReadSectionData(data);
        }
#endif
        void WritePE32OrPE32PlusHeader(System.IO.BinaryWriter stream)
        {
            if (_isPE32Plus)
            {
                stream.Write((ushort)0x20b);
                WritePE32PlusHeader(stream);
            }
            else
            {
                stream.Write((ushort)0x10b);
                WritePE32Header(stream);
            }
            EncodeDirectories(stream);
        }
        void WriteSectionData(System.IO.BinaryWriter stream)
        {
            uint padding = SizeOfHeadersAligned - SizeOfHeaders;
            for (uint n = 0; n < padding; n++) { stream.Write((byte)0); }
            for (int n = 0; n < _sections.Length; n++)
            {
#if NET6_0_OR_GREATER
                byte[]? data = _sections[n].GetData();
#else
                byte[] data = _sections[n].GetData();
#endif
                if (data != null)
                {
                    uint paddedSectionSize = (uint)data.LongLength;
                    paddedSectionSize = ((paddedSectionSize + _fileAlignment - 1) / _fileAlignment) * _fileAlignment;
                    padding = paddedSectionSize - (uint)data.LongLength;
                    stream.Write(data);
                    for (uint x = 0; x < padding; x++) { stream.Write((byte)0); }
                }
            }
        }
        public void Save(System.IO.BinaryWriter stream)
        {
            DosHeader.WriteDOSHeader(stream);
            stream.Write((uint)0x00004550);
            WriteCoffHeader(stream);
            WritePE32OrPE32PlusHeader(stream);
            WriteSectionTable(stream);
            WriteSectionData(stream);
        }

        /// <summary>
        /// Find the section associated with the supplied RVA or null if the RVA belongs to no
        /// section.
        /// </summary>
        /// <param name="rva">The RVA to be located</param>
        /// <returns>The section that contains the RVA or null if the RVA does not fall in any section</returns>
#if NET6_0_OR_GREATER
        public Section? FindSectionFromRelativeVirtualAddress(uint relativeVirtualAddress)
#else
        public Section FindSectionFromRelativeVirtualAddress(uint relativeVirtualAddress)
#endif
        {
            for (int n = 0; n < _sections.Length; n++)
            {
                uint sectionBase = _sections[n].RelativeVirtualAddress;
                uint sectionLimit = sectionBase + _sections[n].Size;
                if (sectionBase <= relativeVirtualAddress && relativeVirtualAddress < sectionLimit)
                {
                    return _sections[n];
                }
            }
            return null;
        }

#if NET6_0_OR_GREATER
        public Span<byte> GetSpanAtRelativeVirtualAddress(uint relativeVirtualAddress, uint length)
        {
            Section? section = FindSectionFromRelativeVirtualAddress(relativeVirtualAddress);
            if (section == null) { return Span<byte>.Empty; }
            uint offset = relativeVirtualAddress - section.RelativeVirtualAddress;
            byte[]? sectionData = section.GetData();
            if (sectionData == null) { return Span<byte>.Empty; }
            uint end = (uint)(offset + length);
            if (end >= sectionData.Length) { end = (uint)(sectionData.Length - 1); }
            length = end - offset;
            return new Span<byte>(sectionData, (int)offset, (int)length);
        }
#endif
#if NET6_0_OR_GREATER
        public byte[]? ReadArrayAtRelativeVirtualAddress(uint relativeVirtualAddress, uint length)
#else
        public byte[] ReadArrayAtRelativeVirtualAddress(uint relativeVirtualAddress, uint length)
#endif
        {
#if NET6_0_OR_GREATER
            Section? section = FindSectionFromRelativeVirtualAddress(relativeVirtualAddress);
#else
            Section section = FindSectionFromRelativeVirtualAddress(relativeVirtualAddress);
#endif
            if (section == null) { return null; }
            uint offset = relativeVirtualAddress - section.RelativeVirtualAddress;
#if NET6_0_OR_GREATER
            byte[]? sectionData = section.GetData();
#else
            byte[] sectionData = section.GetData();
#endif
            if (sectionData == null) { return null; }
            uint end = (uint)(offset + length);
            if (end >= sectionData.Length) { end = (uint)(sectionData.Length - 1); }
            length = end - offset;
            byte[] data = new byte[length];
            for (uint n = 0; n < length; n++)
            {
                data[n] = sectionData[n + offset];
            }
            return data;
        }
#if NET6_0_OR_GREATER
        public string? ReadUTF8StringAtRelativeVirtualAddress(uint rva)
#else
        public string ReadUTF8StringAtRelativeVirtualAddress(uint rva)
#endif
        {
#if NET6_0_OR_GREATER
            Section? section = FindSectionFromRelativeVirtualAddress(rva);
#else
            Section section = FindSectionFromRelativeVirtualAddress(rva);
#endif
            if (section == null) { return null; }
            uint offset = rva - section.RelativeVirtualAddress;
#if NET6_0_OR_GREATER
            byte[]? sectionData = section.GetData();
#else
            byte[] sectionData = section.GetData();
#endif
            if (sectionData == null) { return null; }
            for (uint n = offset; n < sectionData.Length; n++)
            {
                if (sectionData[n] == 0)
                {
                    return System.Text.Encoding.UTF8.GetString(sectionData, (int)offset, (int)(n - offset));
                }
            }
            return System.Text.Encoding.UTF8.GetString(sectionData, (int)offset, (int)(sectionData.Length - offset));
        }
        public ushort ReadUInt16AtRelativeVirtualAddress(uint rva)
        {
#if NET6_0_OR_GREATER
            Section? section = FindSectionFromRelativeVirtualAddress(rva);
#else
            Section section = FindSectionFromRelativeVirtualAddress(rva);
#endif
            if (section == null) { return 0; }
#if NET6_0_OR_GREATER
            byte[]? sectionData = section.GetData();
#else
            byte[] sectionData = section.GetData();
#endif
            if (sectionData == null) { return 0; }
            uint offset = rva - section.RelativeVirtualAddress;
            if (offset + 2 >= sectionData.Length) { return 0; }
            return BitConverterLE.ToUInt16(sectionData, offset);
        }
        public uint ReadUInt32AtRelativeVirtualAddress(uint rva)
        {
#if NET6_0_OR_GREATER
            Section? section = FindSectionFromRelativeVirtualAddress(rva);
#else
            Section section = FindSectionFromRelativeVirtualAddress(rva);
#endif
            if (section == null) { return 0; }
#if NET6_0_OR_GREATER
            byte[]? sectionData = section.GetData();
#else
            byte[] sectionData = section.GetData();
#endif
            if (sectionData == null) { return 0; }
            uint offset = rva - section.RelativeVirtualAddress;
            if (offset + 4 >= sectionData.Length) { return 0; }
            return BitConverterLE.ToUInt32(sectionData, offset);
        }
        public ulong ReadUInt64AtRelativeVirtualAddress(uint rva)
        {
#if NET6_0_OR_GREATER
            Section? section = FindSectionFromRelativeVirtualAddress(rva);
#else
            Section section = FindSectionFromRelativeVirtualAddress(rva);
#endif
            if (section == null) { return 0; }
#if NET6_0_OR_GREATER
            byte[]? sectionData = section.GetData();
#else
            byte[] sectionData = section.GetData();
#endif
            if (sectionData == null) { return 0; }
            uint offset = rva - section.RelativeVirtualAddress;
            if (offset + 8 >= sectionData.Length) { return 0; }
            return BitConverterLE.ToUInt64(sectionData, offset);
        }
    }
}
