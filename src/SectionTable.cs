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
        uint _sectionAlignment = 4096;
        public uint SectionAlignment { get { return _sectionAlignment; } set { _sectionAlignment = value; } }
        uint _fileAlignment = 512;
        public uint FileAlignment { get { return _fileAlignment; } set { _fileAlignment = value; } }
        public static uint SectionHeaderSize { get { return 40; } }
        public uint SectionTableSize { get { return (uint)(SectionHeaderSize * _sections.Length); } }
        Section[] _sections;
        public Section[] Sections { get { return _sections; } set { _sections = value; } }
        class ImportedSection : Section
        {
#if NET6_0_OR_GREATER
            byte[]? _data;
#else
            byte[] _data;
#endif
            internal void SetData(byte[] data)
            {
                _data = data;
            }
            uint _addressOnDisk;
            public uint AddressOnDisk { get { return _addressOnDisk; } }
            uint _sizeOnDisk;
            public uint SizeOnDisk { get { return _sizeOnDisk; } }
            public ImportedSection(string name, uint relativeVirtualAddress, uint size, uint addressOnDisk, uint sizeOnDisk, Flag characteristics) : base(name, relativeVirtualAddress, size, characteristics)
            {
                _addressOnDisk = addressOnDisk;
                _sizeOnDisk = sizeOnDisk;
            }
#if NET6_0_OR_GREATER
            public override Span<byte> GetDataSpan()
            {
                return new Span<byte>(_data);
            }
#endif
#if NET6_0_OR_GREATER
            public override byte[]? GetData()
#else
            public override byte[] GetData()
#endif
            {
                return _data;
            }
        }
        void ReadSectionTable(System.IO.BinaryReader stream)
        {
            for (int n = 0; n < _sections.Length; n++)
            {
                byte[] name = stream.ReadBytes(8);
                string nameStr = "";
                for (int x = 0; x < name.Length && name[x] != 0; x++)
                {
                    nameStr += (char)name[x];
                }
                uint sectionSize = stream.ReadUInt32();
                uint sectionRVA = stream.ReadUInt32();
                uint sectionSizeOnDisk = stream.ReadUInt32();
                uint sectionAddressOnDisk = stream.ReadUInt32();
                stream.ReadUInt32(); // PointerToRelocations
                stream.ReadUInt32(); // PointerToLinenumbers
                stream.ReadUInt16(); // NumberOfRelocations
                stream.ReadUInt16(); // NumberOfLinenumbers

                uint characteristics = stream.ReadUInt32();
                _sections[n] = new ImportedSection(nameStr, sectionRVA, sectionSize, sectionAddressOnDisk, sectionSizeOnDisk, (Section.Flag)characteristics);
            }
        }
#if NET6_0_OR_GREATER
        void ReadSectionTable(Span<byte> data, ref uint offset)
        {
            for (int n = 0; n < _sections.Length; n++)
            {
                byte[] name = new byte[8];
                name[0] = data[(int)offset]; offset++;
                name[1] = data[(int)offset]; offset++;
                name[2] = data[(int)offset]; offset++;
                name[3] = data[(int)offset]; offset++;
                name[4] = data[(int)offset]; offset++;
                name[5] = data[(int)offset]; offset++;
                name[6] = data[(int)offset]; offset++;
                name[7] = data[(int)offset]; offset++;
                string nameStr = "";
                for (int x = 0; x < name.Length && name[x] != 0; x++)
                {
                    nameStr += (char)name[x];
                }
                uint sectionSize = BitConverterLE.ToUInt32(data, offset); offset += 4;
                uint sectionRVA = BitConverterLE.ToUInt32(data, offset); offset += 4;
                uint sectionSizeOnDisk = BitConverterLE.ToUInt32(data, offset); offset += 4;
                uint sectionAddressOnDisk = BitConverterLE.ToUInt32(data, offset); offset += 4;
                offset += 4; // PointerToRelocations
                offset += 4; // PointerToLinenumbers
                offset += 2; // NumberOfRelocations
                offset += 2; // NumberOfLinenumbers

                uint characteristics = BitConverterLE.ToUInt32(data, offset); offset += 4;
                _sections[n] = new ImportedSection(nameStr, sectionRVA, sectionSize, sectionAddressOnDisk, sectionSizeOnDisk, (Section.Flag)characteristics);
            }
        }
#endif
        void WriteSectionTable(System.IO.BinaryWriter stream)
        {
            uint addressOfRawDataOnDisk = SizeOfHeadersAligned;

            for (int n = 0; n < _sections.Length; n++)
            {
                {
                    int x = 0;
                    byte[] name = System.Text.Encoding.UTF8.GetBytes(_sections[n].Name);
                    for (; x < name.Length && x < 8; x++) { stream.Write(name[x]); }
                    for (; x < 8; x++) { stream.Write((byte)0); }
                }
                stream.Write((uint)_sections[n].Size);
                stream.Write((uint)_sections[n].RelativeVirtualAddress);

#if NET6_0_OR_GREATER
                byte[]? data = _sections[n].GetData();
#else
                byte[] data = _sections[n].GetData();
#endif
                if (data != null)
                {
                    uint sectionSizeOnDisk = (uint)data.LongLength;
                    sectionSizeOnDisk = ((sectionSizeOnDisk + _fileAlignment - 1) / _fileAlignment) * _fileAlignment;
                    stream.Write((uint)sectionSizeOnDisk);
                    stream.Write((uint)addressOfRawDataOnDisk);
                    addressOfRawDataOnDisk += sectionSizeOnDisk;
                }
                else
                {
                    stream.Write((uint)0);
                    stream.Write((uint)0);
                }

                stream.Write((uint)0); // PointerToRelocations
                stream.Write((uint)0); // PointerToLinenumbers
                stream.Write((short)0); // NumberOfRelocations
                stream.Write((short)0); // NumberOfLinenumbers

                stream.Write((uint)_sections[n].Characteristics); // Characteristics
            }
        }
    }
}
