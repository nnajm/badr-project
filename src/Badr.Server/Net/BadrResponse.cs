//
// BadrResponse.cs
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
using System.IO.Compression;
using System.IO;
using System.Net.Sockets;
using Badr.Net.Http.Response;
using Badr.Server.Templates;
using Badr.Server.Urls;
using Badr.Server.Views;

namespace Badr.Server.Net
{
    public class BadrResponse: HttpResponse
    {
        public BadrResponse(BadrRequest wRequest, string contentType = null, string charset = null)
            : base(wRequest,
                   contentType ?? wRequest.SiteManager.SiteSettings.DEFAULT_CONTENT_TYPE,
                   charset ?? wRequest.SiteManager.SiteSettings.DEFAULT_CHARSET)
        {
        }

        /// <summary>
        /// Creates a Response to send back to client
        /// </summary>
        /// <param name="request">The original client request</param>
        /// <param name="context">The context object to pass to template engine</param>
        /// <param name="templateOverridePath">The template file path (overrides the supplied path in the TemplateAttribute over the view function)</param>
        /// <param name="contentType">Response content type (mime-type)</param>
        /// <param name="charset">Response charset (encoding)</param>
        /// <returns></returns>
        public static BadrResponse CreateResponse(BadrRequest request, TemplateContext context, string templateOverridePath = null, string contentType = null, string charset = null)
        {
            TemplateEngine templateEngine = request.SiteManager.ViewManager.GetTemplateEngine(request.ViewUrl, templateOverridePath);
            BadrResponse response = new BadrResponse(request, contentType, charset);

			if(context == null)
				context = new TemplateContext();

            request.SiteManager.ContextProcessorManager.Process(context);
            response.Body = templateEngine.Render(request, context);
            return response;
        }

        internal static BadrResponse CreateDebugResponse(BadrRequest request, Exception ex)
        {
            TemplateContext context = new TemplateContext();

            context[StatusPages.DEBUG_PAGE_VAR_HEADER] = string.Format("Debug: {0}", ex.Message);
            context[StatusPages.DEBUG_PAGE_VAR_EXCEPTION] = System.Web.HttpUtility.HtmlEncode(string.Format("{0}:\r\n\r\n{1}", ex.Message, ex.StackTrace));
            context[StatusPages.DEBUG_PAGE_VAR_URLS] = request.SiteManager.UrlsManager.WebPrint();
            if (ex is TemplateException)
            {
                context[StatusPages.DEBUG_PAGE_VAR_TEMPLATE_RENDERERS] = (ex as TemplateException).TemplateEngine.ExprRenderers;
                context[StatusPages.DEBUG_PAGE_VAR_TEMPLATE_ERROR_LINES] = (ex as TemplateException).TemplateEngine.Errors.Select(te => te.Line);
            }

//            if (request != null)
//                context[StatusPages.DEBUG_PAGE_VAR_REQUEST] = request.RawRequest;
            BadrResponse response = new BadrResponse(request);
            response.Body = request.SiteManager.ViewManager.DebugTemplateEngine.Render(request, context);

            return response;
        }
    }
}
