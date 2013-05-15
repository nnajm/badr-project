//
// Views.cs
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
using Badr.Server.Net;
using Badr.Orm;
using Badr.Server.Templates;
using Badr.Server.Urls;

namespace Badr.Apps.Admin
{
	public static class Views
	{
		[Template("admin/model_details.html")]
		public static BadrResponse ModelView(BadrRequest request, UrlArgs args)
		{
			string modelName = null;
			int modelId = -1;
			if (args != null
			    && (modelName = args["model_name"]) != null 
			    && int.TryParse(args["model_id"], out modelId))
            {
				dynamic model = Model.Manager(modelName).Get(modelId);

				dynamic tc = new TemplateContext();
                tc.modelName = modelName;
                tc.model = model;

                return BadrResponse.CreateResponse(request, tc);
            }

            return null;
		}

        [Template("admin/model_list.html")]
        public static BadrResponse ModelListView(BadrRequest request, UrlArgs args)
		{
			if (args != null
				&& args["model_name"] != null) 
			{
                string modelName = args["model_name"];
				string pageNum = args["page_num"];
                dynamic modelsPage = Model.Manager(modelName).Page(pageNum != null ? int.Parse(pageNum) : 1, 20);

				dynamic tc = new TemplateContext ();
				tc.modelName = modelName;
				tc.modelsPage = modelsPage;

				return BadrResponse.CreateResponse (request, tc);
			}

            return null;
		}
	}
}

