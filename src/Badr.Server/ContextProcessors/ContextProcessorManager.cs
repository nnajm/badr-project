//
// ContextProcessorManager.cs
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
using Badr.Server.Settings;
using Badr.Server.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badr.Server.ContextProcessors
{
    public class ContextProcessorManager
    {
        private List<ContextProcessorBase> _contextProcessors;
        private SiteSettings _settings;

        protected internal ContextProcessorManager(SiteSettings settings)
        {
            _settings = settings;
            _contextProcessors = new List<ContextProcessorBase>();
        }

        public void Register (Type contextProcessorType)
		{
			if (typeof(ContextProcessorBase).IsAssignableFrom (contextProcessorType))
			{
				ContextProcessorBase cp = (ContextProcessorBase)Activator.CreateInstance (contextProcessorType);
				cp.Settings = _settings;
				_contextProcessors.Add (cp);
			}
            else
                throw new ArgumentException("contextProcessorType argument is either null or not of type ContextProcessorBase", "contextProcessorType");
        }

        public void Process(TemplateContext context)
        {
            if (context != null)
                foreach (ContextProcessorBase contextProcessor in _contextProcessors)
                    contextProcessor.Process(context);
        }
    }
}
