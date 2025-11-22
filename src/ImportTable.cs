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
using System.Text;

namespace Runic.FileFormats
{
    public partial class PortableExecutable
    {
        public partial class Directories
        {
            public class ImportTable
            {
                public class Library
                {
                    public class Symbol
                    {

                    }
                    public class SymbolByOrdinal : Symbol
                    {
                        ushort _ordinal;
                        public ushort Ordinal { get { return _ordinal; } }
                        public SymbolByOrdinal(ushort ordinal)
                        {
                            _ordinal = ordinal;
                        }
                        public override string ToString()
                        {
                            return _ordinal.ToString();
                        }
                    }
                    public class SymbolByName : Symbol
                    {
                        ushort _hint;
                        public ushort Hint { get { return _hint; } }
                        string? _name;
                        public string? Name { get { return _name; } }
                        public SymbolByName(ushort hint, string? name)
                        {
                            _hint = hint;
                            _name = name;
                        }
                        public override string ToString()
                        {
                            return _name == null ? "" : _name;
                        }
                    }
                    string? _name;
                    public string? Name { get { return _name; } }
                    Symbol[] _symbols;
                    public Symbol[] Symbols { get { return _symbols; } }
                    public Library(string? name, Symbol[] symbols)
                    {
                        _name = name;
                        _symbols = symbols;
                    }
                }
                public static uint ImportDirectoryTableEntrySize { get { return 20; } }
                Library[] _libraries;
                public Library[] Libraries { get { return _libraries; } }
                uint _rva;
                public uint RelativeVirtualAddress { get { return _rva; } set { _rva = value; } }
                bool _isPE32plus;
                public bool IsPE32Plus { get { return _isPE32plus; } set { _isPE32plus = value; } }
                internal Library LoadEntryFromArray(bool pe32plus, byte[] data, uint offset, PortableExecutable portableExecutable)
                {
                    uint importLookupTableRVA = BitConverterLE.ToUInt32(data, offset);
                    DateTime timestamp = ToDatetime(BitConverterLE.ToUInt32(data, offset + 4));
                    uint forwarderChain = BitConverterLE.ToUInt32(data, offset + 8);
                    uint nameRVA = BitConverterLE.ToUInt32(data, offset + 12);
                    uint importAddressTableRVA = BitConverterLE.ToUInt32(data, offset + 16);

                    List<Library.Symbol> entries = new List<Library.Symbol>();
                    uint index = 0;
                    while (true)
                    {
                        ulong import = 0;
                        bool importByOrdinal = false;
                        if (pe32plus)
                        {
                            import = portableExecutable.ReadUInt64AtRelativeVirtualAddress(importLookupTableRVA + index * 8);
                            importByOrdinal = ((import & 0x8000000000000000) != 0);
                        }
                        else

                        {
                            import = portableExecutable.ReadUInt32AtRelativeVirtualAddress(importLookupTableRVA + index * 4);
                            importByOrdinal = ((import & 0x80000000) != 0);
                        }
                        if (import == 0) { break; }

                        if (importByOrdinal)
                        {
                            Library.SymbolByOrdinal symbolByOrdinal = new Library.SymbolByOrdinal((ushort)(import & 0xFFFF));
                            entries.Add(symbolByOrdinal);
                        }
                        else
                        {
                            uint rva = (uint)(import & 0x7FFFFFFF);
                            ushort hint = portableExecutable.ReadUInt16AtRelativeVirtualAddress(rva);
                            string? name = portableExecutable.ReadUTF8StringAtRelativeVirtualAddress(rva + 2);
                            Library.SymbolByName symbolByName = new Library.SymbolByName(hint, name);
                            entries.Add(symbolByName);
                        }
                        index++;
                    }

                    return new Library(portableExecutable.ReadUTF8StringAtRelativeVirtualAddress(nameRVA), entries.ToArray());
                }
                internal void LoadFromArray(bool pe32plus, uint rva, byte[] data, uint offset, uint size, PortableExecutable portableExecutable)
                {
                    _rva = rva;
                    _isPE32plus = pe32plus;
                    uint entryCount = size / ImportDirectoryTableEntrySize;
                    // The last entry is empty
                    _libraries = new Library[entryCount - 1];
                    for (uint n = 0; n < entryCount - 1; n++)
                    {
                        _libraries[n] = LoadEntryFromArray(pe32plus, data, offset + (n * ImportDirectoryTableEntrySize), portableExecutable);
                    }
                }
                public static ImportTable Load(bool pe32plus, uint rva, byte[] data, uint offset, uint size, PortableExecutable portableExecutable)
                {
                    ImportTable importTable = new ImportTable();
                    importTable.LoadFromArray(pe32plus, rva, data, offset, size, portableExecutable);
                    return importTable;
                }
#if NET6_0_OR_GREATER
                internal Library LoadEntryFromSpan(bool pe32plus, Span<byte> data, uint offset, PortableExecutable portableExecutable)
                {
                    uint importLookupTableRVA = BitConverterLE.ToUInt32(data, offset);
                    DateTime timestamp = ToDatetime(BitConverterLE.ToUInt32(data, offset + 4));
                    uint forwarderChain = BitConverterLE.ToUInt32(data, offset + 8);
                    uint nameRVA = BitConverterLE.ToUInt32(data, offset + 12);
                    uint importAddressTableRVA = BitConverterLE.ToUInt32(data, offset + 16);

                    List<Library.Symbol> entries = new List<Library.Symbol>();
                    uint index = 0;
                    while (true)
                    {
                        ulong import = 0;
                        bool importByOrdinal = false;
                        if (pe32plus)
                        {
                            import = portableExecutable.ReadUInt64AtRelativeVirtualAddress(importLookupTableRVA + index * 8);
                            importByOrdinal = ((import & 0x8000000000000000) != 0);
                        }
                        else

                        {
                            import = portableExecutable.ReadUInt32AtRelativeVirtualAddress(importLookupTableRVA + index * 4);
                            importByOrdinal = ((import & 0x80000000) != 0);
                        }
                        if (import == 0) { break; }

                        if (importByOrdinal)
                        {
                            Library.SymbolByOrdinal symbolByOrdinal = new Library.SymbolByOrdinal((ushort)(import & 0xFFFF));
                            entries.Add(symbolByOrdinal);
                        }
                        else
                        {
                            uint rva = (uint)(import & 0x7FFFFFFF);
                            ushort hint = portableExecutable.ReadUInt16AtRelativeVirtualAddress(rva);
                            string? name = portableExecutable.ReadUTF8StringAtRelativeVirtualAddress(rva + 2);
                            Library.SymbolByName symbolByName = new Library.SymbolByName(hint, name);
                            entries.Add(symbolByName);
                        }
                        index++;
                    }

                    return new Library(portableExecutable.ReadUTF8StringAtRelativeVirtualAddress(nameRVA), entries.ToArray());
                }
                internal void LoadFromSpan(bool pe32plus, uint rva, Span<byte> data, uint offset, uint size, PortableExecutable portableExecutable)
                {
                    _rva = rva;
                    _isPE32plus = pe32plus;
                    uint entryCount = size / ImportDirectoryTableEntrySize;
                    // The last entry is empty
                    _libraries = new Library[entryCount - 1];
                    for (uint n = 0; n < entryCount - 1; n++)
                    {
                        _libraries[n] = LoadEntryFromSpan(pe32plus, data, offset + (n * ImportDirectoryTableEntrySize), portableExecutable);
                    }
                }
                public static ImportTable Load(bool pe32plus, uint rva, Span<byte> data, uint offset, uint size, PortableExecutable portableExecutable)
                {
                    ImportTable importTable = new ImportTable();
                    importTable.LoadFromSpan(pe32plus, rva, data, offset, size, portableExecutable);
                    return importTable;
                }
#endif
                public static ImportTable Load(PortableExecutable portableExecutable)
                {
                    ImportTable importTable = new ImportTable();
                    Section? section = portableExecutable.FindSectionFromRelativeVirtualAddress(portableExecutable.ImportTable.RelativeVirtualAddress);
                    if (section == null) { return null; }
                    uint offset = portableExecutable.ImportTable.RelativeVirtualAddress - section.RelativeVirtualAddress;
                    importTable.LoadFromArray(portableExecutable.IsPE32Plus, portableExecutable.ImportTable.RelativeVirtualAddress, section.GetData(), offset, portableExecutable.ImportTable.Size, portableExecutable);
                    return importTable;
                }
                public void Save(System.IO.BinaryWriter binaryWriter)
                {
                    uint importDirectoryTableSize = ImportDirectoryTableEntrySize * ((uint)_libraries.Length + 1);
                    uint allImportTableEntrySize = 0;
                    for (int n = 0; n < _libraries.Length; n++)
                    {
                        allImportTableEntrySize += (uint)_libraries[n].Symbols.Length * (_isPE32plus ? 8U : 4U);
                        allImportTableEntrySize += (_isPE32plus ? 8U : 4U);
                    }
                    uint importLookupTableRVA = _rva + importDirectoryTableSize;
                    uint importAddressTableRVA = importLookupTableRVA + allImportTableEntrySize;
                    uint importDataTableRVA = importAddressTableRVA + allImportTableEntrySize;
                    uint dataPadding = importDataTableRVA % 4;

                    importDataTableRVA += dataPadding;
                    uint stringTableRVA = importDataTableRVA;

                    for (int n = 0; n < _libraries.Length; n++)
                    {
                        byte[] name = Encoding.ASCII.GetBytes(_libraries[n].Name ?? string.Empty);
                        importDataTableRVA += (uint)name.Length;
                        importDataTableRVA += importDataTableRVA % 4U;// Align to 4 bytes
                    }

                    uint currentStringRVA = stringTableRVA;
                    uint currentAddressTableRVA = importAddressTableRVA;
                    uint currentImportLookupTableRVA = importLookupTableRVA;
                    for (int n = 0; n < _libraries.Length; n++)
                    {
                        Library library = _libraries[n];
                        binaryWriter.Write(currentImportLookupTableRVA);
                        binaryWriter.Write((uint)0x0); // Timestamp is set to 0 until the library is loaded
                        binaryWriter.Write((uint)0x0); // Forwarder chain is set to 0 until the library is loaded
                        binaryWriter.Write(currentStringRVA);
                        binaryWriter.Write(currentAddressTableRVA);
                        byte[] name = Encoding.ASCII.GetBytes(_libraries[n].Name ?? string.Empty);
                        currentStringRVA += (uint)name.Length;
                        currentStringRVA += currentStringRVA % 4U;// Align to 4 bytes
                        currentImportLookupTableRVA += (uint)(library.Symbols.Length + 1) * (_isPE32plus ? 8U : 4U);
                        currentAddressTableRVA += (uint)(library.Symbols.Length + 1) * (_isPE32plus ? 8U : 4U);
                    }

                    // Last entry is empty
                    binaryWriter.Write((uint)0x0);
                    binaryWriter.Write((uint)0x0);
                    binaryWriter.Write((uint)0x0);
                    binaryWriter.Write((uint)0x0);
                    binaryWriter.Write((uint)0x0);

                    uint currentEntryRVA = importDataTableRVA;
                    // Write the import lookup table
                    for (int n = 0; n < _libraries.Length; n++)
                    {
                        for (int x = 0; x < _libraries[n].Symbols.Length; x++)
                        {
                            Library.Symbol symbol = _libraries[n].Symbols[x];
                            if (symbol is Library.SymbolByOrdinal symbolByOrdinal)
                            {
                                if (_isPE32plus) { binaryWriter.Write((ulong)(0x8000000000000000 | symbolByOrdinal.Ordinal)); }
                                else { binaryWriter.Write((uint)(0x80000000 | symbolByOrdinal.Ordinal)); }
                            }
                            else if (symbol is Library.SymbolByName symbolByName)
                            {
                                if (_isPE32plus) { binaryWriter.Write((ulong)currentEntryRVA); }
                                else { binaryWriter.Write((uint)currentEntryRVA); }
                                currentEntryRVA += 2; // Hint
                                currentEntryRVA += (uint)Encoding.ASCII.GetBytes(symbolByName.Name ?? string.Empty).Length + 1; // Null terminated name
                                currentEntryRVA += currentEntryRVA % 4U;// Align to 4 bytes
                            }
                        }
                    }

                    // Write the import address table
                    currentEntryRVA = importDataTableRVA;
                    for (int n = 0; n < _libraries.Length; n++)
                    {
                        for (int x = 0; x < _libraries[n].Symbols.Length; x++)
                        {
                            Library.Symbol symbol = _libraries[n].Symbols[x];
                            if (symbol is Library.SymbolByOrdinal symbolByOrdinal)
                            {
                                if (_isPE32plus) { binaryWriter.Write((ulong)(0x8000000000000000 | symbolByOrdinal.Ordinal)); }
                                else { binaryWriter.Write((uint)(0x80000000 | symbolByOrdinal.Ordinal)); }
                            }
                            else if (symbol is Library.SymbolByName symbolByName)
                            {
                                if (_isPE32plus) { binaryWriter.Write((ulong)currentEntryRVA); }
                                else { binaryWriter.Write((uint)currentEntryRVA); }
                                currentEntryRVA += 2; // Hint
                                currentEntryRVA += (uint)Encoding.ASCII.GetBytes(symbolByName.Name ?? string.Empty).Length + 1; // Null terminated name
                                currentEntryRVA += currentEntryRVA % 4U;// Align to 4 bytes
                            }
                        }
                    }

                    // write the padding
                    for (uint n = 0; n < dataPadding; n++) { binaryWriter.Write((byte)0); }

                    // Write the string table (Library names)
                    currentStringRVA = stringTableRVA;
                    for (int n = 0; n < _libraries.Length; n++)
                    {
                        byte[] name = Encoding.ASCII.GetBytes(_libraries[n].Name ?? string.Empty);
                        binaryWriter.Write(name); currentStringRVA += (uint)name.Length;
                        currentStringRVA += (uint)name.Length;
                        while (currentStringRVA % 4 != 0) { binaryWriter.Write((byte)0); currentStringRVA++; }
                    }
                    // Write the import hint where needed
                    for (int n = 0; n < _libraries.Length; n++)
                    {
                        for (int x = 0; x < _libraries[n].Symbols.Length; x++)
                        {
                            Library.Symbol symbol = _libraries[n].Symbols[x];
                            if (symbol is Library.SymbolByName symbolByName)
                            {
                                binaryWriter.Write((ushort)symbolByName.Hint); currentEntryRVA += 2;
                                byte[] name = Encoding.ASCII.GetBytes(symbolByName.Name ?? string.Empty);
                                binaryWriter.Write(name); currentEntryRVA += (uint)name.Length;
                                binaryWriter.Write((byte)0); currentEntryRVA += 0;
                                while (currentEntryRVA % 4 != 0) { binaryWriter.Write((byte)0); currentEntryRVA++; }
                            }
                        }
                    }
                }
                internal ImportTable()
                {
                }
                public ImportTable(uint relativeVirtuablAddress, bool isPE32plus, Library[] libraries)
                {
                    _rva = relativeVirtuablAddress;
                    _isPE32plus = isPE32plus;
                    _libraries = libraries;
                }
            }
        }
    }
}