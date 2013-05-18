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
        public Dictionary<string, FastCGIParam> Params { get; set; }

        public FastCGIRequest()
        {
            Params = new Dictionary<string, FastCGIParam>();
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

        public void BuildParams()
        {
            if(Params.ContainsKey("REQUEST_METHOD"))
                RequestMethod = Params["REQUEST_METHOD"].Value;

            if(Params.ContainsKey("REQUEST_URI"))
                ResourceUri = Params["REQUEST_URI"].Value;

            if (Params.ContainsKey("SERVER_PROTOCOL"))
                ServerProtocol = Params["SERVER_PROTOCOL"].Value;
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
            BuildParams();
            
            StringBuilder sb = new StringBuilder();
			sb.Append(string.Format("{0} {1} {2}\r\n", RequestMethod, ResourceUri, ServerProtocol));
            foreach (FastCGIParam param in Params.Values)
            {
                if (param.Name.StartsWith("HTTP_"))
                    sb.Append(string.Format("{0}:{1}\r\n", param.Name.Substring(5), param.Value));
            }
            sb.Append("\r\n");

            return Encoding.Default.GetBytes(sb.ToString());
        }
    }
}
