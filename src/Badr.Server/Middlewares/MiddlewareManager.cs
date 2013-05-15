//
// MiddlewareManager.cs
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
using Badr.Net.Http.Response;
using Badr.Server.Net;
using Badr.Server.Settings;

namespace Badr.Server.Middlewares
{

    public class MiddlewareManager
    {
        private List<MiddlewareBase> _middlewares;
        private SiteSettings _settings;

        protected internal MiddlewareManager(SiteSettings settings)
        {
            _settings = settings;
            _middlewares = new List<MiddlewareBase>();
        }

        public void Register (Type middlewareType)
		{
            if (typeof(MiddlewareBase).IsAssignableFrom(middlewareType))
                _middlewares.Add((MiddlewareBase)Activator.CreateInstance(middlewareType, _settings));
            else
                throw new ArgumentException("middlewareType argument is either null or not of type ContextProcessorBase", "middlewareType");
        }

        public MiddlewareProcessStatus PreProcess(BadrRequest wRequest, out string errorMessage)
        {
            errorMessage = null;

            if (wRequest == null)
            {
                errorMessage = "Request is null";
                return MiddlewareProcessStatus.ErrorStop;//._501;
            }

            foreach (MiddlewareBase middleware in _middlewares)
            {
                MiddlewareProcessStatus mps = middleware.PreProcess(wRequest, out errorMessage);
                if ((mps & MiddlewareProcessStatus.Stop) == MiddlewareProcessStatus.Stop)
                    return mps;
            }

            return MiddlewareProcessStatus.Continue;
        }

        public bool PostProcess(BadrRequest wRequest, BadrResponse wResponse, out string errorMessage)
        {
            bool result = true;
            errorMessage = null;
            foreach (MiddlewareBase middleware in _middlewares)
            {
                result = middleware.PostProcess(wRequest, wResponse, out errorMessage);
                if (!result || !wResponse.Status.IsSuccess())
                    return result;
            }

            return result;
        }

        public string ResolveSpecialTag(BadrRequest wRequest, string spetagName)
        {
            string result = "";

            foreach (MiddlewareBase middleware in _middlewares)
            {
                if (middleware.ResolveSpecialTag(wRequest, spetagName, out result))
                    return result;
            }

            return result;
        }
    }
}
