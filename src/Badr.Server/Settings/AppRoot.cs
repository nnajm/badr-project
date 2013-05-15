//
// AppRoot.cs
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
using System.Linq;
using System.Text;
using Badr.Server.Urls;
using System.IO;
using System.Reflection;

namespace Badr.Server.Settings
{
    public abstract class AppRoot
    {
        private static char[] _invalidAppNameChars = Path.GetInvalidFileNameChars();

        /// <summary>
        /// Application name, which is the namespace of the AppRoot class & SiteUrls class
        /// </summary>
        public string AppName { get; protected set; }
        internal string AppNamespace { get; private set; }

        public AppRoot()
        {
            AppNamespace = this.GetType().Namespace;

            Set();

            if (string.IsNullOrWhiteSpace(AppName))
                AppName = AppNamespace;
            else
            {
                AppName = AppName.Trim().Replace(' ', '_');

                foreach (char c in _invalidAppNameChars)
                    if (AppName.Contains(c))
                        throw new Exception(string.Format("AppName must contain only valid file name chars, found illegal '{0}'.", c));
            }            
        }

        protected abstract void Set();
    }
}
