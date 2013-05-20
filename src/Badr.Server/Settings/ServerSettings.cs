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
        public SiteSettings[] Websites { get; set; }

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
					EndPoint.Mode = ServerMode.Local;
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

            list = section.SelectNodes("websites/website");
            int listCount = list.Count;
            Websites = new SiteSettings[listCount];

            for (int i = 0; i < listCount; i++)
                Websites[i] = SiteSettings.Deserialize(list[i].OuterXml, TypePrefixes);

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
