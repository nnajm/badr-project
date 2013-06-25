//
// FastCGIInterpreter.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badr.Net.FastCGI
{
    public class FastCGIInterpreter
    {
        public FastCGIRequest Request { get; private set; }

        public FastCGIInterpreter()
        {
            EndOfRequest = false;
        }

        public bool EndOfRequest { get; private set; }
        public bool AbortRequest { get; private set; }

        public int ParseRequest(byte[] buffer, int offset, int length)
        {
            int endOffset = offset + length;

            while (offset < endOffset)
            {
                FastCGIHeader header = new FastCGIHeader(buffer, offset);
                if (header.RequestId != 0)
                {
                    if (Request == null)
                        Request = new FastCGIRequest();

                    offset += FastCGIHeader.LENGTH;

                    if (header.Type == FastCGIHeader.TYPE.BEGIN_REQUEST)
                    {
                        Request.BeginRequestRecord = new BeginRequestRecord()
                        {
                            Header = header,
                            Body = new BeginRequestBody(buffer, offset)
                        };
                        offset += header.ContentLength;
                    }
                    else if (header.Type == FastCGIHeader.TYPE.PARAMS)
                    {
                        offset = Request.ParseParams(header, buffer, offset, endOffset);
                    }
                    else
                    {
                        offset -= FastCGIHeader.LENGTH;
                        break;
                    }

                    offset += header.PaddingLength;
                }
            }

#if DEBUG
            Log();
#endif

            return offset;
        }

        public void Clear()
        {
            Request = null;
			_stdinHeader = null;
			_stdinDataReceivedLength = 0;
            EndOfRequest = false;
            AbortRequest = false;
        }

        private void Log()
        {
            System.IO.File.WriteAllText("nginx_request_" + Request.BeginRequestRecord.Header.RequestId + ".txt", string.Format("{0}{1}{1}---------{1}{1}", Request.ToString(), Environment.NewLine));
        }

        #region FCGI => HTTP

        public int TranslateToHttpData(byte[] buffer, int offset, int length)
        {

            int remainingOffset;
            int remainingReplaceOffset;

            if (Request == null)
            {
                remainingOffset = ParseRequest(buffer, offset, length);

				byte[] requestHeadersData = Request.ToHttpData();
                Array.Copy(requestHeadersData, 0, buffer, offset, requestHeadersData.Length);

                remainingReplaceOffset = offset + requestHeadersData.Length;
            }
            else
            {
                remainingOffset = offset;
                remainingReplaceOffset = offset;
            }

            int endOffset = TranslateRemainingData(buffer, 
                                                   remainingOffset,
                                                   length - (remainingOffset - offset), 
                                                   remainingReplaceOffset);

            return endOffset - offset;
        }

		private FastCGIHeader? _stdinHeader;
		private int _stdinDataReceivedLength = 0;

        private int TranslateRemainingData(byte[] buffer, int offset, int length, int replaceOffset)
        {
			List<byte> stdinData = new List<byte> ();

			int endOffset = offset + length;
			while (offset < endOffset)
			{
				if (!_stdinHeader.HasValue)
				{
					FastCGIHeader header = new FastCGIHeader (buffer, offset);
					offset += FastCGIHeader.LENGTH;
					if (header.RequestId != 0)
					{
						if (header.Type == FastCGIHeader.TYPE.STDIN)
						{
							_stdinHeader = header;
						} else if (header.Type == FastCGIHeader.TYPE.ABORT_REQUEST)
						{
							AbortRequest = true;
						}
					}
				}

				if (_stdinHeader.HasValue)
				{
					if (_stdinHeader.Value.ContentLength == 0)
					{
						// empty stdin = last stdin = endofrequest
						EndOfRequest = true;
						break;
					} else if (_stdinHeader.Value.RequestId != 0 && endOffset > offset)
					{
						int contentLength = Math.Min (_stdinHeader.Value.ContentLength - _stdinDataReceivedLength, endOffset - offset);
						if (contentLength > 0)
						{
							for (int i = offset; i < offset + contentLength; i++)
							{
								buffer [replaceOffset] = buffer [i];
								replaceOffset += 1;
							}
						
							offset += contentLength;
						
							_stdinDataReceivedLength += contentLength;
						} else
						{
							// empty stdin = last stdin = endofrequest
							EndOfRequest = true;
							break;
						}

						if (_stdinDataReceivedLength == _stdinHeader.Value.ContentLength)
						{
							offset += _stdinHeader.Value.PaddingLength;
							_stdinHeader = null;
							_stdinDataReceivedLength = 0;
						}
					}
				}
			}
			return replaceOffset;
		}

        #endregion

        #region HTTP => FCGI

        public byte[] TranslateToFCGIResponse(byte[] responseData)
        {
//            int paddingLength = (int)Math.Ceiling(responseData.Length / 8.0) * 8 - responseData.Length;
//            byte[] fcgiRespData = new byte[FastCGIHeader.LENGTH + responseData.Length + paddingLength + FastCGIHeader.LENGTH + EndRequestBody.LENGTH];

			int maxChunkLength = ushort.MaxValue - 8;
			int lastChunkLength;

			int chuncksCount = Math.DivRem(responseData.Length, maxChunkLength, out lastChunkLength) + 1;
			int padding = 8 - (lastChunkLength % 8);
			if (padding == 8)
				padding = 0;

			byte[] fcgiRespData = new byte[(chuncksCount - 1) * (maxChunkLength + FastCGIHeader.LENGTH) 
			                               + FastCGIHeader.LENGTH + lastChunkLength + padding 
			                               + FastCGIHeader.LENGTH + EndRequestBody.LENGTH];

			for (int i=0; i<chuncksCount; i++)
			{
				int contentLength = i == chuncksCount - 1 ? lastChunkLength : maxChunkLength;
				int paddingLength = i == chuncksCount - 1 ? padding : 0;

				FastCGIHeader stdoutHeader = new FastCGIHeader ();
				stdoutHeader.RequestId = Request.BeginRequestRecord.Header.RequestId;
				stdoutHeader.Type = FastCGIHeader.TYPE.STDOUT;
				stdoutHeader.Version = Request.BeginRequestRecord.Header.Version;
				stdoutHeader.ContentLength = (ushort)contentLength;
				stdoutHeader.PaddingLength = (byte)paddingLength;
				stdoutHeader.Reserved = 0;

				// copy stdout header
				stdoutHeader.CopyBytesTo (fcgiRespData, i * (FastCGIHeader.LENGTH + maxChunkLength));

				// copy stdout body
				Array.Copy (responseData, i * maxChunkLength, fcgiRespData, i * (FastCGIHeader.LENGTH + maxChunkLength) + FastCGIHeader.LENGTH, contentLength);
			}

            // copy endrequest record header & body
			int endReqRecOffset = fcgiRespData.Length - (FastCGIHeader.LENGTH + EndRequestBody.LENGTH);
            GetEndRequestRecord(0, EndRequestBody.PROTOCOL_STATUS.FCGI_REQUEST_COMPLETE)
                               .CopyBytesTo(fcgiRespData, endReqRecOffset);

            return fcgiRespData;
        }

        public EndRequestRecord GetEndRequestRecord(int appStatus, byte protocolStatus)
        {
            FastCGIHeader header = new FastCGIHeader();
            header.RequestId = Request.BeginRequestRecord.Header.RequestId;
            header.Type = FastCGIHeader.TYPE.END_REQUEST;
            header.Version = Request.BeginRequestRecord.Header.Version;
            header.ContentLength = EndRequestBody.LENGTH;
            header.PaddingLength = 0;
            header.Reserved = 0;

            EndRequestBody body = new EndRequestBody();
            body.AppStatus = appStatus;
            body.ProtocolStatus = protocolStatus;

            return new EndRequestRecord()
            {
                Headr = header,
                Body = body
            };
        }

        #endregion
    }
}
