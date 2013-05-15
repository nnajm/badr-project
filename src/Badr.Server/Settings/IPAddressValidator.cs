//
// IPAddressValidator.cs
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
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Badr.Server.Settings
{
    public class IPAddressValidatorAttribute : ConfigurationValidatorAttribute
    {
        public override ConfigurationValidatorBase ValidatorInstance
        {
            get
            {
                return new IPAddressValidator();
            }
        }
    }

    /// <summary>
    /// A config property validator that checks wether a value is a string and contains an ip address of the form "[[d]d]d.[[d]d]d.[[d]d]d.[[d]d]d"
    /// </summary>
    public class IPAddressValidator : ConfigurationValidatorBase
    {
        private Regex _ipregex;

        /// <summary>
        /// Initializes a new instance of IPAddressValidator
        /// </summary>
        public IPAddressValidator()
        {
            _ipregex = new Regex(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$", RegexOptions.Compiled);
        }

        public override bool CanValidate(Type type)
        {
            return type != null && type.Equals(typeof(string));
        }

        /// <summary>
        /// Checks wether the value is a string and contains an ip address of the form "[[d]d]d.[[d]d]d.[[d]d]d.[[d]d]d"
        /// </summary>
        /// <param name="value">the value to validate</param>
        /// <exception cref="SystemArgumentException"></exception>
        public override void Validate(object value)
        {
            if (value == null
                || !(value is string)
                || !_ipregex.IsMatch(value.ToString()))
            {
                throw new ArgumentException("value is not a valid ipaddress");
            }
        }
    }
}
