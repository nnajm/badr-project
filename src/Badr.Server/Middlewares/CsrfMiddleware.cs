//
// CsrfMiddleware.cs
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
using log4net;
using Badr.Server.Settings;
using Badr.Net.Http;

namespace Badr.Server.Middlewares
{
    public class CsrfMiddleware : MiddlewareBase
    {
        private static readonly ILog _Logger = LogManager.GetLogger(typeof(CsrfMiddleware));

        protected const string CSRF_INPUT_NAME = CookieNames.CSRF_TOKEN + "_spetag";
        protected const string CSRF_SPE_TAG_NAME = "csrftoken";

        public override MiddlewareProcessStatus PreProcess(BadrRequest request, out string errorMessage)
        {
            if (!HttpRequest.IsSafeMethod(request.Method))
            {
                if (!request.POST.Contains(CSRF_INPUT_NAME)
                    || request.CsrfToken != request.POST[CSRF_INPUT_NAME].ToString())
                {
                    errorMessage = "POST request does not contain valid csrf token";
                    _Logger.Error(errorMessage);
					return MiddlewareProcessStatus.ErrorStop;// WResponseStatus._403;
                }
            }

            errorMessage = null;
            return MiddlewareProcessStatus.Continue;
        }

        public override bool PostProcess(BadrRequest request, BadrResponse response, out string errorMessage)
        {
            errorMessage = null;
            if ((!SiteManager.Settings.Cookies.CsrfSecure || request.IsSecure)
				 && response.Status.IsSuccess() && IsValidCsrf(request))
            {
				HttpCookieFragment csrfFragment = new HttpCookieFragment(
                    name: CookieNames.CSRF_TOKEN,
                    value: request.CsrfToken,
                    path: "/",
                    domain: request.DomainUri.Host);

				if(SiteManager.Settings.Cookies != null)
				{
					csrfFragment.IsSecure = SiteManager.Settings.Cookies.CsrfSecure;
				}

				response.Cookies[CookieNames.CSRF_TOKEN] = csrfFragment;
            }

            return true;
        }

        public override bool ResolveSpecialTag(BadrRequest request, string spetagName, out string result)
        {
			if (request != null && spetagName == CSRF_SPE_TAG_NAME)
            {
                if (!IsValidCsrf(request))
                    request.CsrfToken = Security.GenerateId(24);

                result = string.Format("<input type=\"hidden\" name=\"{0}\" value=\"{1}\"/>", CSRF_INPUT_NAME, request.CsrfToken);
                return true;
            }

            return base.ResolveSpecialTag(request, spetagName, out result);
        }

        private bool IsValidCsrf(BadrRequest request)
        {
            return !(string.IsNullOrEmpty(request.CsrfToken) || request.CsrfToken.Trim() == "");
        }
    }
}
