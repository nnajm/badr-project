//
// ExprMatchTree.cs
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
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Badr.Server.Templates.Rendering;

namespace Badr.Server.Templates.Parsing
{
	public class ExprMatchTree: ExprMatchGroup
	{
		public ExprMatchTree (Regex re, Match match)
			: base("_root", match.Value, match.Index, match.Length)
		{
			BuildTree (re, match);
		}

		private void BuildTree (Regex re, Match match)
		{
			string[] groups = re.GetGroupNames ();
			Dictionary<string, int> groupNumbers = new Dictionary<string, int> ();
			foreach (string groupName in groups)
				groupNumbers.Add (groupName, re.GroupNumberFromName (groupName));

			List<ExprMatchGroup> emgList = new List<ExprMatchGroup> ();
			for (int i = 0; i < groups.Length; i++)
			{
				string groupName = groups [i];
				if (groupName != "0")
				{
					Group group = match.Groups [groupName];
					if (group.Success)
					{
						for (int j = 0; j < group.Captures.Count; j++)
						{
							Capture cap = group.Captures [j];
							emgList.Add (new ExprMatchGroup (groupName, cap.Value, cap.Index, cap.Length));
						}
					}
				}
			}
			emgList.Sort (new Comparison<ExprMatchGroup> ((emg1, emg2) =>  {
				int c = emg1.StartIndex.CompareTo (emg2.StartIndex);
				if (c == 0)
					c = -1 * emg1.EndIndex.CompareTo (emg2.EndIndex);
				if (c == 0)
				{
					if (emg1.Name == BadrGrammar.GROUP_VARIABLE_VALUE)
						return 1;
					else
						if (emg2.Name == BadrGrammar.GROUP_VARIABLE_VALUE)
							return -1;
				}
				return c;
			}));
			for (int i = 0; i < emgList.Count; i++)
			{
				Add (emgList [i], true);
			}
		}
	}

	public class ExprMatchGroup
	{
		protected Dictionary<string, List<ExprMatchGroup>> SubGroups { get; set; }

		public ExprMatchGroup (string name, string value, int index, int length)
		{
			Name = name;
			Value = value;
			StartIndex = index;
			EndIndex = index + length - 1;
			SubGroups = new Dictionary<string, List<ExprMatchGroup>> ();
		}

		protected bool Add(ExprMatchGroup subGroup, bool addToParent)
		{
			if (subGroup.StartIndex >= StartIndex && subGroup.EndIndex <= EndIndex)
			{
				bool added = false;

				foreach (List<ExprMatchGroup> emgList in SubGroups.Values)
				{
					foreach (ExprMatchGroup emg in emgList)
					{
						if (subGroup.StartIndex >= emg.StartIndex && subGroup.EndIndex <= emg.EndIndex)
						{
							if (emg.Add (subGroup))
							{
								added = true;
								break;
							}
						}
					}

					if(added)
						break;
				}

				if (!added || addToParent)
				{
					if (!SubGroups.ContainsKey (subGroup.Name))
						SubGroups.Add (subGroup.Name, new List<ExprMatchGroup> ());

					SubGroups [subGroup.Name].Add (subGroup);			
				}

				return true;
			}

			return false;
		}

		public bool Add(ExprMatchGroup subGroup)
		{
			return Add (subGroup, false);
		}

		public TemplateVarFiltered GetAsFilteredVariable()
		{
			string varValue = GetGroupValue (BadrGrammar.GROUP_VARIABLE_VALUE);
			if(varValue == "field.HtmlTag")
			{

			}
			return new TemplateVarFiltered (varValue, GetFilters ());
		}

		public TemplateVarFiltered GetFilteredVariable(string variableGroupName)
		{
			if (SubGroups.ContainsKey(variableGroupName))
			{
				return SubGroups[variableGroupName][0].GetAsFilteredVariable();
			}
			
			return null;
		}

		protected List<TemplateFilter> GetFilters()
		{
			List<ExprMatchGroup> filtersGroups = GetGroupList (BadrGrammar.GROUP_VARIABLE_FILTER);
			List<ExprMatchGroup> filtersGroups2 = GetGroupList (BadrGrammar.GROUP_FILTER_NAME);

			if(filtersGroups2 != null)
				if (filtersGroups != null)
					filtersGroups.AddRange (filtersGroups2);
				else
					filtersGroups = GetGroupList (BadrGrammar.GROUP_FILTER_NAME);

			if (filtersGroups == null)
				return null;

			List<TemplateFilter> filters = new List<TemplateFilter> ();
			bool escapeFilterFound = false;
					
			for (int j = 0; j < filtersGroups.Count; j++)
			{
				string filterName = filtersGroups[j].GetGroupValue(BadrGrammar.GROUP_FILTER_NAME);
				bool noArgs = filterName == null;
				if(noArgs)
					filterName = filtersGroups[j].Value;
						
				if (filterName == "Escape")
					escapeFilterFound = true;
				else
				{
					string filterArg = noArgs ? null : filtersGroups [j].GetGroupValue (BadrGrammar.GROUP_FILTER_ARG);
					filters.Add (new TemplateFilter (filterName, filterArg));
				}
			}

			if (escapeFilterFound)
				filters.Add (new TemplateFilter ("Escape", null));
					
			return filters;
		}

		public string GetGroupValue (string groupName)
		{
			if (SubGroups.ContainsKey (groupName))
				return SubGroups [groupName] [0].Value;
			return null;
		}

		public List<string> GetGroupValuesList (string groupName)
		{
			if (SubGroups.ContainsKey (groupName))
			{
				List<string> values = new List<string>();
				foreach(ExprMatchGroup emg in SubGroups[groupName])
					values.Add(emg.Value);
				return values;
			}
			return null;
		}

		public ExprMatchGroup GetGroup (string groupName)
		{
			if (SubGroups.ContainsKey (groupName))
				return SubGroups [groupName][0];
			return null;
		}

		public List<ExprMatchGroup> GetGroupList (string groupName)
		{
			if (SubGroups.ContainsKey (groupName))
				return SubGroups [groupName];
			return null;
		}

		public List<TemplateVarFiltered> GetFilteredVariableList(string variableGroupName)
		{
			if (SubGroups.ContainsKey (variableGroupName))
			{
				List<TemplateVarFiltered> tVars = new List<TemplateVarFiltered>();
				foreach (ExprMatchGroup variableGroup in GetGroupList(variableGroupName))
					tVars.Add(variableGroup.GetAsFilteredVariable());
				return tVars;
			}
			
			return null;
		}
		
		public Dictionary<string, TemplateVarFiltered> GetFilteredAssignmentList(string assignmentGroupName)
		{
			Dictionary<string, TemplateVarFiltered> assignmentMatches = new Dictionary<string, TemplateVarFiltered>();
			if (SubGroups.ContainsKey(assignmentGroupName))
			{
				foreach (ExprMatchGroup assignmentGroup in GetGroupList(assignmentGroupName))
				{
					string varName = assignmentGroup.GetGroupValue(BadrGrammar.GROUP_VARIABLE_NAME);
					ExprMatchGroup assignementValueGroup = assignmentGroup.GetGroup(BadrGrammar.GROUP_ASSIGNMENT_VALUE);
					string varValue = assignementValueGroup.GetGroupValue(BadrGrammar.GROUP_VARIABLE_VALUE);
					assignmentMatches.Add(varName, new TemplateVarFiltered(varValue, assignementValueGroup.GetFilters()));
				}
			}
			
			return assignmentMatches;
		}

		public string Name { get; set;}
		public string Value { get; set;}
		public int StartIndex { get; set;}
		public int EndIndex { get; set;}

		public override string ToString ()
		{
			return string.Format ("[ExprMatchGroup: Name={0}, Value={1}, StartIndex={2}, EndIndex={3}]", Name, Value, StartIndex, EndIndex);
		}
	}
}

