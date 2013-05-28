//
// BadrServer.cs
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
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Badr.Net.Http;
using Badr.Server.Settings;
using Badr.Server.Urls;
using Badr.Server.Net;
using Badr.Server.Middlewares;
using System.Reflection;
using Badr.Net;
using log4net;

namespace Badr.Server.Net
{
	public enum ServerCommands
    {
        Run,
        Syncdb,
        ResetDb,
    }

    public class BadrServer : HttpServer
    {
		public const string SERVER_NAME = "Badr server";

        protected const string DEFAULT_IP_ADDRESS = "127.0.0.1";
        protected const int DEFAULT_PORT = 8080;
        protected const int DEFAULT_MAX_CONNECTIONS = 10;

		#region Field(s)
		
        protected Dictionary<string, SiteManager> SiteManagers;
		protected IHttpHandler _badrHttpHandler;

        protected List<Type> _sites;
        protected bool _siteManagersCreated;
		
		#endregion
		
		#region constructor(s)

        /// <summary>
        /// Creates a new instance of BadrServer that will bind to: 127.0.0.1:8080
        /// </summary>
        public BadrServer()
            : this(DEFAULT_IP_ADDRESS, DEFAULT_PORT, DEFAULT_MAX_CONNECTIONS)
        {

        }

        /// <summary>
        /// Creates a new instance of BadrServer that will bind to the specified ipaddress &amp; port;
        /// </summary>
        public BadrServer(string ipaddress, int port, ServerMode mode = ServerMode.Standalone)
            : this(ipaddress, port, DEFAULT_MAX_CONNECTIONS, mode)
        {
        }

        /// <summary>
        /// Creates a new instance of BadrServer that will bind to the specified ipaddress &amp; port;
        /// </summary>
        public BadrServer(string ipaddress, int port, int maxConnectionNumber, ServerMode mode = ServerMode.Standalone)
            : base(ipaddress, port, maxConnectionNumber, mode)
        {
            _sites = new List<Type>();
            SiteManagers = new Dictionary<string, SiteManager>();
			_badrHttpHandler = new BadrHandler(this);
            Command = ServerCommands.Run;
        }

        /// <summary>
        /// Registers a SiteSettings class type to use as the definition for a hosted site.
        /// </summary>
        /// <typeparam name="TSite">The site settings class type. Must inherit from Badr.Server.Settings.SiteSettings and contains a parameterless constructor.</typeparam>
        /// <returns>And instance of the server</returns>
        public BadrServer RegisterSite<TSite>()
            where TSite: SiteSettings, new()
        {
            Type siteType = typeof(TSite);
            if (!_sites.Contains(siteType))
                _sites.Add(siteType);
            return this;
        }

        /// <summary>
        /// Registers a SiteSettings instance to use as the definition for a hosted site.
        /// </summary>
        /// <typeparam name="TSite">The site settings instance</typeparam>
        /// <returns>And instance of the server</returns>
        public BadrServer RegisterSite(SiteSettings siteSettings)
        {
            SiteManager siteManager = new SiteManager(siteSettings);
            siteManager.RegisterMiddlewares();
            siteManager.RegisterContextProcessors();
            siteManager.CreateOrmManagers();
            siteManager.LoadUrls();
            SiteManagers.Add(siteManager.SiteSettings.SITE_HOST_NAME, siteManager);

            return this;
        }

        /// <summary>
        /// Sets the server endpoint (ipaddress & port)
        /// </summary>
        /// <param name="ipaddress">server binding ip address</param>
        /// <param name="port">server binding port</param>
        public BadrServer Configure(string ipaddress, int port, ServerMode mode)
        {
            IPEndPoint = new IPEndPoint(IPAddress.Parse(ipaddress), port);
			Mode = mode;
            return this;
        }

        /// <summary>
        /// Sets the server endpoint from the command line arguments.
        /// </summary>
        public BadrServer Configure()
        {
            ParseCommandLineArgs();
            return this;
        }

        /// <summary>
        /// Sets the server endpoint from the app.config xml file.
        /// </summary>
        public BadrServer XmlConfigure()
        {
            ServerSettings badrServerSettings = (ServerSettings)System.Configuration.ConfigurationManager.GetSection("BadrServer");
            IPEndPoint = new System.Net.IPEndPoint(IPAddress.Parse(badrServerSettings.EndPoint.IPAddress), badrServerSettings.EndPoint.Port);
			Mode = badrServerSettings.EndPoint.Mode;
            foreach (SiteSettings siteSettings in badrServerSettings.Websites)
            {
                RegisterSite(siteSettings);
            }
            return this;
        }

        protected internal SiteManager GetSiteManager(string siteHostName)
        {
            if (siteHostName != null && SiteManagers.ContainsKey(siteHostName))
                return SiteManagers[siteHostName];
            return null;
        }

        protected internal void BuildSiteManagers()
        {
            if (!_siteManagersCreated)
            {
                _siteManagersCreated = true;

                foreach (Type siteType in _sites)
                {
                    if (siteType != null)
                        RegisterSite((SiteSettings)Activator.CreateInstance(siteType));
                }
            }
        }

        protected internal void ParseCommandLineArgs()
        {
            string[] args = Environment.GetCommandLineArgs();

            string ipaddress = null;
            int port = 0;
			string mode = ServerMode.Standalone.ToString();

            if (args != null && args.Length > 0)
            {
                foreach (string arg in args)
                {
                    string[] split = arg.Split(':');
                    if (split != null && split.Length > 0)
                    {
                        string argName = split[0].ToUpper();
                        string argValue = split.Length > 1 ? arg.Substring(split[0].Length + 1) : null;
                        if (argValue != null)
                        {
                            switch (argName)
                            {
                                case "-S":
                                case "--SERVER":
                                    ipaddress = argValue;
                                    break;
                                case "-P":
                                case "--PORT":
                                    port = int.Parse(argValue);
                                    break;
                                case "-M":
                                case "--MODE":
                                    mode = argValue;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            switch (argName)
                            {
                                case "SYNCDB":
                                    Command = ServerCommands.Syncdb;
                                    return;
                                case "RESETDB":
                                    Command = ServerCommands.ResetDb;
                                    return;
                                default:
                                    break;
                            }
                        }
                    }
                }
				ServerMode sMode;
				if(!Enum.TryParse<ServerMode>(mode, out sMode))
					sMode = ServerMode.Standalone;

                Configure(ipaddress, port, sMode);
            }
        }
		
		#endregion

        #region Server actions

        /// <summary>
        /// The command that the server will execute
        /// </summary>
        public ServerCommands Command { get; protected set; }

        /// <summary>
        /// Start the execution of the specified command.
        /// </summary>
        /// <param name="command"></param>
        public void Start(ServerCommands command)
        {
            Command = command;
            Start();
        }

        /// <summary>
        /// Starts (run) the server
        /// </summary>
        public override void Start()
        {
            if (!_isServerStarted)
            {
                if (Command == ServerCommands.Run)
                {
                    Console.WriteLine("registering sites...");
                    BuildSiteManagers();
                    base.Start();
                }
                else
                    switch (Command)
                    {
                        case ServerCommands.Syncdb:
                            SyncDatabase();
                            break;
                        case ServerCommands.ResetDb:
                            break;
                        default:
                            break;
                    }
            }
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public override void Stop ()
		{
				base.Stop ();
		}

        /// <summary>
        /// Creates the database if not exists and create all declared models (in each registered site) tables
        /// </summary>
        public void SyncDatabase()
        {
            BuildSiteManagers();
            foreach (SiteManager siteManager in SiteManagers.Values)
                siteManager.Syncdb();
        }

        /// <summary>
        /// When overridden in a derived class, returns an instance of NetProcessor to be used by the server
        /// </summary>
        /// <param name="socketAsyncManager">The ISocketAsyncManager to pass to processor</param>
        /// <returns>A (derived) NetProcessor instance</returns>
        protected override ISocketAsyncManager NewSocketAsyncManager(string id)
        {
            return new SocketAsyncManager(new BadrProcessor(this), id);
        }
		
		#endregion
	}
}