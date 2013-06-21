//
// StatusPages.cs
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
using System.Threading.Tasks;

namespace Badr.Server.Views
{
    public static class StatusPages
    {
        internal const string DEBUG_PAGE_VAR_HEADER = "debug_header";
        internal const string DEBUG_PAGE_VAR_EXCEPTION = "debug_body";
        internal const string DEBUG_PAGE_VAR_REQUEST = "debug_request";
        internal const string DEBUG_PAGE_VAR_URLS = "debug_urls";
        internal const string DEBUG_PAGE_VAR_TEMPLATE_RENDERERS = "debug_template_engine";
        internal const string DEBUG_PAGE_VAR_TEMPLATE_ERROR_LINES = "debug_template_errors";
        internal const string DEBUG_PAGE_TEMPLATE =
@"{% load BadrFilters %}
<!DOCTYPE html>
<html>
    <head>
        <script type=""text/javascript"">
           function toggleVisibility(button, elemId){
                var elem = document.getElementById(elemId);
                if(elem){
                    elem.style.display = elem.style.display == ""none"" ? ""block"" :  ""none"";

                    if(button != null){
                        var iconElems = button.getElementsByTagName('span');
                          if(iconElems != null && iconElems.length > 0){
                              iconElems[0].innerHTML = iconElems[0].innerHTML == ""\u25B7"" ? ""\u25BD"" :  ""\u25B7"";
                          }
                    }
                }
          }
        </script>
        <style>

            body {
                font-family: calibri, courier new, arial;
                font-size: 1em;
                margin: 0px;
                background-color: #EFFFE7;
            }

            .button {
                -webkit-touch-callout: none;
                -webkit-user-select: none;
                -khtml-user-select: none;
                -moz-user-select: none;
                -ms-user-select: none;
                user-select: none;
                cursor: pointer;
            }

            .button span {
                margin-right: 3px;
            }

            div._section_header {
                width: 98%;
                border-bottom: 1px solid #55ac00;
                font-size: 117%;
                color: #55AC00;
                padding: 3px 1%;
                height: 23px;
                margin-top: 1%;
            }

            div._debug_header {
                width: 99%;
                color: red;
                font-size: 200%;
                padding: 0.5%;
                
                overflow: hidden;
                white-space: nowrap;
                text-overflow: ellipsis;
            }

            div._page_content {
                width: 98%;
                padding: 0 1%;
            }

            pre {
                width: 98%;
                padding: 1%;
                overflow-x: auto;
            }

            pre._debug_exception {
                background-color: #DDFFCC;
                font-style: italic;
            }

            pre._debug_request {
                
            }

            table._urls_table {
                margin: 1%;
                font-family: monospace;
            }

            table._urls_table th {
                border-bottom: 1px dashed #55ac00;
            }

            table._urls_table td {
                padding: 0 17px;
            }

        </style>
    </head>
    
    <body>
        <div class=""_debug_header"">
            {{ " + DEBUG_PAGE_VAR_HEADER + @" }}
        </div>
        <div class=""_page_content"">

            {% if " + DEBUG_PAGE_VAR_EXCEPTION + @" %}
            <div class=""_section_header""><span class=""button"" onclick=""javascript:toggleVisibility(this, '_id_debug_exception');""><span>&#x25BD;</span>Stack trace:</span></div>
            <pre id=""_id_debug_exception"" class=""_debug_exception"">{{ " + DEBUG_PAGE_VAR_EXCEPTION + @" }}</pre>
            {% endif %}

            {% if " + DEBUG_PAGE_VAR_TEMPLATE_RENDERERS + @" %}
            <div class=""_section_header""><span class=""button"" onclick=""javascript:toggleVisibility(this, '_id_debug_template');""><span>&#x25BD;</span>Template structure:</span></div>
            <pre id=""_id_debug_template"">
            {% for exprRenderer in " + DEBUG_PAGE_VAR_TEMPLATE_RENDERERS + @" %}
            {{ exprRenderer.SourceTemplateLine }}.    {% if exprRenderer.SourceTemplateLine in " + DEBUG_PAGE_VAR_TEMPLATE_ERROR_LINES + @" %}<span style=""color:red;font-weight:bold;"">{{ exprRenderer.SourceTemplateMatch|PadTabsLeft:exprRenderer.Level }}</span>{% else %}{{ exprRenderer.SourceTemplateMatch|PadTabsLeft:exprRenderer.Level }}{% endif %}
            {% endfor %}
            </pre>
            {% endif %}

            {% if " + DEBUG_PAGE_VAR_URLS + @" %}
            <div class=""_section_header""><span class=""button"" onclick=""javascript:toggleVisibility(this, '_id_urls_table');""><span>&#x25B7;</span>Urls:</span></div>
            <div id=""_id_urls_table"" style=""display:none"">
            {{ " + DEBUG_PAGE_VAR_URLS + @"|Safe }}
            </div>
            {% endif %}

            {% if " + DEBUG_PAGE_VAR_REQUEST + @" %}
            <div class=""_section_header""><span class=""button"" onclick=""javascript:toggleVisibility(this, '_id_debug_request');""><span>&#x25B7;</span>Request:</span></div>
            <pre id=""_id_debug_request"" class=""_debug_request"" style=""display:none"">{{ " + DEBUG_PAGE_VAR_REQUEST + @" }}</pre>
            {% endif %}

        </div>
    </body>
</html>";
    }
}
