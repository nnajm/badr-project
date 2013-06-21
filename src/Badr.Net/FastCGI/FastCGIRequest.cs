//
// FastCGIRequest.cs
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
using System.Threading.Tasks;

namespace Badr.Net.FastCGI
{
    public class FastCGIRequest
    {
        public BeginRequestRecord BeginRequestRecord { get; set; }
        public DefaultMap<string, FastCGIParam> Params { get; set; }

        public FastCGIRequest()
        {
            Params = new DefaultMap<string, FastCGIParam>(FastCGIParam.Empty);
        }

        public int ParseParams(FastCGIHeader paramsHeader, byte[] buffer, int offset, int endOffset)
        {
            if (paramsHeader.ContentLength > 0)
            {
                int paramsEndOffset = Math.Min(offset + paramsHeader.ContentLength, endOffset);
                while (offset < paramsEndOffset)
                {
                    FastCGIParam param = new FastCGIParam(buffer, offset);
                    Params.Add(param.Name, param);

                    offset = param.ParamEndOffset;
                }
            }

            return offset;
        }

        public string RequestMethod { get; private set; }
        public string ResourceUri{ get; private set; }
        public string ServerProtocol { get; private set; }
		public bool IsSecure { get; private set; }

        public void ExtractMainParams ()
		{
			RequestMethod = Params [FastCGIParam.REQUEST_METHOD].Value;
			ResourceUri = Params [FastCGIParam.REQUEST_URI].Value;
			ServerProtocol = Params [FastCGIParam.SERVER_PROTOCOL].Value;

			// check https

			string httpsParamName = "HTTP_X_IS_HTTPS";

			// first, if HTTP_X_IS_HTTPS is present this means that `X-Is-Https` header was set by the client and this is not good.
			// remove it
			if(Params.Contains(httpsParamName))
				Params.Remove(httpsParamName);

			// If HTTPS was set by the reverse proxy then copy it into HTTP_X_IS_HTTPS (prefixed by HTTP_ to be recognized as header by Badr.Net.Http.HttpRequest)
			string https = Params [FastCGIParam.HTTPS].Value;
			if (https != null && https.ToLower () == "on")
			{
				Params [httpsParamName] = new FastCGIParam (httpsParamName, "on");
			}
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}{1}", BeginRequestRecord.ToString(), Environment.NewLine);
            sb.AppendFormat("PARAMS:{0}", Environment.NewLine);
            foreach (FastCGIParam param in Params.Values)
                sb.AppendFormat("{0}{1}", param.ToString(), Environment.NewLine);

            return sb.ToString();
        }

        internal byte[] ToHttpData()
        {
            ExtractMainParams();
            
            StringBuilder sb = new StringBuilder();
			sb.Append(string.Format("{0} {1} {2}\r\n", RequestMethod, ResourceUri, ServerProtocol));
            foreach (FastCGIParam param in Params.Values)
            {
                if (param.Name.StartsWith("HTTP_"))
                    sb.Append(string.Format("{0}:{1}\r\n", param.Name.Substring(5).Replace('_', '-'), param.Value));
            }
            sb.Append("\r\n");

            return Encoding.Default.GetBytes(sb.ToString());
        }
    }
}
