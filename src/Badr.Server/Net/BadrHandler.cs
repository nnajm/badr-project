//
// BadrHandler.cs
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
using System.Net.Sockets;
using Badr.Server.Urls;
using Badr.Server.Net;
using Badr.Server.Middlewares;
using Badr.Net.Http;
using Badr.Net.Http.Request;
using Badr.Net.Http.Response;
using log4net;
using Badr.Server.Templates;
using Badr.Server.Views;

namespace Badr.Server.Net
{

	public class BadrHandler: IHttpHandler
	{
        private static readonly ILog _Logger = LogManager.GetLogger(typeof(BadrHandler));

		BadrServer _badrServer;

		public BadrHandler(BadrServer server)
		{
			_badrServer = server;
		}

		public HttpResponse Handle (HttpRequest request)
		{
			return Handle(request as BadrRequest);
		}

        public HttpResponse Handle(BadrRequest request)
        {
            string exceptionMessage = null;
            string errorMessage;
            BadrResponse response = null;
            SiteManager siteManager = null;

            try
            {
                if (request == null)
                    throw new Exception("Request is not a BadrRequest");

                if (request.Valid && request.Headers.ContainsKey(HttpRequestHeaders.Host))
                {
                    siteManager = _badrServer.GetSiteManager(request.Headers[HttpRequestHeaders.Host]);
                    if (siteManager != null)
                    {
                        request.SiteManager = siteManager;

                        MiddlewareProcessStatus middlewarePreProcessStatus = siteManager.MiddlewareManager.PreProcess(request, out errorMessage);
                        if ((middlewarePreProcessStatus & MiddlewareProcessStatus.Stop) == MiddlewareProcessStatus.Stop)
                            exceptionMessage = string.Format("Request pre-processing error: {0}", errorMessage);
                        else
                        {
                            ViewUrl viewUrl = siteManager.UrlsManager.GetViewUrl(request.Resource);
                            if (viewUrl != null)
                            {
                                request.ViewUrl = viewUrl;
                                response = viewUrl.View(request, viewUrl.GetArgs(request.Resource));
                            }
                            else
                                exceptionMessage = string.Format("Unknown resource url: {0}", request.Resource);

                            if (response != null)
                                if (!siteManager.MiddlewareManager.PostProcess(request, response, out errorMessage))
                                    exceptionMessage = string.Format("Request post-processing error: {0}", errorMessage);
                        }
                    }
                    else
                        throw new Exception(string.Format("Unknown host '{0}'", request.Headers[HttpRequestHeaders.Host]));
                }

                if (exceptionMessage != null)
                {
                    if (siteManager != null && siteManager.SiteSettings.DEBUG)
                        throw new Exception(exceptionMessage);
                    else
                    {
                        _Logger.Error(exceptionMessage);
                        return HttpResponse.CreateResponse(request, HttpResponseStatus._404);
                    }
                }
                else
                    return response;
            }
            catch (Exception ex)
            {
                _Logger.Error(ex.Message, ex);

                if (siteManager != null && siteManager.SiteSettings.DEBUG)
                    return BadrResponse.CreateDebugResponse(request, ex);
                else
                    return HttpResponse.CreateResponse(request, HttpResponseStatus._404);
            }
        }
	}
}

