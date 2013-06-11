//
// ServerSettings.cs
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
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Badr.Net.Http;

namespace Badr.Server.Settings
{
    public class ServerSettings : IConfigurationSectionHandler
    {
        [XmlElement("endpoint")]
        public ServerEndPoint EndPoint { get; set; }

        [XmlIgnore]
        public Dictionary<string, string> TypePrefixes { get; set; }

        [XmlIgnore]
        public SiteSettings Website { get; set; }

        public object Create(object parent, object configContext, XmlNode section)
        {
            XmlNodeList list = section.SelectNodes("endpoint");
            if (list.Count > 0)
            {

                EndPoint = new ServerEndPoint()
                {
                    IPAddress = list[0].Attributes["ipaddress"].Value,
                    Port = int.Parse(list[0].Attributes["port"].Value)
                };

				XmlAttribute modeAttr = list[0].Attributes["mode"];
				if(modeAttr != null)
					EndPoint.Mode = (ServerMode)Enum.Parse(typeof(ServerMode), modeAttr.Value, true);
				else
					EndPoint.Mode = ServerMode.Standalone;
			}

            list = section.SelectNodes("typeprefixes/prefix");
            if (list.Count > 0)
            {
                TypePrefixes = new Dictionary<string, string>();
                foreach (XmlElement elem in list)
                {
                    TypePrefixes.Add(elem.Attributes["name"].Value, elem.Attributes["value"].Value);
                }
            }

            list = section.SelectNodes("website");
            if(list.Count > 0)
                Website = SiteSettings.Deserialize(list[0].OuterXml, TypePrefixes);

            return this;
        }
    }

    [XmlRoot("endpoint")]
    public class ServerEndPoint
    {
        [XmlAttribute("IpAddress")]
        public string IPAddress { get; set; }
        
		[XmlAttribute("port")]
        public int Port { get; set; }

		[XmlAttribute("mode")]
		public ServerMode Mode { get; set; }
    }
}
