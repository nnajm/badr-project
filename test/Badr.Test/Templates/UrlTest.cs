//
// UrlTest.cs
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
using Badr.Server.Templates;
using Xunit;

namespace Badr.Test.Templates
{
	public class UrlTest: TestBase
	{
		/// <summary>
		/// Urls are declared in TestApp.Urls
		/// </summary>
		public UrlTest ()
		{
		}

		[Fact(DisplayName="template tag: {% url 'noargs_url' %}")]
		public void Url_noargs()
		{
			string tt = "{% url 'noargs_url' %}";			
			TemplateEngine te = new TemplateEngine (tt);

			Assert.Equal ("/page/", te.Render (null));			
		}

		[Fact(DisplayName="template tag: {% url 'named_arg_url' page_num=7 %}")]
		public void Url_namedArgUrl()
		{
			string tt = "{% url 'named_arg_url' page_num=7 %}";			
			TemplateEngine te = new TemplateEngine (tt);
			
			Assert.Equal ("/page/7/", te.Render (null));			
		}

		[Fact(DisplayName="template tag: {% url 'pos_arg_url' %}")]
		public void Url_posArgUrl()
		{
			string tt = "{% url 'pos_arg_url' 7 %}";			
			TemplateEngine te = new TemplateEngine (tt);
			
			Assert.Equal ("/page/7/", te.Render (null));			
		}

		[Fact(DisplayName="template tag: {% url 'named_and_pos_arg_url' product_name=product 7 %}, product=\"badr\"")]
		public void Url_namedAndPosArgUrl()
		{
			string tt = "{% url 'named_and_pos_arg_url' product_name=product 7 %}";			
			TemplateEngine te = new TemplateEngine (tt);
			TemplateContext tc = new TemplateContext();
			tc ["product"] = "badr";
			
			Assert.Equal ("/badr/page/7/", te.Render (tc));			
		}

	}
}

