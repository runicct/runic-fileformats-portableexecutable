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
        public static partial class Directories
        {
            public class ExportTable
            {
                public class Symbol
                {
#if NET6_0_OR_GREATER
                    string? _name;
                    public string? Name { get { return _name; } }
#else
                    string _name;
                    public string Name { get { return _name; } }
#endif
                    uint _address;
                    public uint Address { get { return _address; } }
#if NET6_0_OR_GREATER
                    public Symbol(string? name, uint address)
#else
                    public Symbol(string name, uint address)
#endif
                    {
                        _name = name;
                    }
                    public override string ToString()
                    {
                        return _name != null ? _name : "<0x" + _address.ToString("X") + ">";
                    }
                }
                uint _flags = 0;
                public uint Flags { get { return _flags; } set { _flags = value; } }
                DateTime _timestamp = DateTime.UtcNow;
                public DateTime Timestamp { get { return _timestamp; } set { _timestamp = value; } }

                Version _version;
                public Version Version { get { return _version; } set { _version = value; } }
                public static uint ExportDirectoryTableSize { get { return 40; } }

                uint _exportOrdinalBase = 0x1;
                public uint ExportOrdinalBase { get { return _exportOrdinalBase; } }
                uint _exportAddressTableRVA = 0x0;
                public uint ExportAddressTableRVA { get { return _exportAddressTableRVA; } }
                uint _exportNameTableRVA = 0x0;
                public uint ExportNameTableRVA { get { return _exportNameTableRVA; } }
                uint _exportOrdinalTableRVA = 0x0;
                public uint ExportOrdinalTableRVA { get { return _exportOrdinalTableRVA; } }
                uint _imageNameRVA = 0x0;
                public uint ImageNameRVA { get { return _imageNameRVA; } set { _imageNameRVA = value; } }
                uint _RVA = 0;
                Symbol[] _symbols;
                public Symbol[] Symbols { get { return _symbols; } }
                internal void LoadFromArray(byte[] data, uint index, PortableExecutable pe)
                {
                    _flags = BitConverterLE.ToUInt32(data, index); index += 4;
                    _timestamp = ToDatetime(BitConverterLE.ToUInt32(data, index)); index += 4;
                    ushort versionMinor = BitConverterLE.ToUInt16(data, index); index += 2;
                    ushort versionMajor = BitConverterLE.ToUInt16(data, index); index += 2;
                    _version = new Version(versionMajor, versionMinor);
                    _imageNameRVA = BitConverterLE.ToUInt32(data, index); index += 4;
                    _exportOrdinalBase = BitConverterLE.ToUInt32(data, index); index += 4;
                    uint symbolCount = BitConverterLE.ToUInt32(data, index); index += 4;
                    uint namedSymbolCount = BitConverterLE.ToUInt32(data, index); index += 4;
                    _exportAddressTableRVA = BitConverterLE.ToUInt32(data, index); index += 4;
                    _exportNameTableRVA = BitConverterLE.ToUInt32(data, index); index += 4;
                    _exportOrdinalTableRVA = BitConverterLE.ToUInt32(data, index); index += 4;
#if NET6_0_OR_GREATER
                    Span<byte> exportAddressTable = pe.GetSpanAtRelativeVirtualAddress(_exportAddressTableRVA, 4 * symbolCount);
                    Span<byte> exportOrdinalTable = pe.GetSpanAtRelativeVirtualAddress(_exportOrdinalTableRVA, 4 * namedSymbolCount);
                    Span<byte> exportNameTable = pe.GetSpanAtRelativeVirtualAddress(_exportNameTableRVA, 4 * namedSymbolCount);
                    string?[] names = new string?[namedSymbolCount];
#else
                    byte[] exportAddressTable = pe.ReadArrayAtRelativeVirtualAddress(_exportAddressTableRVA, 4 * symbolCount);
                    byte[] exportOrdinalTable = pe.ReadArrayAtRelativeVirtualAddress(_exportOrdinalTableRVA, 4 * namedSymbolCount);
                    byte[] exportNameTable = pe.ReadArrayAtRelativeVirtualAddress(_exportNameTableRVA, 4 * namedSymbolCount);
                    string[] names = new string[namedSymbolCount];
#endif
                    _symbols = new Symbol[symbolCount];
                    for (int n = 0; n < symbolCount; n++)
                    {
                        _symbols[n] = null;
                    }
                    for (int n = 0; n < namedSymbolCount; n++)
                    {
                        uint ordinal = BitConverterLE.ToUInt16(exportOrdinalTable, (uint)(n * 2));
                        uint address = BitConverterLE.ToUInt32(exportAddressTable, ordinal);
                        _symbols[ordinal] = new Symbol(pe.ReadUTF8StringAtRelativeVirtualAddress(BitConverterLE.ToUInt32(exportNameTable, (uint)(n * 4))), address);
                    }
                    for (int n = 0; n < namedSymbolCount; n++)
                    {
                        if (_symbols[n] == null)
                        {
                            _symbols[n] = new Symbol(null, BitConverterLE.ToUInt32(exportAddressTable, (uint)(n * 4)));
                        }
                    }
                }
#if NET6_0_OR_GREATER
                internal void LoadFromSpan(Span<byte> data, uint index, PortableExecutable pe)
                {
                    _flags = BitConverterLE.ToUInt32(data, index); index += 4;
                    _timestamp = ToDatetime(BitConverterLE.ToUInt32(data, index)); index += 4;
                    ushort versionMinor = BitConverterLE.ToUInt16(data, index); index += 2;
                    ushort versionMajor = BitConverterLE.ToUInt16(data, index); index += 2;
                    _version = new Version(versionMajor, versionMinor);
                    _imageNameRVA = BitConverterLE.ToUInt32(data, index); index += 4;
                    _exportOrdinalBase = BitConverterLE.ToUInt32(data, index); index += 4;
                    uint symbolCount = BitConverterLE.ToUInt32(data, index); index += 4;
                    uint namedSymbolCount = BitConverterLE.ToUInt32(data, index); index += 4;
                    _exportAddressTableRVA = BitConverterLE.ToUInt32(data, index); index += 4;
                    _exportNameTableRVA = BitConverterLE.ToUInt32(data, index); index += 4;
                    _exportOrdinalTableRVA = BitConverterLE.ToUInt32(data, index); index += 4;

                    Span<byte> exportAddressTable = pe.GetSpanAtRelativeVirtualAddress(_exportAddressTableRVA, 4 * symbolCount);
                    Span<byte> exportOrdinalTable = pe.GetSpanAtRelativeVirtualAddress(_exportOrdinalTableRVA, 4 * namedSymbolCount);
                    Span<byte> exportNameTable = pe.GetSpanAtRelativeVirtualAddress(_exportNameTableRVA, 4 * namedSymbolCount);

                    string?[] names = new string?[namedSymbolCount];
                    _symbols = new Symbol[symbolCount];
                    for (int n = 0; n < symbolCount; n++)
                    {
                        _symbols[n] = null;
                    }
                    for (int n = 0; n < namedSymbolCount; n++)
                    {
                        uint ordinal = BitConverterLE.ToUInt16(exportOrdinalTable, (uint)(n * 2));
                        uint address = BitConverterLE.ToUInt32(exportAddressTable, ordinal);
                        _symbols[ordinal] = new Symbol(pe.ReadUTF8StringAtRelativeVirtualAddress(BitConverterLE.ToUInt32(exportNameTable, (uint)(n * 4))), address);
                    }
                    for (int n = 0; n < namedSymbolCount; n++)
                    {
                        if (_symbols[n] == null)
                        {
                            _symbols[n] = new Symbol(null, BitConverterLE.ToUInt32(exportAddressTable, (uint)(n * 4)));
                        }
                    }
                }
#endif
                public static ExportTable Load(byte[] data, uint startIndex, PortableExecutable portableExecutable)
                {
                    ExportTable exportTable = new ExportTable();
                    exportTable.LoadFromArray(data, startIndex, portableExecutable);
                    return exportTable;
                }
#if NET6_0_OR_GREATER
                public static ExportTable Load(Span<byte> data, uint startIndex, PortableExecutable portableExecutable)
                {
                    ExportTable exportTable = new ExportTable();
                    exportTable.LoadFromSpan(data, startIndex, portableExecutable);
                    return exportTable;
                }
#endif
#if NET6_0_OR_GREATER
                public static ExportTable? Load(PortableExecutable portableExecutable, uint exportTableRelativeVirtualAddress)
#else
                public static ExportTable Load(PortableExecutable portableExecutable, uint exportTableRelativeVirtualAddress)
#endif
                {
#if NET6_0_OR_GREATER
                    Span<byte> exportDirectoryTable = portableExecutable.GetSpanAtRelativeVirtualAddress(exportTableRelativeVirtualAddress, ExportDirectoryTableSize);
                    if (exportDirectoryTable.IsEmpty) { return null; }
#else
                    byte[] exportDirectoryTable = portableExecutable.ReadArrayAtRelativeVirtualAddress(exportTableRelativeVirtualAddress, ExportDirectoryTableSize);
                    if (exportDirectoryTable == null) { return null; }
#endif
                    return Load(exportDirectoryTable, 0, portableExecutable);
                }
#if NET6_0_OR_GREATER

                public static ExportTable? Load(PortableExecutable portableExecutable)
#else
                public static ExportTable Load(PortableExecutable portableExecutable)
#endif
                {
                    if (portableExecutable.ExportTable.RelativeVirtualAddress == 0 || portableExecutable.ExportTable.Size == 0) { return null; }
                    return Load(portableExecutable, portableExecutable.ExportTable.RelativeVirtualAddress);
                }

                public void Save(System.IO.BinaryWriter binaryWriter)
                {
                    uint namedSymbolCount = 0;
                    for (int n = 0; n < _symbols.Length; n++)
                    {
                        if (_symbols[n].Name != null) { namedSymbolCount++; }
                    }

                    Symbol[] namedSymbols = new Symbol[namedSymbolCount];
                    ushort[] ordinals = new ushort[namedSymbolCount];
                    for (int n = 0, x = 0; n < _symbols.Length; n++)
                    {
                        if (_symbols[n].Name != null)
                        {
                            namedSymbols[x] = _symbols[n];
                            ordinals[x] = (ushort)n;
                            x++;
                        }
                    }
                    Array.Sort(namedSymbols, (Symbol a, Symbol b) =>
                    {
                        return LexicographicCompare.Compare(a.Name, b.Name);
                    });

                    uint exportedAddressTableSize = (uint)_symbols.Length * 4;
                    uint exportedOrdinalTableSize = (uint)namedSymbolCount * 2;
                    uint exportedNameTableSize = (uint)namedSymbolCount * 4;
                    uint exportedAddressTableRVA = _RVA + ExportDirectoryTableSize;
                    uint exportedOrdinalTableRVA = exportedAddressTableRVA + exportedAddressTableSize;
                    uint exportedNameTableRVA = exportedOrdinalTableRVA + exportedOrdinalTableSize;
                    uint stringTableRVA = exportedNameTableRVA + exportedNameTableSize;

                    uint[] nameRVA = new uint[namedSymbols.Length];
                    {
                        uint currentRVA = stringTableRVA;
                        // Calculate the RVA for the string table
                        for (int n = 0; n < _symbols.Length; n++)
                        {
                            if (_symbols[n].Name == null)
                            {
                                nameRVA[n] = 0;
                            }
                            else
                            {
                                byte[] name = System.Text.Encoding.UTF8.GetBytes(_symbols[n].Name);
                                nameRVA[n] = currentRVA;
                                currentRVA += (uint)(name.Length + 1);
                            }
                        }
                    }

                    // Write the directory
                    binaryWriter.Write(_flags);
                    binaryWriter.Write(ToTimestamp(_timestamp));
                    binaryWriter.Write((ushort)_version.Major);
                    binaryWriter.Write((ushort)_version.Minor);
                    binaryWriter.Write(_imageNameRVA);
                    binaryWriter.Write(_exportOrdinalBase);
                    binaryWriter.Write(_symbols.Length);
                    binaryWriter.Write(namedSymbolCount);

                    binaryWriter.Write(exportedAddressTableRVA);
                    binaryWriter.Write(exportedNameTableRVA);
                    binaryWriter.Write(exportedOrdinalTableRVA);

                    // Write the address table
                    for (int n = 0; n < _symbols.Length; n++)
                    {
                        binaryWriter.Write(_symbols[n].Address);
                    }

                    // Write the ordinal table
                    for (int n = 0; n < namedSymbols.Length; n++)
                    {
                        binaryWriter.Write((ushort)(ordinals[n]));
                    }

                    // Write the name table
                    for (int n = 0; n < namedSymbols.Length; n++)
                    {
                        binaryWriter.Write(nameRVA[n]);
                    }

                    // Write the string table
                    {
                        uint currentRVA = stringTableRVA;
                        // Calculate the RVA for the string table
                        for (int n = 0; n < _symbols.Length; n++)
                        {
                            if (_symbols[n].Name != null)
                            {
                                byte[] name = System.Text.Encoding.UTF8.GetBytes(_symbols[n].Name);
                                binaryWriter.Write(name);
                                binaryWriter.Write((byte)0);
                            }
                        }

                        binaryWriter.Write((byte)0);
                        binaryWriter.Write((byte)0);
                    }
                }
                internal ExportTable()
                {

                }
                public ExportTable(uint rva, uint flags, DateTime timestamp, uint imageNameRva, uint exportOrdinalBase, Symbol[] symbols)
                {
                    _RVA = rva;
                    _flags = flags;
                    _timestamp = timestamp;
                    _exportOrdinalBase = exportOrdinalBase;
                    _imageNameRVA = imageNameRva;


                    uint namedSymbolCount = 0;
                    for (int n = 0; n < _symbols.Length; n++)
                    {
                        if (_symbols[n].Name != null) { namedSymbolCount++; }
                    }

                    uint exportedAddressTableSize = (uint)_symbols.Length * 4;
                    uint exportedOrdinalTableSize = (uint)namedSymbolCount * 2;
                    uint exportedNameTableSize = (uint)namedSymbolCount * 4;
                    _exportAddressTableRVA = rva + ExportDirectoryTableSize;
                    _exportOrdinalTableRVA = _exportAddressTableRVA + exportedAddressTableSize;
                    _exportNameTableRVA = _exportOrdinalTableRVA + exportedOrdinalTableSize;
                }
            }
        }
    }
}