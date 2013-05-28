//
// UrlArgs.cs
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
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using Badr.Server.Utils;

namespace Badr.Server.Urls
{

	public sealed class UrlArgs
	{
        private readonly Dictionary<string, string> _namedArgs;
        private readonly List<string> _positionalArgs;

        public UrlArgs()
        {
            _namedArgs = new Dictionary<string, string>();
            _positionalArgs = new List<string>();
        }

        public void Add(string name, string value)
        {
            _namedArgs[name] = value;
        }

        internal void Add(string value)
        {
            _positionalArgs.Add(value);
        }

        /// <summary>
        /// Returns the argument named 'argumentName'
        /// </summary>
        /// <param name="argumentName">argument name</param>
        /// <returns></returns>
        public string this[string argumentName]
        {
            get
            {
                if (_namedArgs.ContainsKey(argumentName))
                    return _namedArgs[argumentName];

                return null;
            }
        }

        /// <summary>
        /// Returns argument in position 'argumentPosition'
        /// </summary>
        /// <param name="argumentPosition">argument position starting from 1 (not 0)</param>
        /// <returns></returns>
        public string this[int argumentPosition]
        {
            get
            {
                if (argumentPosition > 0 && argumentPosition <= _positionalArgs.Count)
                    return _positionalArgs[argumentPosition - 1];

                return null;
            }
        }
	}

}
