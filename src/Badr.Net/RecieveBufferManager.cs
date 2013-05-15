//
// ReceiveBufferManager.cs
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
using Badr.Net.Utils;

namespace Badr.Net.Http
{
    public class ReceiveBufferManager
    {
        protected internal static byte[] EOL = { (byte)'\r', (byte)'\n' };

        protected byte[] _receiveBuffer;
        protected byte[] _workingBuffer;

        private int _receivedTotal;
        private int _treatedTotal;
        private int _workingCount;
        private int _pendingStartIndex;


        /// <summary>
        /// Initializes a new intance of ReceiveBufferManager
        /// </summary>
        /// <param name="size"></param>
        public ReceiveBufferManager(int size)
        {
            _receiveBuffer = new byte[size];
            _workingBuffer = new byte[size * 2];
        }

        /// <summary>
        /// Tells ReceiveBufferManager that new bytes were received into the ReceiveBuffer
        /// This means those bytes will be copied into the workingBuffer taking into account the pending bytes not marked as treated yet
        /// </summary>
        /// <param name="received">The number of bytes received</param>
        public void RefreshWorkingBuffer(int received)
        {
            for (int i = _pendingStartIndex; i < _workingCount; i++)
                _workingBuffer[i - _pendingStartIndex] = _workingBuffer[i];
            for (int i = 0; i < received; i++)
                _workingBuffer[Count + i] = _receiveBuffer[i];

            _receivedTotal += received;
            _workingCount = Count;
            _pendingStartIndex = 0;
        }

        /// <summary>
        /// Tells ReceiveBufferManager to integrate the supplied buffer
        /// This means those bytes will be copied into the workingBuffer taking into account the pending bytes not marked as treated yet
        /// </summary>
        /// <param name="buffer">the buffer containing data</param>
        /// <param name="offset">data offset in the buffer</param>
        /// <param name="length">data length</param>
        public void RefreshWorkingBuffer(byte[] buffer, int offset, int length)
        {
            try
            {
                for (int i = _pendingStartIndex; i < _workingCount; i++)
                    _workingBuffer[i - _pendingStartIndex] = _workingBuffer[i];
                for (int i = 0; i < length; i++)
                    _workingBuffer[Count + i] = buffer[offset + i];
                for (int i = Count + length; i < _workingBuffer.Length; i++)
                    _workingBuffer[i] = 0;
            }
            catch (Exception ex)
            {
            }

            _receivedTotal += length;
            _workingCount = Count;
            _pendingStartIndex = 0;
        }

        /// <summary>
        /// Returns the index of the first occurence of '\r\n' within the workingBuffer starting from StartIndex
        /// </summary>
        /// <returns>the index of '\r\n' if found, otherwise -1</returns>
        public int IndexOfEol()
        {
            return _workingBuffer.IndexOfSubArray(EOL, _pendingStartIndex, Count);
        }

        /// <summary>
        /// Returns the index of the last occurence of '\r\n' within the workingBuffer starting from StartIndex
        /// </summary>
        /// <returns>the index of '\r\n' if found, otherwise -1</returns>
        public int LastIndexOfEol()
        {
            return _workingBuffer.LastIndexOfSubArray(EOL, WorkingCount - 1, Count);
        }

        /// <summary>
        /// Returns the index between StartIndex and limitIndex of the first occurence of subArray within the workingBuffer.
        /// </summary>
        /// <param name="subArray">The subarray to search for</param>
        /// <param name="limitIndex">The limiting upper index of the search range</param>
        /// <returns>The index of subArray if found, otherwise -1</returns>
        internal int IndexOfArray(byte[] subArray, int limitIndex)
        {
            return _workingBuffer.IndexOfSubArray(subArray, _pendingStartIndex, limitIndex - _pendingStartIndex);
        }

        /// <summary>
        /// Marks the bytes from StartIndex to endIndex as treated and returns the string representation of those bytes.
        /// </summary>
        /// <param name="endIndex">The end index of the string (start index is StartIndex). leaving endIndex = -1 means pop the string to the end of the buffer </param>
        /// <param name="extraDiscardBytes">Extra bytes to mark as treated but not include in the output string</param>
        /// <returns></returns>
        public string PopString(int endIndex = -1, int extraDiscardBytes = 0)
        {
            int length = endIndex == -1 ? Count : endIndex - _pendingStartIndex;
            string str = Encoding.Default.GetString(_workingBuffer, _pendingStartIndex, length);
            MarkAsTreated(length + extraDiscardBytes);
            return str;
        }

#if DEBUG
        public string GetNextString(int length = -1)
        {
            return Encoding.Default.GetString(_workingBuffer, _pendingStartIndex, length == -1? Count: length);
        }
#endif

        /// <summary>
        /// Marks the next 'length' bytes as treated => StartIndex will be incremented by 'length' and Count will be decremented by 'length'
        /// </summary>
        /// <param name="length"></param>
        public void MarkAsTreated(int length)
        {
            if (_treatedTotal + length > _receivedTotal)
            {
            }

            _treatedTotal += length;
            _pendingStartIndex += length;
        }

        /// <summary>
        /// Intermediary buffer to use with socket.Receive
        /// </summary>
        public byte[] ReceiveBuffer { get { return _receiveBuffer; } }
        /// <summary>
        /// Real working buffer containing bytes to decode (non treated bytes starts from StartIndex to StartIndex + Count - 1
        /// </summary>
        public byte[] WorkingBuffer { get { return _workingBuffer; } }
        /// <summary>
        /// Working buffer length since the last refresh (call to RefreshWorkingBuffer)
        /// </summary>
        public int WorkingCount { get { return _workingCount; } }
        /// <summary>
        /// Non treated bytes count
        /// </summary>
        public int Count { get { return _receivedTotal - _treatedTotal; } }
        /// <summary>
        /// Non treated bytes starting index within WorkingBuffer
        /// </summary>
        public int StartIndex { get { return _pendingStartIndex; } }

        /// <summary>
        /// Checks if WorkingBuffer has '\r\n' at StartIndex
        /// </summary>
        /// <returns></returns>
        internal bool StartsWithEol()
        {
            return _workingBuffer.ContainsSubArrayAt(EOL, _pendingStartIndex);
        }

        public void Reset()
        {
            Array.Clear(_receiveBuffer, 0, _receiveBuffer.Length);
            Array.Clear(_workingBuffer, 0, _workingBuffer.Length);

            _receivedTotal = 0;
            _treatedTotal = 0;
            _workingCount = 0;
            _pendingStartIndex = 0;
        }
    }
}
