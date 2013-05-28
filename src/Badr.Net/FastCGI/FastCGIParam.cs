//
// FastCGIParam.cs
//
// Author: najmeddine nouri
//
// Copyright (c) 2013 najmeddine nouri, amine gassem
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// Except as contained in this notice, the name(s) of the above copyright holders
// shall not be used in advertising or otherwise to promote the sale, use or other
// dealings in this Software without prior written authorization.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badr.Net.FastCGI
{
    public class FastCGIParam
    {
        public readonly string Name;
        public readonly string Value;
        public readonly int ParamEndOffset;

        public FastCGIParam(byte[] buffer, int offset)
        {
            int nameLength = GetNextLength(buffer, ref offset);
            int valueLength = GetNextLength(buffer, ref offset);

            Name = Encoding.ASCII.GetString(buffer, offset, nameLength);
            if (valueLength > 0)
                Value = Encoding.ASCII.GetString(buffer, offset + nameLength, valueLength);
            else
                Value = "";

            ParamEndOffset = offset + nameLength + valueLength;
        }

        private int GetNextLength(byte[] buffer, ref int offset)
        {
            int length = buffer[offset];
            if (length >> 7 == 1)
            {
                length = ((length & 0x7F) << 24) + (buffer[offset + 1] << 16) + (buffer[offset + 2] << 8) + (buffer[offset + 3]);
                offset += 4;
            }
            else
                offset += 1;

            return length;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, Value);
        }
    }
}
