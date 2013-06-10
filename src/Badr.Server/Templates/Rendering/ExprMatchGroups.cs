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
        private static readonly Regex _filteredVarRegex = new Regex(BadrGrammar.VARIABLE_VALUE_FILTERED, System.Text.RegularExpressions.RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private static readonly Regex _filteredAssignationRegex = new Regex(BadrGrammar.VARIABLE_ASSIGNATION, System.Text.RegularExpressions.RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private readonly Dictionary<string, List<string>> _matchGroups;

        public ExprMatchGroups()
        {
            _matchGroups = new Dictionary<string, List<string>>();
        }

        public void Clear()
        {
            _matchGroups.Clear();
        }

        public void CopyTo(ExprMatchGroups emg)
        {
            if (emg != null)
            {
                emg.Clear();
                foreach (KeyValuePair<string, List<string>> group in _matchGroups)
                    emg._matchGroups.Add(group.Key, new List<string>(group.Value));
            }
        }

        public void Add(string group, string value)
        {
            if (!_matchGroups.ContainsKey(group))
                _matchGroups.Add(group, new List<string>());
            _matchGroups[group].Add(value);
        }

        public string GetGroupValue(string group)
        {
            if (_matchGroups.ContainsKey(group))
                return _matchGroups[group][0];
            return null;
        }

        public List<string> GetGroupValuesList(string group)
        {
            if (_matchGroups.ContainsKey(group))
                return _matchGroups[group];
            return null;
        }

        public bool Contains(string group)
        {
            return _matchGroups.ContainsKey(group);
        }

        private TemplateVarFiltered ParseVariableString(string variableString)
        {
            if (variableString != null)
            {
                MatchCollection mcs = _filteredVarRegex.Matches(variableString);
                if (mcs != null)
                {
                    Match match = mcs[0];
                    if (match.Success)
                    {
                        string varValue = match.Groups[BadrGrammar.GROUP_VARIABLE_VALUE].Value;
                        return new TemplateVarFiltered(varValue, GetFilteres(match.Groups[BadrGrammar.GROUP_VARIABLE_FILTER]));
                    }
                }
            }

            return null;
        }

        public TemplateVarFiltered GetFilteredVariable(string variableGroup)
        {
            if (Contains(variableGroup))
            {
                return ParseVariableString(GetGroupValue(variableGroup));
            }

            return null;
        }

        public List<TemplateVarFiltered> GetFilteredVariableList(string variableGroup)
        {
            if (Contains(variableGroup))
            {
                List<TemplateVarFiltered> tVars = new List<TemplateVarFiltered>();
                foreach (string variable in GetGroupValuesList(variableGroup))
                    tVars.Add(ParseVariableString(GetGroupValue(variableGroup)));
                return tVars;
            }

            return null;
        }

        public Dictionary<string, TemplateVarFiltered> GetFilteredAssignationList(string assignationGroup)
        {
            Dictionary<string, TemplateVarFiltered> assignationMatches = new Dictionary<string, TemplateVarFiltered>();
            if (Contains(assignationGroup))
            {
                foreach (string variable in GetGroupValuesList(assignationGroup))
                {
                    MatchCollection mcs = _filteredAssignationRegex.Matches(variable);
                    if (mcs != null)
                    {
                        for (int i = 0; i < mcs.Count; i++)
                        {
                            Match match = mcs[i];
                            if (match.Success)
                            {
                                string varName = match.Groups[BadrGrammar.GROUP_VARIABLE_NAME].Value;
                                assignationMatches.Add(varName, ParseVariableString(match.Groups[BadrGrammar.GROUP_ASSIGNATION_VALUE].Value));
                            }
                        }
                    }
                }
            }

            return assignationMatches;
        }

        protected List<TemplateFilter> GetFilteres(Group filtersGroup)
        {
            if (filtersGroup != null)
            {
                int capCount = filtersGroup.Captures.Count;
                if (capCount > 0)
                {
                    List<TemplateFilter> filters = new List<TemplateFilter>();
                    for (int j = 0; j < capCount; j++)
                    {
						string filter = filtersGroup.Captures[j].Value;
						string filterName = filter.Split(':')[0];
						string filterArg = filter.Length > filterName.Length+1 ? filter.Substring(filterName.Length+1) : null;
                        filters.Add(new TemplateFilter(filterName, 
						                               filterArg != null ? new TemplateVar(filterArg) : null)
						            );
                    }
                    return filters;
                }
            }

            return null;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, List<string>> group in _matchGroups)
            {
                sb.AppendLine(string.Format("{0}: {1} ", group.Key, string.Join(",", group.Value)));
            }
            return sb.ToString();
        }
    }

    public class TemplateFilter
    {
        public readonly string Name;
        public readonly TemplateVar Argument;

        public TemplateFilter(string name, TemplateVar argument)
        {
            Name = name;
            Argument = argument;
        }
    }
}
