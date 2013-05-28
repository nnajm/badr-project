//
// FastCGIRecords.cs
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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Badr.Net.FastCGI
{
    #region FCGI_HEADER

    public struct FastCGIHeader
    {
        public const int LENGTH = 8;
        public const byte FCGI_VERSION_1 = 1;
        public const ushort NULL_REQUEST_ID = 0;

        public FastCGIHeader(byte[] data, int offset = 0)
        {
            Version = data[offset];
            Type = data[offset+1];
            RequestId = (ushort)((data[offset + 2] << 8) + data[offset + 3]);
            ContentLength = (ushort)((data[offset + 4] << 8) + data[offset + 5]);
            PaddingLength = data[offset + 6];
            Reserved = data[offset + 7];
        }


        public byte Version;
        public byte Type;
        public ushort RequestId;
        public ushort ContentLength;
        public byte PaddingLength;
        public byte Reserved;

        public static class TYPE
        {
            public const int BEGIN_REQUEST = 1;
            public const int ABORT_REQUEST = 2;
            public const int END_REQUEST = 3;
            public const int PARAMS = 4;
            public const int STDIN = 5;
            public const int STDOUT = 6;
            public const int STDERR = 7;
            public const int DATA = 8;

            public static string ToString(int recordType)
            {
                switch (recordType)
                {
                    case FastCGIHeader.TYPE.BEGIN_REQUEST: return "FCGI_BEGIN_REQUEST";
                    case FastCGIHeader.TYPE.ABORT_REQUEST: return "FCGI_ABORT_REQUEST";
                    case FastCGIHeader.TYPE.END_REQUEST: return "FCGI_END_REQUEST";
                    case FastCGIHeader.TYPE.PARAMS: return "FCGI_PARAMS";
                    case FastCGIHeader.TYPE.STDIN: return "FCGI_STDIN";
                    case FastCGIHeader.TYPE.STDOUT: return "FCGI_STDOUT";
                    case FastCGIHeader.TYPE.STDERR: return "FCGI_STDERR";
                    case FastCGIHeader.TYPE.DATA: return "FCGI_DATA";
                    case FastCGIHeader.TYPE.GET_VALUES: return "FCGI_GET_VALUES";
                    case FastCGIHeader.TYPE.GET_VALUES_RESULT: return "FCGI_GET_VALUES_RESULT";
                    case FastCGIHeader.TYPE.UNKNOWN_TYPE: return "FCGI_UNKNOWN_TYPE";
                    default:
                        return "";
                }
            }

            /* management types */
            public const int GET_VALUES = 9;
            public const int GET_VALUES_RESULT = 10;

            public const int UNKNOWN_TYPE = 11;
            public const int FCGI_MAXTYPE = UNKNOWN_TYPE;
        }

        public static class MANAGEMENT_VARIABLE
        {
            public const string FCGI_MAX_CONNS = "FCGI_MAX_CONNS";
            public const string FCGI_MAX_REQS = "FCGI_MAX_REQS";
            public const string FCGI_MPXS_CONNS = "FCGI_MPXS_CONNS";
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("VERSION:{0}{1}", Version, Environment.NewLine);
            sb.AppendFormat("Type:{0}{1}", TYPE.ToString(Type), Environment.NewLine);
            sb.AppendFormat("RequestId:{0}{1}", RequestId, Environment.NewLine);
            sb.AppendFormat("ContentLength:{0}{1}", ContentLength, Environment.NewLine);
            sb.AppendFormat("PaddingLength:{0}{1}", PaddingLength, Environment.NewLine);            

            return sb.ToString();
        }

        internal void CopyBytesTo(byte[] buffer, int copyOffset)
        {
            buffer[copyOffset] = Version;
            buffer[copyOffset + 1] = Type;
            
            buffer[copyOffset + 2] = (byte)((RequestId & 0xFF00) >> 8);
            buffer[copyOffset + 3] = (byte)(RequestId & 0xFF);

            buffer[copyOffset + 4] = (byte)((ContentLength & 0xFF00) >> 8);
            buffer[copyOffset + 5] = (byte)(ContentLength & 0xFF);

            
            buffer[copyOffset + 6] = PaddingLength;
            buffer[copyOffset + 7] = Reserved;
        }
    }

    #endregion

    #region BeginRequest Record

    public struct BeginRequestBody
    {
        public const int LENGTH = 8;
        public const int RESERVED_LENGTH = 5;
        public const byte FCGI_KEEP_CONN_FLAG = 1;

        public BeginRequestBody(byte[] data, int offset = 0)
        {
            Role = (ushort)((data[offset] << 8) + data[offset + 1]);
            Flags = data[offset + 2];

            Reserved = new byte[RESERVED_LENGTH];
            Array.Copy(data, offset + 3, Reserved, 0, RESERVED_LENGTH);
        }

        public ushort Role;
        public byte Flags;
        public byte[] Reserved;

        public static class ROLE
        {
            public const ushort RESPONDER = 1;
            public const ushort AUTHORIZER = 2;
            public const ushort FILTER = 3;

            public static string ToString(int role)
            {
                switch (role)
                {
                    case RESPONDER: return "ROLE_RESPONDER";
                    case AUTHORIZER: return "ROLE_AUTHORIZER";
                    case FILTER: return "ROLE_FILTER";
                    default:
                        return "";
                        break;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("ROLE:{0}{2}FLAGS:{1}{2}", ROLE.ToString(Role), Flags, Environment.NewLine);
        }
    }

    public struct BeginRequestRecord
    {
        public FastCGIHeader Header;
        public BeginRequestBody Body;

        public override string ToString()
        {
            return string.Format("BeginRequestRecord:{2}HEADER:{2}{0}{2}BODY:{2}{1}",
                Header.ToString(),
                Body.ToString(),
                Environment.NewLine);
        }
    }

    #endregion

    #region EndRequest Record

    public struct EndRequestBody
    {
        public const int LENGTH = 8;
        public const int RESERVED_LENGTH = 3;

        public EndRequestBody(byte[] data, int offset = 0)
        {
            AppStatus = (ushort)((data[offset] << 24) + (data[offset + 1] << 16) + (data[offset + 2] << 8) + data[offset + 3]);
            ProtocolStatus = data[offset + 4];
            
            Reserved = new byte[RESERVED_LENGTH];
            Array.Copy(data, offset + 5, Reserved, 0, RESERVED_LENGTH);
        }

        public int AppStatus;
        public byte ProtocolStatus;
        public byte[] Reserved;

        public static class PROTOCOL_STATUS
        {
            public const int FCGI_REQUEST_COMPLETE = 0;
            public const int FCGI_CANT_MPX_CONN = 1;
            public const int FCGI_OVERLOADED = 2;
            public const int FCGI_UNKNOWN_ROLE = 3;
        }

        internal void CopyBytesTo(byte[] buffer, int copyOffset)
        {
            buffer[copyOffset + 0] = (byte)((AppStatus & 0xFF000000) >> 24);
            buffer[copyOffset + 1] = (byte)((AppStatus & 0x00FF0000) >> 16);
            buffer[copyOffset + 2] = (byte)((AppStatus & 0x0000FF00) >> 8);
            buffer[copyOffset + 3] = (byte)(AppStatus & 0xFF);

            buffer[copyOffset + 4] = ProtocolStatus;
            buffer[copyOffset + 5] = 0;
            buffer[copyOffset + 6] = 0;
            buffer[copyOffset + 7] = 0;            
        }
    }

    public struct EndRequestRecord
    {
        public FastCGIHeader Headr;
        public EndRequestBody Body;

        internal void CopyBytesTo(byte[] buffer, int copyOffset)
        {
            Headr.CopyBytesTo(buffer, copyOffset);
            Body.CopyBytesTo(buffer, copyOffset + FastCGIHeader.LENGTH);
        }
    }

    #endregion

    #region UnknownType Record

    public struct UnknownTypeBody
    {
        public const int LENGTH = 78;
        public const int RESERVED_LENGTH = 7;

        public UnknownTypeBody(byte[] data, int offset = 0)
        {
            Type = data[offset];

            Reserved = new byte[RESERVED_LENGTH];
            Array.Copy(data, offset + 1, Reserved, 0, RESERVED_LENGTH);
        }

        public byte Type;
        public byte[] Reserved;
    }

    public struct UnknownTypeRecord
    {
        public FastCGIHeader Headr;
        public UnknownTypeBody Body;
    }

    #endregion
}
