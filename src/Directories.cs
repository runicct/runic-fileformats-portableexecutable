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
        Directory _exportTable = new Directory(0, 0);
        public Directory ExportTable { get { return _exportTable; } set { _exceptionTable = value; } }
        Directory _importTable = new Directory(0, 0);
        public Directory ImportTable { get { return _importTable; } set { _importTable = value; } }
        Directory _resourceTable = new Directory(0, 0);
        public Directory ResourceTable { get { return _resourceTable; } set { _resourceTable = value; } }
        Directory _exceptionTable = new Directory(0, 0);
        public Directory ExceptionTable { get { return _exceptionTable; } set { _exceptionTable = value; } }
        Directory _certificateTable = new Directory(0, 0);
        public Directory CertificateTable { get { return _certificateTable; } set { _certificateTable = value; } }
        Directory _baseRelocationTable = new Directory(0, 0);
        public Directory BaseRelocationTable { get { return _baseRelocationTable; } set { _baseRelocationTable = value; } }
        Directory _debugDirectory = new Directory(0, 0);
        public Directory DebugDirectory { get { return _debugDirectory; } set { _debugDirectory = value; } }
        Directory _globalPointer = new Directory(0, 0);
        public Directory GlobalPointer { get { return _globalPointer; } set { _globalPointer = value; } }
        Directory _TLSTable = new Directory(0, 0);
        public Directory TLSTable { get { return _TLSTable; } set { _TLSTable = value; } }
        Directory _loadConfigurationTable = new Directory(0, 0);
        public Directory LoadConfigurationTable { get { return _loadConfigurationTable; } set { _loadConfigurationTable = value; } }
        Directory _boundImportTable = new Directory(0, 0);
        public Directory BoundImportTable { get { return _boundImportTable; } set { _boundImportTable = value; } }
        Directory _importAddressTable = new Directory(0, 0);
        public Directory ImportAddressTable { get { return _importAddressTable; } set { _importAddressTable = value; } }
        Directory _delayImportDescriptor = new Directory(0, 0);
        public Directory DelayImportDescriptor { get { return _delayImportDescriptor; } set { _delayImportDescriptor = value; } }
        Directory _CLIHeader = new Directory(0, 0);
        public Directory CLIHeader { get { return _CLIHeader; } set { _CLIHeader = value; } }


        void DecodeDirectories(System.IO.BinaryReader reader)
        {
            int numberOfRvaAndSizes = reader.ReadInt32();
            // Export Table
            if (numberOfRvaAndSizes > 0)
            {
                _exportTable.RelativeVirtualAddress = reader.ReadUInt32();
                _exportTable.Size = reader.ReadUInt32();
            }
            // Import Table
            if (numberOfRvaAndSizes > 1)
            {
                _importTable.RelativeVirtualAddress = reader.ReadUInt32();
                _importTable.Size = reader.ReadUInt32();
            }
            // Resource Table
            if (numberOfRvaAndSizes > 2)
            {
                _resourceTable.RelativeVirtualAddress = reader.ReadUInt32();
                _resourceTable.Size = reader.ReadUInt32();
            }
            // Exception Table
            if (numberOfRvaAndSizes > 3)
            {
                _exceptionTable.RelativeVirtualAddress = reader.ReadUInt32();
                _exceptionTable.Size = reader.ReadUInt32();
            }
            // Certificate Table
            if (numberOfRvaAndSizes > 4)
            {
                _certificateTable.RelativeVirtualAddress = reader.ReadUInt32();
                _certificateTable.Size = reader.ReadUInt32();
            }
            // Base Relocation Table
            if (numberOfRvaAndSizes > 5)
            {
                _baseRelocationTable.RelativeVirtualAddress = reader.ReadUInt32();
                _baseRelocationTable.Size = reader.ReadUInt32();
            }
            // Debug Table
            if (numberOfRvaAndSizes > 6)
            {
                _debugDirectory.RelativeVirtualAddress = reader.ReadUInt32();
                _debugDirectory.Size = reader.ReadUInt32();
            }
            // Architecture Table
            if (numberOfRvaAndSizes > 7)
            {
                reader.ReadUInt32();
                reader.ReadUInt32();
            }
            // Global Ptr Table
            if (numberOfRvaAndSizes > 8)
            {
                _globalPointer.RelativeVirtualAddress = reader.ReadUInt32();
                reader.ReadUInt32(); // Expected to be 0
            }
            // TLS Table
            if (numberOfRvaAndSizes > 9)
            {
                _TLSTable.RelativeVirtualAddress = reader.ReadUInt32();
                _TLSTable.Size = reader.ReadUInt32();
            }
            // Load Config Table
            if (numberOfRvaAndSizes > 10)
            {
                _loadConfigurationTable.RelativeVirtualAddress = reader.ReadUInt32();
                _loadConfigurationTable.Size = reader.ReadUInt32();
            }
            // Bound Import
            if (numberOfRvaAndSizes > 11)
            {
                _boundImportTable.RelativeVirtualAddress = reader.ReadUInt32();
                _boundImportTable.Size = reader.ReadUInt32();
            }
            // IAT
            if (numberOfRvaAndSizes > 12)
            {
                _importAddressTable.RelativeVirtualAddress = reader.ReadUInt32();
                _importAddressTable.Size = reader.ReadUInt32();
            }
            // Delay Import Descriptor
            if (numberOfRvaAndSizes > 13)
            {
                _delayImportDescriptor.RelativeVirtualAddress = reader.ReadUInt32();
                _delayImportDescriptor.Size = reader.ReadUInt32();
            }
            if (numberOfRvaAndSizes > 14)
            {
                uint s = CLIHeader.Size;
                _CLIHeader.RelativeVirtualAddress = reader.ReadUInt32();
                uint headerSize = reader.ReadUInt32();
                if (headerSize < 72) { _CLIHeader.RelativeVirtualAddress = 0; }
                else { _CLIHeader.Size = 72; }
                numberOfRvaAndSizes -= 15;
                while (numberOfRvaAndSizes > 0)
                {
                    reader.ReadUInt32();
                    reader.ReadUInt32();
                    numberOfRvaAndSizes--;
                }
            }
        }
        void DecodeDirectories(Span<byte> data, ref uint offset)
        {
            uint numberOfRvaAndSize = BitConverterLE.ToUInt32(data, offset); offset += 4;

            // Decode Entries

            // Export Table
            if (numberOfRvaAndSize > 0)
            {
                _exportTable.RelativeVirtualAddress = BitConverterLE.ToUInt32(data, offset); offset += 4;
                _exportTable.Size = BitConverterLE.ToUInt32(data, offset); offset += 4;
            }
            // Import Table
            if (numberOfRvaAndSize > 1)
            {
                _importTable.RelativeVirtualAddress = BitConverterLE.ToUInt32(data, offset); offset += 4;
                _importTable.Size = BitConverterLE.ToUInt32(data, offset); offset += 4;
            }
            // Resource Table
            if (numberOfRvaAndSize > 2)
            {
                _resourceTable.RelativeVirtualAddress = BitConverterLE.ToUInt32(data, offset); offset += 4;
                _resourceTable.Size = BitConverterLE.ToUInt32(data, offset); offset += 4;
            }
            // Exception Table
            if (numberOfRvaAndSize > 3)
            {
                _exceptionTable.RelativeVirtualAddress = BitConverterLE.ToUInt32(data, offset); offset += 4;
                _exceptionTable.Size = BitConverterLE.ToUInt32(data, offset); offset += 4;
            }
            // Certificate Table
            if (numberOfRvaAndSize > 4)
            {
                _certificateTable.RelativeVirtualAddress = BitConverterLE.ToUInt32(data, offset); offset += 4;
                _certificateTable.Size = BitConverterLE.ToUInt32(data, offset); offset += 4;
            }
            // Base Relocation Table
            if (numberOfRvaAndSize > 5)
            {
                _baseRelocationTable.RelativeVirtualAddress = BitConverterLE.ToUInt32(data, offset); offset += 4;
                _baseRelocationTable.Size = BitConverterLE.ToUInt32(data, offset); offset += 4;
            }
            // Debug Table
            if (numberOfRvaAndSize > 6)
            {
                _debugDirectory.RelativeVirtualAddress = BitConverterLE.ToUInt32(data, offset); offset += 4;
                _debugDirectory.Size = BitConverterLE.ToUInt32(data, offset); offset += 4;
            }
            // Architecture Table
            if (numberOfRvaAndSize > 7)
            {
                offset += 4;
                offset += 4;
            }
            // Global Ptr Table
            if (numberOfRvaAndSize > 8)
            {
                _globalPointer.RelativeVirtualAddress = BitConverterLE.ToUInt32(data, offset); offset += 4;
                offset += 4; // Expected to be 0
            }
            // TLS Table
            if (numberOfRvaAndSize > 9)
            {
                _TLSTable.RelativeVirtualAddress = BitConverterLE.ToUInt32(data, offset); offset += 4;
                _TLSTable.Size = BitConverterLE.ToUInt32(data, offset); offset += 4;
            }
            // Load Config Table
            if (numberOfRvaAndSize > 10)
            {
                _loadConfigurationTable.RelativeVirtualAddress = BitConverterLE.ToUInt32(data, offset); offset += 4;
                _loadConfigurationTable.Size = BitConverterLE.ToUInt32(data, offset); offset += 4;
            }
            // Bound Import
            if (numberOfRvaAndSize > 11)
            {
                _boundImportTable.RelativeVirtualAddress = BitConverterLE.ToUInt32(data, offset); offset += 4;
                _boundImportTable.Size = BitConverterLE.ToUInt32(data, offset); offset += 4;
            }
            // IAT
            if (numberOfRvaAndSize > 12)
            {
                _importAddressTable.RelativeVirtualAddress = BitConverterLE.ToUInt32(data, offset); offset += 4;
                _importAddressTable.Size = BitConverterLE.ToUInt32(data, offset); offset += 4;
            }
            // Delay Import Descriptor
            if (numberOfRvaAndSize > 13)
            {
                _delayImportDescriptor.RelativeVirtualAddress = BitConverterLE.ToUInt32(data, offset); offset += 4;
                _delayImportDescriptor.Size = BitConverterLE.ToUInt32(data, offset); offset += 4;
            }
            if (numberOfRvaAndSize > 14)
            {
                _CLIHeader.RelativeVirtualAddress = BitConverterLE.ToUInt32(data, offset); offset += 4;
                uint headerSize = BitConverterLE.ToUInt32(data, offset); offset += 4;
                if (headerSize < 72) { _CLIHeader.RelativeVirtualAddress = 0; }
                else { _CLIHeader.Size = 72; }
                numberOfRvaAndSize -= 15;
                while (numberOfRvaAndSize > 0)
                {
                    offset += 4;
                    offset += 4;
                    numberOfRvaAndSize--;
                }
            }
        }
        void EncodeDirectories(System.IO.BinaryWriter writer)
        {
            writer.Write((int)16); // Number of RVA and Sizes

            // Encode Entries
            if (_exportTable.RelativeVirtualAddress != 0)
            {
                writer.Write(_exportTable.RelativeVirtualAddress); writer.Write(_exportTable.Size); // Export Table
            }
            else
            {
                writer.Write((int)0); writer.Write((int)0); // Export Table
            }
            if (_importTable.RelativeVirtualAddress != 0)
            {
                writer.Write(_importTable.RelativeVirtualAddress); writer.Write(_importTable.Size); // Import Table
            }
            else
            {
                writer.Write((int)0); writer.Write((int)0); // Import Table
            }
            if (_resourceTable.RelativeVirtualAddress != 0)
            {
                writer.Write(_resourceTable.RelativeVirtualAddress); writer.Write(_resourceTable.Size); // Resource Table
            }
            else
            {
                writer.Write((int)0); writer.Write((int)0); // Resource Table
            }
            if (_exceptionTable.RelativeVirtualAddress != 0)
            {
                writer.Write(_exceptionTable.RelativeVirtualAddress); writer.Write(_exceptionTable.Size); // Exception Table
            }
            else
            {
                writer.Write((int)0); writer.Write((int)0); // Exception Table
            }
            if (_certificateTable.RelativeVirtualAddress != 0)
            {
                writer.Write(_certificateTable.RelativeVirtualAddress); writer.Write(_certificateTable.Size); // Certificate Table
            }
            else
            {
                writer.Write((int)0); writer.Write((int)0); // Certificate Table
            }
            if (_baseRelocationTable.RelativeVirtualAddress != 0)
            {
                writer.Write(_baseRelocationTable.RelativeVirtualAddress); writer.Write(_baseRelocationTable.Size); // Base Relocation Table
            }
            else
            {
                writer.Write((int)0); writer.Write((int)0); // Base Relocation Table
            }
            if (_debugDirectory.RelativeVirtualAddress != 0)
            {
                writer.Write(_debugDirectory.RelativeVirtualAddress); writer.Write(_debugDirectory.Size); // Debug Table
            }
            else
            {
                writer.Write((int)0); writer.Write((int)0); // Debug Table
            }
            writer.Write((int)0); writer.Write((int)0); // Architecture (Always 0 for PE32/PE32+)

            if (_globalPointer.RelativeVirtualAddress != 0)
            {
                writer.Write(_globalPointer.RelativeVirtualAddress); writer.Write(_globalPointer.Size); // Global Pointer
            }
            else
            {
                writer.Write((int)0); writer.Write((int)0); // Global Pointer
            }

            if (_TLSTable.RelativeVirtualAddress != 0)
            {
                writer.Write(_TLSTable.RelativeVirtualAddress); writer.Write(_TLSTable.Size); // TLS Table
            }
            else
            {
                writer.Write((int)0); writer.Write((int)0); // TLS Table
            }
            if (_loadConfigurationTable.RelativeVirtualAddress != 0)
            {
                writer.Write(_loadConfigurationTable.RelativeVirtualAddress); writer.Write(_loadConfigurationTable.Size); // Load Config Table
            }
            else
            {
                writer.Write((int)0); writer.Write((int)0); // Load Config Table
            }
            if (_boundImportTable.RelativeVirtualAddress != 0)
            {
                writer.Write(_boundImportTable.RelativeVirtualAddress); writer.Write(_boundImportTable.Size); // Bound Import
            }
            else
            {
                writer.Write((int)0); writer.Write((int)0); // Bound Import
            }
            if (_importAddressTable.RelativeVirtualAddress != 0)
            {
                writer.Write(_importAddressTable.RelativeVirtualAddress); writer.Write(_importAddressTable.Size); // IAT
            }
            else
            {
                writer.Write((int)0); writer.Write((int)0); // IAT
            }
            if (_delayImportDescriptor.RelativeVirtualAddress != 0)
            {
                writer.Write(_delayImportDescriptor.RelativeVirtualAddress); writer.Write(_delayImportDescriptor.Size); // Delay Import Descriptor
            }
            else
            {
                writer.Write((int)0); writer.Write((int)0); // Delay Import Descriptor
            }
            if (_CLIHeader.RelativeVirtualAddress != 0)
            {
                writer.Write((int)_CLIHeader.RelativeVirtualAddress); writer.Write((int)72); // CLR Runtime Header
            }
            else
            {
                writer.Write((int)0); writer.Write((int)0);
            }

            writer.Write((int)0); writer.Write((int)0); // Reserved
        }
    }
}
