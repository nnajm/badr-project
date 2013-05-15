//
// BadrServerConfigSection.cs
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
using System.Configuration;

namespace Badr.Server.Settings
{
    public class BadrServerConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("endpoint")]
        public BadrServerEndPointElement EndPoint
        {
            get
            {
                return (BadrServerEndPointElement)this["endpoint"];
            }
            set
            { this["endpoint"] = value; }
        }
    }

    public class BadrServerEndPointElement: ConfigurationElement
    {
        [ConfigurationProperty("ipaddress", DefaultValue = "127.0.0.1", IsRequired = true)]
        [IPAddressValidatorAttribute()]
        public string IPAddress
        {
            get
            {
                return (string)this["ipaddress"];
            }
            set
            {
                this["ipaddress"] = value;
            }
        }

        [ConfigurationProperty("port", DefaultValue = "8080", IsRequired = true)]
        [IntegerValidator(MinValue=1, MaxValue=65535)]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }
    }
}
