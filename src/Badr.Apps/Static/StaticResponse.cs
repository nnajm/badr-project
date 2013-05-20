//
// StaticResponse.cs
//
// Author: kadmin
//
// Copyright (c) 2013 kadmin
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
using Badr.Net.Http.Response;
using Badr.Net.Http.Request;
using Badr.Server.Net;

namespace Badr.Apps
{
	public class StaticResponse: BadrResponse
	{
		public StaticResponse (BadrRequest request, string contenttype = DEFAULT_CONTENT_TYPE, string charset = DEFAULT_CHARSET)
			:base(request, contenttype, charset)
		{
		}

		internal byte[] BodyBytes
		{
			get;
			set;
		}

		protected override byte[] GetBodyData ()
		{
			return BodyBytes;
		}
	}
}

