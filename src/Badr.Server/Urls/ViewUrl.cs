//
// ViewUrl.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Badr.Server.Views;
using Badr.Server.Templates;
using Badr.Server.Settings;

namespace Badr.Server.Urls
{
    public class ViewUrl
    {
        private string _url;
		private string _urlRe;
        private Regex _urlArgumentRe = new Regex(@"(?<GRP>\([^\)]+\))", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private bool _isSimpleUrl;

        public ViewUrl(string re, ViewHandler view, string name = null)
        {
			_urlRe = re;
            _url = _urlRe.TrimStart('^').TrimEnd('$');

            if (view != null)
            {
                MethodInfo viewMethodInfo = view.Method;

                ViewName = name ?? (viewMethodInfo.DeclaringType.FullName + "." + viewMethodInfo.Name);
                RE = new Regex(re, RegexOptions.Compiled);
                _isSimpleUrl = !_urlArgumentRe.IsMatch(_url);
                View = view;
                

                Attribute[] templateAttributes = (Attribute[])viewMethodInfo.GetCustomAttributes(typeof(TemplateAttribute), true);
                if (templateAttributes != null && templateAttributes.Length > 0)
                    TemplatePath = (templateAttributes[0] as TemplateAttribute).TemplatePath;
            }
        }

		public string Build (List<object> args)
		{
            string result = _url;

            if (args != null && args.Count > 0)
            {
                int i = 0;

                while (i < args.Count)
                {
                    result = _urlArgumentRe.Replace(result, (args[i] ?? "").ToString(), 1);
                    i++;
                }

                return "/" + result;
            }
            else if (_isSimpleUrl)
                return "/" + result;

			return null;
		}

		public Regex RE { get; private set; }
		public bool IsStatic { get; internal set; }
        public ViewHandler View { get; private set; }
        public string ViewName { get; private set; }
        /// <summary>
        /// The template file path: AppName.TemplateFileName
        /// </summary>
        public string TemplatePath { get; private set; }

        public bool IsMatch(string url)
        {
            return RE.IsMatch(url);
        }

        public UrlArgs GetArgs(string url)
        {
            if (!_isSimpleUrl)
            {
                Match match = RE.Match(url);
                if (match.Groups.Count > 1)
                {
                    UrlArgs args = new UrlArgs();
                    for (int groupIndex = 1; groupIndex < match.Groups.Count; groupIndex++)
                    {
                        Group group = match.Groups[groupIndex];
                        string groupName = RE.GroupNameFromNumber(groupIndex);
                        int groupNum;
                        if (!int.TryParse(groupName, out groupNum))
                        {
                            args.Add(groupName, group.Value);
                        }
                        else
                            args.Add(group.Value);
                    }
                    return args;
                }
            }

            return null;
        }

        public override string ToString()
        {
            return string.Format("[{0}]: URL={1}, Template={2}", 
                ViewName, 
                _urlRe,
                TemplatePath);
        }

        internal string WebPrint()
        {
            return string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>",
                System.Web.HttpUtility.HtmlEncode(_urlRe),
                System.Web.HttpUtility.HtmlEncode(ViewName),
                System.Web.HttpUtility.HtmlEncode(TemplatePath));
        }
    }
}
