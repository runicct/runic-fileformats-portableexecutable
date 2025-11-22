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
        public class Section
        {
            string _name;
            public virtual string Name { get { return _name; } set { if (value.Length > 8) { throw new Exception("Name is too long"); } _name = value; } }
            uint _size;
            public virtual uint Size { get { return _size; } set { _size = value; } }
            uint _relativeVirtualAddress;
            public virtual uint RelativeVirtualAddress { get { return _relativeVirtualAddress; } set { _relativeVirtualAddress = value; } }
            public virtual byte[]? GetData() { return null; }
#if NET6_0_OR_GREATER
            public virtual Span<byte> GetDataSpan() { return Span<byte>.Empty; }
#endif
            Flag _characteristic = Flag.None;
            public virtual Flag Characteristics { get { return _characteristic; } set { _characteristic = value; } }
            public enum Flag : uint
            {
                None = 0,
                NoPad = 0x8,
                Code = 0x20,
                InitializedData = 0x40,
                UninitializedData = 0x80,
                Remove = 0x800,
                Comdat = 0x1000,
                MemExecute = 0x20000000,
                MemRead = 0x40000000,
                MemWrite = 0x80000000,
            }
            public Section(string name, uint relativeVirtualAddress, uint size, Flag characteristics)
            {
                _name = name;
                if (_name.Length > 8) { throw new Exception("Name is too long"); }
                _relativeVirtualAddress = relativeVirtualAddress;
                _size = size;
                _characteristic = characteristics;
            }
        }
    }
}
