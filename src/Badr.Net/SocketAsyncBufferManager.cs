//
// HttpRequestHeaders.cs
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
using System.Net.Sockets;
using System.Text;

namespace Badr.Net
{
    // Contains a single large buffer divided between SocketAsyncEventArgs instances.
    public class SocketAsyncBufferManager
    {
        private int _theBufferSize;
        private byte[] _theBuffer;
        private int _availableOffset;
        private int _divBufferSize;

        public SocketAsyncBufferManager(int totalBytes, int bufferSize)
        {
            _theBufferSize = totalBytes;
            _availableOffset = 0;
            _divBufferSize = bufferSize;
        }

        public void CreateBuffer()
        {
            _theBuffer = new byte[_theBufferSize];
        }

        public bool AssignBuffer (SocketAsyncEventArgs args)
		{

			if ((_theBufferSize - _divBufferSize) < _availableOffset)
			{
				return false;
			}
			args.SetBuffer (_theBuffer, _availableOffset, _divBufferSize);
			_availableOffset += _divBufferSize;

			return true;
		}
    }
}
