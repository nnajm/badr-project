//
// UrlsTest.cs
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
using Badr.Server.Urls;
using Xunit;
using Badr.Server.Net;

namespace Badr.Orm.Test
{
	public class UrlsTest
	{
		public UrlsTest ()
		{
		}

		[Fact(DisplayName="Reversing urls")]
		public void ReversingUrls()
		{
			ViewUrl url1 = new ViewUrl(@"^product/(?<category>\w+)/(\d+)/$", View1, "url1");
			ViewUrl url2 = new ViewUrl(@"^product/(?<category>(?:[\w\s]+))/\((\d+)\)/(\w+)/$", View1, "url2");
			ViewUrl url3 = new ViewUrl(@"^product/(?<category>\w+)/\d+/$", View1, "url3");

			string revUrl1 = url1.Reverse("category=green", "1=7");
			string revUrl2 = url2.Reverse("category=green and yellow", "1=7", "2=page");

			Assert.Equal("/product/green/7/", revUrl1);
			Assert.Equal("/product/green%20and%20yellow/(7)/page/", revUrl2);
			Assert.Throws(typeof(Exception), () => {
				// positional argument 1 = "page" is not permitted because it must be numeric
				url2.Reverse("category=green", "1=page", "2=7");
			});
			Assert.Throws(typeof(Exception), () => {
				// url3 is not reversible because \d+ at the end was not enclosed in a group and can not be resolved
				url3.Reverse("category=green", "1=7");
			});
		}

		[Fact(DisplayName="Matching urls")]
		public void MatchingUrls()
		{
			ViewUrl url1 = new ViewUrl(@"^product/(?<category>\w+)/(\d+)/$", View1, "url1");
			ViewUrl url2 = new ViewUrl(@"^product/(?<category>(?:[\w\s]+))/(\d+)/$", View1, "url2");
			ViewUrl url3 = new ViewUrl(@"^product/(?<category>\w+)/\((\d+)\)/$", View1, "url2");

			Assert.True(url1.IsMatch("product/new/945/"));
			Assert.False(url1.IsMatch("product/categrory 1/15/"));
			Assert.False(url1.IsMatch("product/categrory%201/15/"));

			Assert.True(url2.IsMatch("product/new and old/125/"));
			Assert.True(url2.IsMatch("product/new%20and old/125/"));
			Assert.True(url2.IsMatch("product/new%20and+old/125/"));
			Assert.False(url2.IsMatch("product/new% 20and old/125/"));

			Assert.True(url3.IsMatch("product/old/(47)/"));
			Assert.False(url3.IsMatch("product/1/"));
		}

		private BadrResponse View1(BadrRequest request, UrlArgs args) {
			return null;
		}
	}
}

