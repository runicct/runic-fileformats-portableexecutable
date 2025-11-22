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
            public class CLIHeader
            {
                public enum CorFlags : uint
                {
                    None = 0x00000000,
                    ILOnly = 0x00000001,
                    Requires32Bit = 0x00000002,
                    ILLibrary = 0x00000004,
                    StrongNameSigned = 0x00000008,
                    NativeEntryPoint = 0x00000010,
                    TrackDebugData = 0x00010000,
                    Preffers32Bit = 0x00020000
                }
                uint _metadataRootRVA;
                public uint MetadataRootRVA
                {
                    get { return _metadataRootRVA; }
                    set { _metadataRootRVA = value; }
                }
                uint _metadataSize;
                public uint MetadataSize
                {
                    get { return _metadataSize; }
                    set { _metadataSize = value; }
                }
                uint _resourcesRVA;
                public uint ResourcesRVA
                {
                    get { return _resourcesRVA; }
                    set { _resourcesRVA = value; }
                }
                uint _resourcesSize;
                public uint ResourcesSize
                {
                    get { return _resourcesSize; }
                    set { _resourcesSize = value; }
                }
                uint _strongNameSignatureRVA;
                public uint StrongNameSignatureRVA
                {
                    get { return _strongNameSignatureRVA; }
                    set { _strongNameSignatureRVA = value; }
                }
                uint _strongNameSignatureSize;
                public uint StrongNameSignatureSize
                {
                    get { return _strongNameSignatureSize; }
                    set { _strongNameSignatureSize = value; }
                }
                uint _VTableFixupsRVA;
                public uint VTableFixupsRVA
                {
                    get { return _VTableFixupsRVA; }
                    set { _VTableFixupsRVA = value; }
                }
                uint _VTableFixupsSize;
                public uint VTableFixupsSize
                {
                    get { return _VTableFixupsSize; }
                    set { _VTableFixupsSize = value; }
                }
                CorFlags _flags;
                public CorFlags Flags
                {
                    get { return _flags; }
                    set { _flags = value; }
                }
                uint _entryPoint;
                public uint EntryPoint
                {
                    get { return _entryPoint; }
                    set { _entryPoint = value; }
                }
                Version _runtimeVersion;
                public Version RuntimeVersion
                {
                    get { return _runtimeVersion; }
                    set { _runtimeVersion = value; }
                }
                public void Save(System.IO.BinaryWriter binaryWriter)
                {
                    binaryWriter.Write((int)72);
                    binaryWriter.Write((short)_runtimeVersion.Major); // Major Runtime version
                    binaryWriter.Write((short)_runtimeVersion.Minor); // Minor Runtime version
                    binaryWriter.Write((uint)_metadataRootRVA);
                    binaryWriter.Write((uint)_metadataSize);
                    binaryWriter.Write((uint)_flags);
                    binaryWriter.Write((uint)_entryPoint);
                    binaryWriter.Write((uint)_resourcesRVA);
                    binaryWriter.Write((uint)_resourcesSize);
                    binaryWriter.Write((uint)_strongNameSignatureRVA);
                    binaryWriter.Write((uint)_strongNameSignatureSize);
                    binaryWriter.Write((ulong)0); // Reserved
                    binaryWriter.Write((uint)_VTableFixupsRVA);
                    binaryWriter.Write((uint)_VTableFixupsSize);
                    binaryWriter.Write((ulong)0); // Reserved
                    binaryWriter.Write((ulong)0); // Reserved
                }
                public static CLIHeader? Load(byte[] data, uint offset)
                {
                    if (data.Length < offset + 72) { return null; }
                    offset += 4; // Skip Header Size
                    ushort majorRuntimeVersion = BitConverterLE.ToUInt16(data, offset); offset += 2;
                    ushort minorRuntimeVersion = BitConverterLE.ToUInt16(data, offset); offset += 2;
                    uint metadataRootRVA = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint metadataSize = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    CorFlags flags = (CorFlags)BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint entrypoint = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint resourcesRVA = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint resourcesSize = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint strongNameSignatureRVA = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint strongNameSignatureSize = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    offset += 8; // Reserved
                    uint VTableFixupsRVA = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint VTableFixupsSize = BitConverterLE.ToUInt32(data, offset); offset += 4;

                    return new CLIHeader(new Version(majorRuntimeVersion, minorRuntimeVersion), metadataRootRVA, metadataSize, flags, entrypoint, resourcesRVA, resourcesSize, strongNameSignatureRVA, strongNameSignatureSize, VTableFixupsRVA, VTableFixupsSize);
                }
#if NET6_0_OR_GREATER
                public static CLIHeader? Load(Span<byte> data, uint offset)
                {
                    if (data.Length < offset + 72) { return null; }
                    offset += 4; // Skip Header Size
                    ushort majorRuntimeVersion = BitConverterLE.ToUInt16(data, offset); offset += 2;
                    ushort minorRuntimeVersion = BitConverterLE.ToUInt16(data, offset); offset += 2;
                    uint metadataRootRVA = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint metadataSize = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    CorFlags flags = (CorFlags)BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint entrypoint = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint resourcesRVA = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint resourcesSize = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint strongNameSignatureRVA = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint strongNameSignatureSize = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    offset += 8; // Reserved
                    uint VTableFixupsRVA = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint VTableFixupsSize = BitConverterLE.ToUInt32(data, offset); offset += 4;

                    return new CLIHeader(new Version(majorRuntimeVersion, minorRuntimeVersion), metadataRootRVA, metadataSize, flags, entrypoint, resourcesRVA, resourcesSize, strongNameSignatureRVA, strongNameSignatureSize, VTableFixupsRVA, VTableFixupsSize);
                }
#endif
                public static CLIHeader? Load(PortableExecutable portableExecutable)
                {
                    if (portableExecutable.CLIHeader.RelativeVirtualAddress == 0) { return null; }

#if NET6_0_OR_GREATER
                    Span<byte> header = portableExecutable.GetSpanAtRelativeVirtualAddress(portableExecutable.CLIHeader.RelativeVirtualAddress, 72);
                    if (header.IsEmpty || header.Length < 72) { return null; }
#else
                    byte[]? header = portableExecutable.ReadArrayAtRelativeVirtualAddress(portableExecutable.CLIHeader.RelativeVirtualAddress, 72);
                    if (header == null || header.Length < 72) { return null; }
#endif

                    return Load(header, 0);
                }


                public CLIHeader(Version runtimeVersion, uint metadataRootRVA, uint metadataSize, CorFlags flags, uint entrypoint, uint resourcesRVA, uint resourcesSize, uint strongNameSignatureRVA, uint strongNameSignatureSize, uint VTableFixupsRVA, uint VTableFixupsSize)
                {
                    _runtimeVersion = runtimeVersion;
                    _metadataRootRVA = metadataRootRVA;
                    _metadataSize = metadataSize;
                    _flags = flags;
                    _entryPoint = entrypoint;
                    _resourcesRVA = resourcesRVA;
                    _resourcesSize = resourcesSize;
                    _strongNameSignatureRVA = strongNameSignatureRVA;
                    _strongNameSignatureSize = strongNameSignatureSize;
                    _VTableFixupsRVA = VTableFixupsRVA;
                    _VTableFixupsSize = VTableFixupsSize;
                }
            }
        }
    }
}
