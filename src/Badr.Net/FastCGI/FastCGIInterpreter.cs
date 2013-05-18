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
            byte[] requestHeadersData = null;

            int remainingOffset;
            int remainingReplaceOffset;

            if (Request == null)
            {
                remainingOffset = ParseRequest(buffer, offset, length);

                requestHeadersData = Request.ToHttpData();
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

        private int TranslateRemainingData(byte[] buffer, int offset, int length, int replaceOffset)
        {
            List<byte> stdinData = new List<byte>();

            int endOffset = offset + length;
            while (offset < endOffset)
            {
                FastCGIHeader header = new FastCGIHeader(buffer, offset);
                offset += FastCGIHeader.LENGTH;
                if (header.RequestId != 0 )
                {
                    if (header.Type == FastCGIHeader.TYPE.STDIN)
                    {
                        if (header.ContentLength > 0)
                        {
                            for (int i = offset; i < offset + header.ContentLength; i++)
                            {
                                buffer[replaceOffset] = buffer[i];
                                replaceOffset += 1;
                            }

                            offset += header.ContentLength;
                        }
                        else
                        {
                            // empty stdin = last stdin = endofrequest
                            EndOfRequest = true;
                            break;
                        }
                    }
                    if (header.Type == FastCGIHeader.TYPE.ABORT_REQUEST)
                    {
                        AbortRequest = true;
                    }
                }

                offset += header.PaddingLength;
            }

            return replaceOffset;
        }

        #endregion

        #region HTTP => FCGI

        public byte[] TranslateToFCGIResponse(byte[] responseData)
        {
            int paddingLength = (int)Math.Ceiling(responseData.Length / 8.0) * 8 - responseData.Length;
            byte[] fcgiRespData = new byte[FastCGIHeader.LENGTH + responseData.Length + paddingLength + FastCGIHeader.LENGTH + EndRequestBody.LENGTH];
            
            FastCGIHeader stdoutHeader = new FastCGIHeader();
            stdoutHeader.RequestId = Request.BeginRequestRecord.Header.RequestId;
            stdoutHeader.Type = FastCGIHeader.TYPE.STDOUT;
            stdoutHeader.Version = Request.BeginRequestRecord.Header.Version;
            stdoutHeader.ContentLength = (ushort)responseData.Length;
            stdoutHeader.PaddingLength = (byte)paddingLength;
            stdoutHeader.Reserved = 0;

            // copy stdout header
            stdoutHeader.CopyBytesTo(fcgiRespData, 0);

            // copy stdout body
            Array.Copy(responseData, 0, fcgiRespData, FastCGIHeader.LENGTH, responseData.Length);

            // copy endrequest record header & body
            int endReqRecOffset = FastCGIHeader.LENGTH + responseData.Length + paddingLength;
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
