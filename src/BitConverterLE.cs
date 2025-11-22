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
        internal static class BitConverterLE
        {
#if NET6_0_OR_GREATER
            public static ulong ToUInt64(Span<byte> data, uint offset)
            {
                return ((ulong)data[(int)offset]) +
                       (((ulong)data[(int)offset + 1]) << 8) +
                       (((ulong)data[(int)offset + 2]) << 16) +
                       (((ulong)data[(int)offset + 3]) << 24) +
                       (((ulong)data[(int)offset + 4]) << 32) +
                       (((ulong)data[(int)offset + 5]) << 40) +
                       (((ulong)data[(int)offset + 6]) << 48) +
                       (((ulong)data[(int)offset + 7]) << 56);
            }
            public static uint ToUInt32(Span<byte> data, uint offset)
            {
                return ((uint)data[(int)offset]) +
                       (((uint)data[(int)offset + 1]) << 8) +
                       (((uint)data[(int)offset + 2]) << 16) +
                       (((uint)data[(int)offset + 3]) << 24);
            }
            public static ushort ToUInt16(Span<byte> data, uint offset)
            {
                return (ushort)(((uint)data[(int)offset]) +
                                (((uint)data[(int)offset + 1]) << 8));
            }
#endif
            public static ulong ToUInt64(byte[] data, uint offset)
            {
                return ((ulong)data[offset]) +
                       (((ulong)data[offset + 1]) << 8) +
                       (((ulong)data[offset + 2]) << 16) +
                       (((ulong)data[offset + 3]) << 24) +
                       (((ulong)data[offset + 4]) << 32) +
                       (((ulong)data[offset + 5]) << 40) +
                       (((ulong)data[offset + 6]) << 48) +
                       (((ulong)data[offset + 7]) << 56);
            }
            public static uint ToUInt32(byte[] data, uint offset)
            {
                return ((uint)data[offset]) +
                       (((uint)data[offset + 1]) << 8) +
                       (((uint)data[offset + 2]) << 16) +
                       (((uint)data[offset + 3]) << 24);
            }
            public static ushort ToUInt16(byte[] data, uint offset)
            {
                return (ushort)(((uint)data[offset]) +
                                (((uint)data[offset + 1]) << 8));
            }
        }
    }
}
