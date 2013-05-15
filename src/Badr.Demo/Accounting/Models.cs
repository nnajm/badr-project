//
// Models.cs
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
using Badr.Orm;
using Badr.Orm.Fields;

namespace Badr.Demo.Accounting
{
    [Manager(typeof(Manager))]
    public class AccountType : Model
    {
        protected override void Configure(dynamic self)
        {
            base.Configure(this);

            self.Description = new CharField() { MaxLength = 500 };
        }

        public string Description { get; set; }

        public override string Unicode(dynamic self)
        {
            return self.Description;
        }

        //class Meta:
        //    verbose_name_plural = "Account types"
    }

    public class AccountClass : Model
    {
        protected override void Configure(dynamic self)
        {
            base.Configure(this);

            self.AccountType = new ForeignKeyField(typeof(AccountType));
            self.Name = new CharField() { MaxLength = 100 };
            self.Description = new CharField() { MaxLength = 100 };
        }

        public override string Unicode(dynamic self)
        {
            return string.Format("[{0}] {1}", self.Id, self.Description);
        }

        //    class Meta:
        //        verbose_name_plural = "Account classes"
    }

    public class Account : Model
    {
        protected override void Configure(dynamic self)
        {
            base.Configure(this);

            self.AccountNumber = new IntegerField();
            self.AccountClass = new ForeignKeyField(typeof(AccountClass));
            self.ParentAccount = new ForeignKeyField(typeof(Account)) { Null = true };
            self.Description = new CharField() { MaxLength = 300 };
        }

        public override string Unicode(dynamic self)
        {
            return string.Format("{0}. {1}", self.AccountNumber, self.Description);
        }

        //    class Meta:
        //        verbose_name_plural = "Accounts"
    }

    public class Currency : Model
    {
        protected override void Configure(dynamic self)
        {
            base.Configure(this);

            self.Name = new CharField() { MaxLength = 3 };
            self.ToEur = new IntegerField();
        }

        public override string Unicode(dynamic self)
        {
            return self.Name;
        }

        //    class Meta:
        //        verbose_name_plural = "Currencies"

    }
}
