//
// SocketAsyncManager.cs
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
using System.Net.Sockets;
using System.Text;

namespace Badr.Net
{
    public delegate void DataReceivedHandler(byte[] buffer, int offset, int length);

    public interface ISocketAsyncManager: IDisposable
    {
        event Action<ISocketAsyncManager> ConnectionClosed;

        void ReceiveAsync();
        void SendAsync(byte[] buffer, int offset, int length, bool close = false);
        void CloseConnection();
        void Clear();

        Socket SendReceiveSocket { get; set; }
        SocketAsyncReceiveArgs Receiver { get; set; }
        SocketAsyncSendArgs Sender { get; set; }
        NetProcessor Processor { get; }
        string ID { get; }
        int ReceiveOpCount { get; }
        int TotalReceived { get; }
        int TotalSent { get; }
        bool ShouldCloseConnection { get; set; }
    }

    public class SocketAsyncManager : ISocketAsyncManager
    {
        public event Action<ISocketAsyncManager> ConnectionClosed;

        public NetProcessor Processor { get; protected set; }

        public Socket SendReceiveSocket { get; set; }
        public SocketAsyncReceiveArgs Receiver { get; set; }
        public SocketAsyncSendArgs Sender { get; set; }        
        public string ID { get; protected set; }
        public int ReceiveOpCount { get { return Receiver.ReceiveOpCount; } }
        public int TotalReceived { get { return Receiver.TotalReceived; } }
        public int TotalSent { get { return Sender.TotalSent; } }
        public bool ShouldCloseConnection { get; set; }

        public SocketAsyncManager(NetProcessor processor, string id)
        {
            ID = id;
            Receiver = new SocketAsyncReceiveArgs(this);
            Sender = new SocketAsyncSendArgs(this);
            if (processor != null)
            {
                Processor = processor;
                Processor.SocketAsyncManager = this;
            }
        }

        public void ReceiveAsync()
        {
            Receiver.ReceiveAsync();
        }

        public void SendAsync(byte[] buffer, int offset, int length, bool closeConnection = false)
        {
            ShouldCloseConnection = closeConnection;
            Sender.SendAsync(buffer, offset, length);
        }

        public void CloseConnection()
        {
            ShouldCloseConnection = true;

            if (SendReceiveSocket != null)
            {
                try
                {
                    SendReceiveSocket.Shutdown(SocketShutdown.Send);
                }
                catch (Exception) { }

                SendReceiveSocket.Close();

                Action<SocketAsyncManager> cc = ConnectionClosed;
                if (cc != null)
                    cc(this);
            }
        }

        public void Clear()
        {
            ShouldCloseConnection = false;

            if (Processor != null)
                Processor.Clear();

            if (Receiver != null)
                Receiver.Clear();

            if (Sender != null)
                Sender.Clear();
        }

        protected bool _isDisposed;
        public void Dispose()
        {
            if (!_isDisposed)
            {
                if (Receiver != null)
                {
                    Receiver.Dispose();
                    Receiver = null;
                }

                if (Sender != null)
                {
                    Sender.Dispose();
                    Sender = null;
                }

                _isDisposed = true;
            }
        }     
    }
}
