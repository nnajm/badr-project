//
// ExprMatchGroups.cs
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Badr.Server.Templates.Rendering
{
    public class ExprMatchGroups
    {
        private static readonly Regex VVF_REGEX = new Regex(BadrGrammar.VARIABLE_VALUE_FILTERED, System.Text.RegularExpressions.RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private readonly Dictionary<string, string> _singleMatchGroups;
        private readonly Dictionary<string, List<string>> _multiMatchGroups;

        public ExprMatchGroups()
        {
            _singleMatchGroups = new Dictionary<string, string>();
            _multiMatchGroups = new Dictionary<string, List<string>>();
        }

        public void Clear()
        {
            _singleMatchGroups.Clear();
        }

        public void CopyTo(ExprMatchGroups emg)
        {
            if (emg != null)
            {
                emg.Clear();
                foreach (KeyValuePair<string, List<string>> group in _multiMatchGroups)
                    emg._multiMatchGroups.Add(group.Key, new List<string>(group.Value));
                foreach (KeyValuePair<string, string> group in _singleMatchGroups)
                    emg._singleMatchGroups.Add(group.Key, group.Value);
            }
        }

        public void Add(string group, string value)
        {
            if (_multiMatchGroups.ContainsKey(group))
            {
                _multiMatchGroups[group].Add(value);
            }
            else if (!_singleMatchGroups.ContainsKey(group))
            {
                _singleMatchGroups.Add(group, value);
            }
            else
            {
                _multiMatchGroups.Add(group, new List<string>());
                _multiMatchGroups[group].Add(_singleMatchGroups[group]);
                _multiMatchGroups[group].Add(value);

                _singleMatchGroups.Remove(group);
            }
        }

        public string this[string group]
        {
            get
            {
                if (_singleMatchGroups.ContainsKey(group))
                    return _singleMatchGroups[group];
                return null;
            }
        }

        public bool Contains(string group)
        {
            return _multiMatchGroups.ContainsKey(group)
                || _singleMatchGroups.ContainsKey(group);
        }

        public List<ExprMatchVar> GetVariableAndFilteres(string variableGroup)
        {
            List<ExprMatchVar> variableMatches = new List<ExprMatchVar>();
            if (Contains(variableGroup))
            {
                foreach (string varGroupMatch in GetMatchesList(variableGroup))
                {
                    MatchCollection mcs = VVF_REGEX.Matches(varGroupMatch);
                    if (mcs != null)
                    {
                        for (int i = 0; i < mcs.Count; i++)
                        {
                            Match match = mcs[i];
                            if (match.Success)
                                variableMatches.Add(new ExprMatchVar(match));
                        }
                    }
                }
            }

            return variableMatches;
        }

        public List<string> GetMatchesList(string group)
        {
            if (_multiMatchGroups.ContainsKey(group))
                return _multiMatchGroups[group];
            else
                if (_singleMatchGroups.ContainsKey(group))
                    return new List<string>(new string[] { _singleMatchGroups[group] });

            return null;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> group in _singleMatchGroups)
            {
                sb.AppendLine(string.Format("{0}: {1}", group.Key, group.Value));
            }
            foreach (KeyValuePair<string, List<string>> group in _multiMatchGroups)
            {
                sb.AppendLine(string.Format("{0}: {1}", group.Key, string.Join(",", group.Value)));
            }
            return sb.ToString();
        }
    }
}
