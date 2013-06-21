//
// TemplatesTest.cs
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
using Xunit;
using Badr.Server.Templates;

namespace Badr.Test.Templates
{
	public class IfTest: TestBase
	{
		public IfTest ()
		{
		}

		[Fact(DisplayName="template tag: {% if c = d %}")]
		public void IfTag_equal()
		{
			string tt = "{% if a = b %}true{% else %}false{% endif %}";
			
			TemplateEngine te = new TemplateEngine (tt);
			TemplateContext tc = new TemplateContext ();
			tc ["a"] = 1;			
			tc ["b"] = 1;
			Assert.Equal ("true", te.Render (tc));

			tc ["b"] = 2;
			Assert.Equal ("false", te.Render (tc));
		}

		[Fact(DisplayName="template tag: {% if c != d %}")]
		public void IfTag_notequal()
		{
			string tt = "{% if a != b %}true{% else %}false{% endif %}";
			
			TemplateEngine te = new TemplateEngine (tt);
			TemplateContext tc = new TemplateContext ();
			tc ["a"] = 1;			
			tc ["b"] = 1;
			Assert.Equal ("false", te.Render (tc));
			
			tc ["b"] = 2;
			Assert.Equal ("true", te.Render (tc));
		}

		[Fact(DisplayName="template tag: {% if c > d %}")]
		public void IfTag_greaterThan()
		{
			string tt = "{% if a > b %}true{% else %}false{% endif %}";
			
			TemplateEngine te = new TemplateEngine (tt);
			TemplateContext tc = new TemplateContext ();
			tc ["a"] = 1;			
			tc ["b"] = 1;
			Assert.Equal ("false", te.Render (tc));
			
			tc ["a"] = 2;
			Assert.Equal ("true", te.Render (tc));
		}

		[Fact(DisplayName="template tag: {% if c < d %}")]
		public void IfTag_lessThan()
		{
			string tt = "{% if a < b %}true{% else %}false{% endif %}";
			
			TemplateEngine te = new TemplateEngine (tt);
			TemplateContext tc = new TemplateContext ();
			tc ["a"] = 1;			
			tc ["b"] = 1;
			Assert.Equal ("false", te.Render (tc));
			
			tc ["b"] = 2;
			Assert.Equal ("true", te.Render (tc));
		}

		[Fact(DisplayName="template tag: {% if c >= d %}")]
		public void IfTag_greaterOrEqualThan()
		{
			string tt = "{% if a >= b %}true{% else %}false{% endif %}";
			
			TemplateEngine te = new TemplateEngine (tt);
			TemplateContext tc = new TemplateContext ();
			tc ["a"] = 1;			
			tc ["b"] = 1;
			Assert.Equal ("true", te.Render (tc));
			
			tc ["a"] = 2;
			Assert.Equal ("true", te.Render (tc));
			
			tc ["a"] = 0.7;
			Assert.Equal ("false", te.Render (tc));
		}

		[Fact(DisplayName="template tag: {% if c <= d %}")]
		public void IfTag_lessOrEqualThan()
		{
			string tt = "{% if a <= b %}true{% else %}false{% endif %}";
			
			TemplateEngine te = new TemplateEngine (tt);
			TemplateContext tc = new TemplateContext ();
			tc ["a"] = 1;			
			tc ["b"] = 1;
			Assert.Equal ("true", te.Render (tc));
			
			tc ["b"] = 2;
			Assert.Equal ("true", te.Render (tc));
			
			tc ["b"] = 0.7;
			Assert.Equal ("false", te.Render (tc));
		}

		[Fact(DisplayName="template tag: {% if c in d %}")]
		public void IfTag_in()
		{
			string tt = "{% if a in b %}true{% else %}false{% endif %}";
			
			TemplateEngine te = new TemplateEngine (tt);
			TemplateContext tc = new TemplateContext ();
			tc ["a"] = 1;			
			tc ["b"] = new int[]{1, 3, 7};
			Assert.Equal ("true", te.Render (tc));
			
			tc ["a"] = 2;
			Assert.Equal ("false", te.Render (tc));
		}

		[Fact(DisplayName="template tag: {% if c not in d %}")]
		public void IfTag_notIn()
		{
			string tt = "{% if a not in b %}true{% else %}false{% endif %}";
			
			TemplateEngine te = new TemplateEngine (tt);
			TemplateContext tc = new TemplateContext ();
			tc ["a"] = 2;			
			tc ["b"] = new int[]{1, 3, 7};
			Assert.Equal ("true", te.Render (tc));
			
			tc ["a"] = 1;
			Assert.Equal ("false", te.Render (tc));
		}

		[Fact(DisplayName="template tag: {% if c or d or z %}")]
		public void IfTag_or()
		{
			string tt = "{% if a = 1 or a < -50 or a in list %}true{% else %}false{% endif %}";

			TemplateEngine te = new TemplateEngine (tt);
			TemplateContext tc = new TemplateContext();
			tc ["list"] = new double[]{-11.7, -7, -1, 3, 5, 81};

			tc ["a"] = 1;
			Assert.Equal ("true", te.Render (tc));

			tc ["a"] = 81;
			Assert.Equal ("true", te.Render (tc));

			tc ["a"] = 7;
			Assert.Equal ("false", te.Render (tc));

			tc ["a"] = -2;
			Assert.Equal ("false", te.Render (tc));

			tc ["a"] = -71.7;
			Assert.Equal ("true", te.Render (tc));
		}

		[Fact(DisplayName="template tag: {% if c and d and z %}")]
		public void IfTag_and()
		{
			string tt = "{% if a < 10 and a > -2 and a in list %}true{% else %}false{% endif %}";
			
			TemplateEngine te = new TemplateEngine (tt);
			TemplateContext tc = new TemplateContext();
			tc ["list"] = new double[]{-11.7, -7, -1, 3, 5, 81};

			tc ["a"] = 3;
			Assert.Equal ("true", te.Render (tc));
			
			tc ["a"] = -1;
			Assert.Equal ("true", te.Render (tc));
			
			tc ["a"] = 9;
			Assert.Equal ("false", te.Render (tc));
			
			tc ["a"] = 81;
			Assert.Equal ("false", te.Render (tc));
			
			tc ["a"] = -11.7;
			Assert.Equal ("false", te.Render (tc));
		}

		[Fact(DisplayName="template tag: {% if c and not d %}")]
		public void IfTag_andNot()
		{
			string tt = "{% if a < 10 and not a = 5 %}true{% else %}false{% endif %}";
			
			TemplateEngine te = new TemplateEngine (tt);
			TemplateContext tc = new TemplateContext();

			tc ["a"] = 3;
			Assert.Equal ("true", te.Render (tc));
			
			tc ["a"] = -7;
			Assert.Equal ("true", te.Render (tc));
			
			tc ["a"] = 5;
			Assert.Equal ("false", te.Render (tc));
			
			tc ["a"] = 19;
			Assert.Equal ("false", te.Render (tc));
		}

		[Fact(DisplayName="template tag: {% if c and d or f and g and h or x %}")]
		public void IfTag_and_or_mix()
		{
			string tt = "{% if a < 10 and not a = 5 or a > 10 and a < 20 and not a = 15 or a = 27 %}true{% else %}false{% endif %}";
			
			TemplateEngine te = new TemplateEngine (tt);
			TemplateContext tc = new TemplateContext();
			
			tc ["a"] = 3;
			Assert.Equal ("true", te.Render (tc));
			
			tc ["a"] = 17;
			Assert.Equal ("true", te.Render (tc));

			tc ["a"] = 27;
			Assert.Equal ("true", te.Render (tc));

			tc ["a"] = -77;
			Assert.Equal ("true", te.Render (tc));
			
			tc ["a"] = 5;
			Assert.Equal ("false", te.Render (tc));
			
			tc ["a"] = 15;
			Assert.Equal ("false", te.Render (tc));

			tc ["a"] = 77;
			Assert.Equal ("false", te.Render (tc));
		}

		[Fact(DisplayName="template tag: {% if c %}")]
		public void IfTag_noRhs()
		{
			string tt = "{% if a %}true{% else %}false{% endif %}";
			
			TemplateEngine te = new TemplateEngine (tt);
			TemplateContext tc = new TemplateContext();

			tc ["a"] = true;			
			Assert.Equal ("true", te.Render (tc));

			tc ["a"] = 0;			
			Assert.Equal ("true", te.Render (tc));

			tc ["a"] = null;			
			Assert.Equal ("false", te.Render (tc));

			tc ["a"] = false;			
			Assert.Equal ("false", te.Render (tc));
		}
	}
}

