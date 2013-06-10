//
// Views.cs
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
using Badr.Net.Http.Response;
using Badr.Server;
using Badr.Server.Net;
using Badr.Server.Settings;
using Badr.Server.Urls;
using Badr.Server.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badr.Apps.Static
{
    public static class Views
    {
        internal const string STATIC_RESOURCE_GROUP_NAME = "STATIC_RESOURCE_GROUP";

        private static FilesManager _staticFilesManager;
		private static FilesManager StaticFilesManager
		{
			get
			{
				if(_staticFilesManager == null)
					_staticFilesManager = new FilesManager(SiteManager.Settings.STATIC_ROOT);

				return _staticFilesManager;
			}
		}

        public static BadrResponse ServeStaticFiles(BadrRequest request, UrlArgs args = null)
        {
            string resourcePath = null;
            if (args != null && (resourcePath = args[STATIC_RESOURCE_GROUP_NAME]) != null)
            {
				bool reloadFile = true;
				bool conditionalGet = request.Headers.ContainsKey(Badr.Net.Http.Request.HttpRequestHeaders.IfModifiedSince);

				DateTime resourceLastModificationDate = StaticFilesManager.GetLastModificationTimeUtc(resourcePath);
				DateTime clientLastModificationDate;

				if(conditionalGet)
				{
					if(DateTime.TryParse(request.Headers[Badr.Net.Http.Request.HttpRequestHeaders.IfModifiedSince], out clientLastModificationDate))
					{
						reloadFile = resourceLastModificationDate.CompareTo(clientLastModificationDate) > 0;
					}
				}

				BadrResponse response;

                if (reloadFile){
                    response = new StaticResponse(request, MimeMapping.GetMimeMapping(resourcePath))
                    {
                        Status = HttpResponseStatus._200,
                        BodyBytes = StaticFilesManager.GetFileBytes(resourcePath)
                    };
				}
				else {
					response = new BadrResponse(request) { Status = HttpResponseStatus._304 };
				}

				response.Headers.Add(HttpResponseHeaders.LastModified, resourceLastModificationDate.ToString("r"));
				return response;
            }

            return null;
        }
    }
}
