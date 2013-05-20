//
// FilesManager.cs
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
using Badr.Server.Net;
using Badr.Server.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Badr.Server.Views
{
	public class FilesManager
	{
		protected string[] PathRoots { get; private set; }

		public FilesManager (params string[] pathRoots)
		{
			PathRoots = pathRoots;
		}

		public string GetFileText (string filepath)
		{
			filepath = GetAbsolutePath (filepath);
			if (filepath != null)
			{
				return File.ReadAllText (filepath);
			}
			return null;
		}

		public byte[] GetFileBytes (string filepath)
		{
			filepath = GetAbsolutePath (filepath);
			if (filepath != null)
			{
				return File.ReadAllBytes (filepath);
			}
			return null;
		}

		public bool Exists (string filepath)
		{
			return GetAbsolutePath (filepath) != null;
		}

		private string GetAbsolutePath (string filepath)
		{
			if (filepath != null && !string.IsNullOrWhiteSpace (filepath))
			{
				if (Path.IsPathRooted (filepath))
					return filepath;
				else
					for (int i = 0; i < PathRoots.Length; i++)
					{
						string absFilePath = Path.Combine (PathRoots [i], filepath);
						if (File.Exists (absFilePath))
							return absFilePath;
					}
			}
			return null;
		}
	}
}
