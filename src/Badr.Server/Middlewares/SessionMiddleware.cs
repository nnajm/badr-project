//
// SessionMiddleware.cs
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
using System.Linq;
using System.Text;
using Badr.Server.Net;
using Badr.Net.Http.Request;
using Badr.Net.Http.Response;
using Badr.Server.Settings;
using Badr.Net.Http;

namespace Badr.Server.Middlewares
{
    public class SessionMiddleware : MiddlewareBase
    {
        protected const string SESSION_SPE_TAG_NAME = "sessiontoken";

        public override MiddlewareProcessStatus PreProcess(BadrRequest request, out string errorMessage)
        {
            string sessionId = request.Cookies[CookieNames.SESSION_ID].Value;
			bool sessionReceived = !string.IsNullOrWhiteSpace(sessionId);

            if (!sessionReceived)
                sessionId = Security.GenerateId(24);

            request.Session = new BadrSession(sessionId) { SendCookie = !sessionReceived };

            errorMessage = null;
			return MiddlewareProcessStatus.Continue;
        }

        public override bool PostProcess(BadrRequest request, BadrResponse response, out string errorMessage)
        {
			CookiesSettings cookiesSettings = SiteManager.Settings.Cookies;
            errorMessage = null;

            if ((!cookiesSettings.SessionSecure || request.IsSecure)
				&& response.Status.IsSuccess() && request.Session != null && request.Session.SendCookie)
            {
				HttpCookieFragment sessionFragment = new HttpCookieFragment(
                    name: CookieNames.SESSION_ID,
                    value: request.Session.ID,
                    path: "/",
                    domain: request.DomainUri.Host);

				if(cookiesSettings != null)
				{
					if(!SiteManager.Settings.Cookies.SessionExpireAtBrowserClose)
						sessionFragment[HttpCookieFragment.ATTR_MAX_AGE] = cookiesSettings.SessionAge.ToString();

					sessionFragment.IsSecure = cookiesSettings.SessionSecure;
					sessionFragment.IsHttpOnly = cookiesSettings.SessionHttpOnly;
				}

				response.Cookies[CookieNames.SESSION_ID] = sessionFragment;
            }

            return true;
        }

        public override bool ResolveSpecialTag(BadrRequest request, string spetagName, out string result)
        {
			if (request != null && spetagName == SESSION_SPE_TAG_NAME)
            {
                result = request.Session.ID;
                return true;
            }

            return base.ResolveSpecialTag(request, spetagName, out result);
        }
    }
}
