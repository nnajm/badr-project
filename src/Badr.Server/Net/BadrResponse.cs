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
using Badr.Net.Http.Request;

namespace Badr.Server.Net
{
    public class BadrResponse: HttpResponse
    {
		public BadrResponse(HttpResponseStatus status, string contentType = null, string charset = null)
            : base(status,
                   contentType ?? SiteManager.Settings.DefaultContentType,
                   charset ?? SiteManager.Settings.DefaultCharset)
        {
        }

        public BadrResponse(BadrRequest request, string contentType = null, string charset = null)
            : base(request,
                   contentType ?? SiteManager.Settings.DefaultContentType,
                   charset ?? SiteManager.Settings.DefaultCharset)
        {
        }

		public static BadrResponse Create (BadrRequest request, HttpResponseStatus status)
		{
			BadrResponse response = new BadrResponse (request) { Status = status };
			
			if (status == HttpResponseStatus._404)
				response.Body = @"<html><body style=""font-size:404;font-family:lucida console"">404 Not found</body></html>" + HttpRequest.WR_SEPARATOR;
			else if (status == HttpResponseStatus._403)
				response.Body = @"<html><body style=""font-size:403;font-family:lucida console"">403 Forbidden</body></html>" + HttpRequest.WR_SEPARATOR;
			else if (status == HttpResponseStatus._408)
				response.Body = @"<html><body style=""font-size:408;font-family:lucida console"">408 Bad Request</body></html>" + HttpRequest.WR_SEPARATOR;
			
			return response;			
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
        public static BadrResponse Create(BadrRequest request, TemplateContext context, string templateOverridePath = null, string contentType = null, string charset = null)
        {
			return Create(request, 
			                      SiteManager.Views.GetTemplateEngine(request.ViewUrl, templateOverridePath), 
			                      context, 
			                      contentType, 
			                      charset);
        }

		public static BadrResponse Create(BadrRequest request, string responseBody, TemplateContext context, string contentType = null, string charset = null)
        {
            return Create(request, 
			                      SiteManager.Views.GetTemplateEngineFromText(responseBody), 
			                      context, 
			                      contentType, 
			                      charset);
        }

		private static BadrResponse Create(BadrRequest request, TemplateEngine templateEngine, TemplateContext context, string contentType = null, string charset = null)
		{
			BadrResponse response = new BadrResponse(request, contentType, charset);

			if(context == null)
				context = new TemplateContext();

            SiteManager.ContextProcessors.Process(context);
            response.Body = templateEngine.Render(request, context);
            return response;
		}

		public static BadrResponse Redirect(string url, bool permanent = false)
        {
			BadrResponse response = new BadrResponse(permanent ? HttpResponseStatus._301 : HttpResponseStatus._302);
			response.Headers.Add(HttpResponseHeaders.Location, System.Web.HttpUtility.UrlPathEncode(url));
			return response;
        }

        internal static BadrResponse CreateDebugResponse(BadrRequest request, Exception ex)
        {
            TemplateContext context = new TemplateContext();

            context[StatusPages.DEBUG_PAGE_VAR_HEADER] = string.Format("Debug: {0}", ex.Message);
            context[StatusPages.DEBUG_PAGE_VAR_EXCEPTION] = System.Web.HttpUtility.HtmlEncode(string.Format("{0}:\r\n\r\n{1}", ex.Message, ex.StackTrace));
            context[StatusPages.DEBUG_PAGE_VAR_URLS] = SiteManager.Urls.WebPrint();
            if (ex is TemplateException)
            {
                context[StatusPages.DEBUG_PAGE_VAR_TEMPLATE_RENDERERS] = (ex as TemplateException).TemplateEngine.ExprRenderers;
                context[StatusPages.DEBUG_PAGE_VAR_TEMPLATE_ERROR_LINES] = (ex as TemplateException).TemplateEngine.Errors.Select(te => te.Line);
            }

//            if (request != null)
//                context[StatusPages.DEBUG_PAGE_VAR_REQUEST] = request.RawRequest;
            BadrResponse response = new BadrResponse(request);
            response.Body = SiteManager.Views.DebugTemplateEngine.Render(request, context);

            return response;
        }
    }
}
