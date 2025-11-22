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
        internal static class LexicographicCompare
        {
            public static int Compare(string a, string b)
            {
                int min = a.Length;
                if (b.Length < min) { min = b.Length; }
                for (int n = 0; n < min; n++)
                {
                    if (a[n] == b[n]) { continue; }
                    if (char.IsDigit(a[n]) && !char.IsDigit(b[n])) { return -1; }
                    if (char.IsDigit(b[n]) && !char.IsDigit(a[n])) { return 1; }
                    if (char.IsUpper(a[n]) && !char.IsUpper(b[n])) { return -1; }
                    if (char.IsUpper(b[n]) && !char.IsUpper(a[n])) { return 1; }
                    if (a[n] == '_') { return -1; }
                    else if (b[n] == '_') { return 1; }
                    if ((byte)a[n] < (byte)b[n]) { return -1; }
                    return 1;
                }
                if (a.Length < b.Length) { return -1; }
                if (b.Length < a.Length) { return 1; }
                return 0;
            }
        }
    }
}
