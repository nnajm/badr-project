//
// MiddlewareBase.cs
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
	[Flags]
	public enum MiddlewareProcessStatus
	{
		Continue = 1,
		Error = 2,
		Stop = 4,
		ErrorStop = MiddlewareProcessStatus.Error | MiddlewareProcessStatus.Stop,
		ErrorContinue = MiddlewareProcessStatus.Error | MiddlewareProcessStatus.Continue,
	}

    public abstract class MiddlewareBase
    {
        public MiddlewareBase()
        {
        }

        public abstract MiddlewareProcessStatus PreProcess(BadrRequest request, out string errorMessage);
        public abstract bool PostProcess(BadrRequest request, BadrResponse response, out string errorMessage);

        public virtual bool ResolveSpecialTag(BadrRequest request, string spetagName, out string result)
        {
            result = "";
            return false;
        }
    }

}
