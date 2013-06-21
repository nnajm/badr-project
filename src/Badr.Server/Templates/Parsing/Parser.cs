//
// Parser.cs
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
using Badr.Server.Templates.Rendering;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Badr.Server.Templates.Parsing
{
    public class Parser
    {
        Dictionary<string, Regex> _regExs;
        Dictionary<KeyValuePair<string, string>, List<ExprMatchResult>> _cache;

        public Parser()
        {
            _regExs = new Dictionary<string, Regex>();
            _cache = new Dictionary<KeyValuePair<string, string>, List<ExprMatchResult>>();
        }

        public Parser(params string[] patterns)
            : this()
        {
            foreach (string pattern in patterns)
                AddRegex(pattern);
        }

        public void AddRegex(string pattern)
        {
            _regExs[pattern] = new Regex(pattern);
            _cache.Clear();
        }

        public List<ExprMatchResult> Match(string source, ExprType exprType, string expressionName)
        {
            var key = new KeyValuePair<string, string>(source, expressionName);
            if (_cache.ContainsKey(key))
                return _cache[key];

            List<ExprMatchResult> parserResults = new List<ExprMatchResult>();

            foreach (KeyValuePair<string, Regex> regex in _regExs)
            {
                Match match = regex.Value.Match(source);

                while (match.Success)
                {
                    Group exprGroup = match.Groups[regex.Value.GroupNumberFromName(expressionName)];
                    if (exprGroup != null && exprGroup.Success)
                    {
                        int line = source.Substring(0, exprGroup.Index).Split('\n').Length;
                        parserResults.Add(new ExprMatchResult(regex.Key, exprType, expressionName, source, exprGroup.Value, line, exprGroup.Index, exprGroup.Length));
                    }

                    match = match.NextMatch();
                }
            }

            _cache[key] = parserResults;

            return parserResults;
        }

        public class ExprMatchResult
        {
            private readonly string _tostring;

            public ExprMatchResult(string re, ExprType exprType, string exprName, string source, string match, int line, int startIndex, int length)
            {
                RE = re;
                ExprName = exprName;
                ExprType = exprType;
                Source = source;
                Match = match;
                Line = line;
                StartIndex = startIndex;
                EndIndex = startIndex + length - 1;
                Length = length;

                _tostring = string.Format("{0}: {1} (line {2})", ExprName, Match, Line);
            }

            public string RE { get; private set; }
            public string ExprName { get; private set; }
            public ExprType ExprType { get; private set; }
            public string Source { get; private set; }
            public string Match { get; private set; }
            public int Line { get; private set; }
            public int StartIndex { get; private set; }
            public int EndIndex { get; private set; }
            public int Length { get; private set; }

            public override string ToString()
            {
                return _tostring;
            }
        }
    }
}
