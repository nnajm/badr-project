//
// NetServer.cs
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
using Badr.Net.Http;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Badr.Net
{
    public class NetServer
    {
        #region fields

        private static readonly ILog _Logger = LogManager.GetLogger(typeof(NetServer));

        protected internal const int BUFFER_SIZE = 1024 * 8;

        protected int _maxConnectionNumber;
        protected int _receiveBufferSize;

        protected BufferManager _bufferManager;
        protected const int opsToPreAlloc = 2;

        protected Socket _listenSocket;

        protected SocketAsyncManagersPool _asyncManagersPool;
        protected List<ISocketAsyncManager> _connectedClients;

        protected Semaphore _maxConnectionsSemaphore;

        protected bool _isServerStarted;
        protected bool _acceptConnections;

        #endregion

        #region constructor(s)

        public NetServer (IPEndPoint ipEndPoint, int maxConnectionNumber)
		{
			IPEndPoint = ipEndPoint;

            _maxConnectionNumber = maxConnectionNumber;
            _receiveBufferSize = BUFFER_SIZE;

            _maxConnectionsSemaphore = new Semaphore(maxConnectionNumber, maxConnectionNumber);

            Init();
        }

        public NetServer(IPAddress ipAddr, int port, int maxConnectionNumber)
            : this(new IPEndPoint(ipAddr, port), maxConnectionNumber)
        {
        }

        public NetServer(string ipAddr, int port, int maxConnectionNumber)
            : this(IPAddress.Parse(ipAddr), port, maxConnectionNumber)
        {
        }

		#endregion

        #region Properties

        public IPEndPoint IPEndPoint { get; protected set; }

        #endregion

         protected void Init()
        {
            _bufferManager = new BufferManager(BUFFER_SIZE * _maxConnectionNumber * opsToPreAlloc, BUFFER_SIZE);
            _bufferManager.InitBuffer();

            _asyncManagersPool = new SocketAsyncManagersPool(_maxConnectionNumber);

            for (int i = 0; i < _maxConnectionNumber; i++)
            {
                ISocketAsyncManager asyncManager = NewSocketAsyncManager("async_man_" + i);
                asyncManager.ConnectionClosed += AsyncManager_ConnectionClosed;
               
                asyncManager.Receiver = new SocketAsyncReceiveArgs(asyncManager);
                asyncManager.Sender = new SocketAsyncSendArgs(asyncManager);

                _bufferManager.SetBuffer(asyncManager.Receiver);

                _asyncManagersPool.Push(asyncManager);
            }

            _connectedClients = new List<ISocketAsyncManager>();
        }

        public virtual void Start()
        {
            if (!_isServerStarted)
            {
                _isServerStarted = true;
                _acceptConnections = true;

                //ThreadPool.SetMinThreads(_maxConnectionNumber / 4, _maxConnectionNumber / 4);

                _listenSocket = new Socket(IPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _listenSocket.Bind(IPEndPoint);
                _listenSocket.Listen(701);

                StartAccept(null);

                Console.WriteLine(string.Format("Server started: {0}", IPEndPoint.ToString()));

                while (_isServerStarted)
                    Thread.Sleep(900000);
            }
        }

        public virtual void Stop()
        {
            if(_isServerStarted)
            {
                _acceptConnections = false;

                for (int i = 0; i < _connectedClients.Count; i++)
                    _connectedClients[0].CloseConnection();

                _connectedClients.Clear();
                _asyncManagersPool.Dispose();

                if (_listenSocket.IsBound)
                {
                    _listenSocket.Disconnect(false);
                    _listenSocket.Close();
                }

                _isServerStarted = false;
            }
        }

        protected ISocketAsyncManager PopFromAsynSocketsPool()
        {
            lock (_asyncManagersPool)
            {
                ISocketAsyncManager asyncManager = _asyncManagersPool.Pop();
                _connectedClients.Add(asyncManager);
                return asyncManager;
            }
        }

        protected void ReturnToAsyncSocketsPool(ISocketAsyncManager asyncManager)
        {
            _maxConnectionsSemaphore.Release();

            lock (_asyncManagersPool)
            {
                asyncManager.Clear();
                _connectedClients.Remove(asyncManager);
                _asyncManagersPool.Push(asyncManager);
            }
        }

        public void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (_acceptConnections)
            {
                if (acceptEventArg == null)
                {
                    acceptEventArg = new SocketAsyncEventArgs();
                    acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessAccept);
                }
                else
                    acceptEventArg.AcceptSocket = null;

                _maxConnectionsSemaphore.WaitOne();
                
                if (!_listenSocket.AcceptAsync(acceptEventArg))
                {
                    ProcessAccept(null, acceptEventArg);
                }
            }
        }

        private int _acceptCount = 0;

        private void ProcessAccept(object sender, SocketAsyncEventArgs e)
        {
            Interlocked.Increment(ref _acceptCount);
            _Logger.InfoFormat("accepted so far: {0}", _acceptCount);
            
            Socket accSocket = e.AcceptSocket;
            StartAccept(e);

            ISocketAsyncManager asyncManager = PopFromAsynSocketsPool();
            asyncManager.SendReceiveSocket = accSocket;
            asyncManager.ReceiveAsync();
        }

        private void AsyncManager_ConnectionClosed(ISocketAsyncManager asyncManager)
        {
            ReturnToAsyncSocketsPool(asyncManager);
        }

        /// <summary>
        /// When overridden in a derived class, returns an instance of NetProcessor to be used by the server
        /// </summary>
        /// <param name="socketAsyncManager">The ISocketAsyncManager to pass to processor</param>
        /// <returns>A (derived) NetProcessor instance</returns>
        protected virtual ISocketAsyncManager NewSocketAsyncManager(string id)
        {
            return null;
        }

    }
}
