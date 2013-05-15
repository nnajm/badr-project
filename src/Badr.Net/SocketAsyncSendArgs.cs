//
// SocketAsyncSendArgs.cs
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
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Badr.Net
{
    public class SocketAsyncSendArgs : SocketAsyncEventArgs
    {
        private static readonly ILog _Logger = LogManager.GetLogger(typeof(SocketAsyncSendArgs));

        public string ID { get; private set; }
        public int TotalSent { get; private set; }
        public int SendOpCount { get; private set; }
        protected ISocketAsyncManager _asyncManager;

        public SocketAsyncSendArgs(ISocketAsyncManager asyncManager)
            : base()
        {
            _asyncManager = asyncManager;
            ID = _asyncManager.ID + "_SEND";

            TotalSent = 0;
        }

        public void SendAsync(byte[] buffer, int offset, int length)
        {
            SetBuffer(buffer, offset, length);
            if (!_asyncManager.SendReceiveSocket.SendAsync(this))
                ProcessSend();
        }

        public void ProcessSend()
        {
            if (SocketError == SocketError.Success)
            {
                SendOpCount++;
                TotalSent += BytesTransferred;

                //_Logger.InfoFormat("[Thread-{0}][SOCKET[{1}->>{2}][{3}-{4}]: {5}",
                //    System.Threading.Thread.CurrentThread.ManagedThreadId,
                //    _asyncManager.SendReceiveSocket.Handle,
                //    _asyncManager.SendReceiveSocket.RemoteEndPoint,
                //    ID,
                //    SendOpCount,
                //    Encoding.Default.GetString(Buffer, Offset, Math.Min(101, Count)));

                if(_asyncManager.DisconnetAfterSend)
                    _asyncManager.CloseConnection();
            }
            else
            {
                _Logger.DebugFormat("Error {0}", SocketError);
                _asyncManager.CloseConnection();
            }
        }

        protected override void OnCompleted(SocketAsyncEventArgs e)
        {
            base.OnCompleted(e);

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    ProcessSend();
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }


        public void Clear()
        {
            TotalSent = 0;
        }
    }
}
