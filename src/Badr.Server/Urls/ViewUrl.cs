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
using Badr.Server.Utils;
using System.Web;

namespace Badr.Server.Urls
{
    public class ViewUrl
    {
		private Regex _urlArgumentsRe = new Regex(@"(?<NMD_GRP>\(\?\<(?<NMD_GRP_NAME>\w+)\>[^\)]+\))|(?<POS_GRP>\([^\?][^\)]*\))", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

		private string _urlRe;

		private int _namedArgsCount;
		private int _positionalArgsCount;
		private string _reversedUrl;
		private bool _isReversible;
        private bool _isSimpleUrl;

        public ViewUrl(string re, ViewHandler view, string name = null)
        {
			_urlRe = re;
            _reversedUrl = _urlRe.TrimStart('^').TrimEnd('$');

            if (view != null)
            {
                MethodInfo viewMethodInfo = view.Method;

                Name = name ?? (viewMethodInfo.DeclaringType.FullName + "." + viewMethodInfo.Name);
                RE = new Regex(re, RegexOptions.Compiled);
                View = view;

				string purgedUrl = CleanUrlPattern (_reversedUrl);
			
				MatchCollection mc = _urlArgumentsRe.Matches(purgedUrl);
				if(mc.Count > 0)
				{
					int currIndex = 0;
					_isSimpleUrl = false;

					_positionalArgsCount = 0;
					_namedArgsCount = 0;

					foreach(Match m in mc) 
					{
						Group group = m.Groups["NMD_GRP"];
						string groupId = null;
						if(group.Success)
						{
							_namedArgsCount++;
							groupId = "##" + m.Groups["NMD_GRP_NAME"] + "##";
						}else
						{
							group = m.Groups["POS_GRP"];
							if(group.Success)
							{
								groupId = "##pos_" + (_positionalArgsCount+1) + "##";
								_positionalArgsCount++;
							}
						}

						if(groupId != null)
						{
							int part1EndIndex = group.Index + currIndex;
							int part3StartIndex = part1EndIndex + group.Length;
							string part1 = part1EndIndex == 0 ? "" : _reversedUrl.Substring(0, part1EndIndex);
							string part3 = part3StartIndex == _reversedUrl.Length ? "" : _reversedUrl.Substring(part3StartIndex);
							_reversedUrl = part1 + groupId + part3;
							currIndex += groupId.Length - group.Length;
						}
					}
				}else
					_isSimpleUrl = true;

				try
				{
					string pattern = _reversedUrl;
					_reversedUrl = Regex.Unescape(_reversedUrl);
					_isReversible = Regex.IsMatch(_reversedUrl, pattern);
				} catch (Exception ex)
				{
					_isReversible = false;
				}

                Attribute[] templateAttributes = (Attribute[])viewMethodInfo.GetCustomAttributes(typeof(TemplateAttribute), true);
                if (templateAttributes != null && templateAttributes.Length > 0)
                    TemplatePath = (templateAttributes[0] as TemplateAttribute).TemplatePath;
            }
        }

		private string CleanUrlPattern (string urlPattern)
		{
			string purgedUrl = urlPattern.ReplaceRecur(@"\\", @"##").Replace(@"\(", "##").Replace(@"\)", "##");
			string result = "";
			int groupStart = -1;
			int innerParensCount = 0;
			for(int i=0;i<purgedUrl.Length;i++)
			{
				char c = purgedUrl[i];
				if(c == '(')
				{
					if(groupStart == -1)
					{
						groupStart = i;
						result += c;
					}
					else
					{
						innerParensCount++;
						result += "#";
					}
				}
				else if(c == ')')
				{
					if(innerParensCount == 0)
					{
						groupStart = -1;
						result += c;
					}
					else
					{
						innerParensCount--;
						result += "#";
					}
				}
				else
				{
					result += c;
				}
			}

			return result;
		}

		public string Reverse (params string[] urlArgs)
		{
			string result = _reversedUrl;
			string resultEncoded = _reversedUrl;

			if(!_isReversible)
				throw new Exception("Current url is not reversible (not all variable regex elements are encolosed in named or positional groups).");

			if (_isSimpleUrl)
			{
				return "/" + result;
			}
			else
            {
				int namedArgsCount = 0;
				int posArgsCount = 0;

				if (urlArgs != null && urlArgs.Length > 0)
				{
					int i = 0;

	                while (i < urlArgs.Length)
	                {
						string[] argi = urlArgs[i].Split('=');
						string argName = argi[0];
						string argValue = urlArgs[i].Substring(argName.Length+1);
						string groupid;

						int argPos;
						if(int.TryParse(argName, out argPos))
						{
							if(argPos < 1 || argPos > _positionalArgsCount)
								throw new Exception("Url argument position out of range.");

							groupid = "##pos_"+argPos+"##";
							posArgsCount++;
						}else{

							groupid = "##" + argName + "##";

							if(!result.Contains(groupid))
								throw new Exception(string.Format("Url argument named '{0}' not found.", argName));

							namedArgsCount++;
						}

						resultEncoded = resultEncoded.Replace(groupid, HttpUtility.UrlPathEncode(argValue));
						result = result.Replace(groupid, argValue);
	                    i++;
	                }
				}

				if(namedArgsCount < _namedArgsCount || posArgsCount < _positionalArgsCount)
					throw new ArgumentException("Reverse not possible: insufficient url arguments passed to this function.", "urlArgs");

				if(IsMatch(result))
					return "/" + resultEncoded;

				throw new Exception("Reverse not possible, check url definition.");
            }
		}

		public Regex RE { get; private set; }
		public bool IsStatic { get; internal set; }
        public ViewHandler View { get; private set; }
        public string Name { get; private set; }
        /// <summary>
        /// The template file path
        /// </summary>
        public string TemplatePath { get; private set; }

        public bool IsMatch(string url)
        {
            return RE.IsMatch(HttpUtility.UrlDecode(url));
        }

        public UrlArgs GetArgs(string url)
        {
			UrlArgs args = new UrlArgs() { UrlName = this.Name };

            if (!_isSimpleUrl)
            {
                Match match = RE.Match(HttpUtility.UrlDecode(url));
                if (match.Groups.Count > 1)
                {
                    for (int groupIndex = 1; groupIndex < match.Groups.Count; groupIndex++)
                    {
                        Group group = match.Groups[groupIndex];
                        if (group.Success)
                        {
                            string groupName = RE.GroupNameFromNumber(groupIndex);
                            int groupNum;
                            if (!int.TryParse(groupName, out groupNum))
                            {
                                args.Add(groupName, group.Value);
                            }
                            else
                                args.Add(group.Value);
                        }
                    }
                    return args;
                }
            }

            return args;
        }

        public override string ToString()
        {
            return string.Format("[{0}]: URL={1}, Template={2}", 
                Name, 
                _urlRe,
                TemplatePath);
        }

        internal string WebPrint()
        {
            return string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>",
                System.Web.HttpUtility.HtmlEncode(_urlRe),
                System.Web.HttpUtility.HtmlEncode(Name),
                System.Web.HttpUtility.HtmlEncode(TemplatePath));
        }
    }
}
