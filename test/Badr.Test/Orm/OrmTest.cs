//
// DbOperationsTest.cs
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
using Badr.Orm.Query;
using Badr.Test.TestApp.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Badr.Orm;

namespace Badr.Test.Orm
{
	public class OrmTest: TestBase
    {
        /// <summary>
        /// Initializing database file
        /// </summary>
        private static void ResetDatabase()
        {
			using (Stream s = typeof(OrmTest).Assembly.GetManifestResourceStream("Badr.Test.Orm.badr_orm_test.db"))
				using (FileStream fs = File.Create(WebsiteSettings.DB_FILE_PATH))
                s.CopyTo(fs);
        }

        /// <summary>
        /// Counting Projects
        /// </summary>
        [Fact(DisplayName = "Counting Projects")]
        public void ProjectsCount()
        {
            Assert.Equal(7, Model<Project>.Manager.Count());
        }

        /// <summary>
        /// Counting Members
        /// </summary>
        [Fact(DisplayName = "Counting Members")]
        public void MembersCount()
        {
            Assert.Equal(12, Model<Member>.Manager.Count());
        }

        /// <summary>
        /// Counting Projects-Members relations
        /// </summary>
        [Fact(DisplayName = "Counting Projects-Members relations")]
        public void ProjectsMembersCount()
        {
            Assert.Equal(27, Model<ProjectMembers>.Manager.Count());
        }

        /// <summary>
        /// Loading Project5-Member3 relation
        /// </summary>
        [Fact(DisplayName = "Loading Project5-Member3 relation")]
        public void LoadProject5Member3Relation()
        {
            dynamic p5m3Rel = Model<ProjectMembers>.DManager.Filter(project_id: 5, member_id: 3).Get();

            Assert.NotNull(p5m3Rel);
            Assert.Equal("project5", p5m3Rel.Project.Name);
            Assert.Equal("developer", p5m3Rel.Member.Role);
            Assert.Equal(new DateTime(2009, 03, 01), p5m3Rel.JoinDate);
        }

        /// <summary>
        /// Loading project 1
        /// </summary>
        [Fact(DisplayName = "Loading project 1")]
        public void LoadProject1()
        {
            dynamic project1 = (Project)Model<Project>.Manager.Get(1);
            
            Assert.NotNull(project1);

            Assert.Equal(1, project1.PK);
            Assert.Equal(1, project1.Id);
            Assert.Equal("project1", project1.Name);
            Assert.Equal(new DateTime(2005, 7, 1), project1.StartDate);
            Assert.Equal(new DateTime(2007, 1, 1), project1.DueDate);
        }

        /// <summary>
        /// Loading project 1 M2M Members
        /// </summary>
        [Fact(DisplayName = "Loading project 1 M2M Members")]
        public void LoadProject1_M2M_Members()
        {
            List<Model> members = Model<Project>.DManager.Get(1).Members.All();

            Assert.NotNull(members);
            Assert.Equal(3, members.Count);
            Assert.Equal(2, members[0].PK);
            Assert.Equal(3, members[1].PK);
            Assert.Equal(4, members[2].PK);
        }

        /// <summary>
        /// Loading member 7 M2M Projects
        /// </summary>
        [Fact(DisplayName = "Loading member 7 M2M Projects")]
        public void LoadMember7_M2M_Projects()
        {
            var m7projects = Model<Member>.DManager.Get(7).Project_set.All();

            Assert.NotNull(m7projects);
            Assert.Equal(2, m7projects.Count);
            Assert.Equal(3, m7projects[0].PK);
            Assert.Equal(6, m7projects[1].PK);
        }

        /// <summary>
        /// Loading Member 3
        /// </summary>
        [Fact(DisplayName = "Loading Member 3")]
        public void LoadMember3()
        {
            dynamic member3 = (Member)Model<Member>.Manager.Get(3);

            Assert.NotNull(member3);
            Assert.Equal(3, member3.PK);
            Assert.Equal(3, member3.Id);
            Assert.Equal("developer", member3.Role);
            Assert.Equal((decimal)1.3, member3.Rate);
        }

        /// <summary>
        /// Inserting a new member(Role=nothing, Rate=101)
        /// </summary>
        [Fact(DisplayName = "Inserting a new member(Role=nothing, Rate=101)")]
        public void InsertNewMember()
        {
            dynamic newMember = new Member();
            newMember.Role = "nothing";
            newMember.Rate = "101";
            newMember.Save(); // newMember.Id is updated from database (last_insert_id)

            Console.WriteLine("New member id = " + newMember.Id);

            dynamic newMemberFromDb = Model<Member>.Manager.Get(newMember.Id);
            Assert.NotNull(newMemberFromDb);
            Assert.Equal(13, newMemberFromDb.Id);
            Assert.Equal("nothing", newMemberFromDb.Role);
            Assert.Equal(101, newMemberFromDb.Rate);

            ResetDatabase();
        }

        /// <summary>
        /// Deleting member 11
        /// </summary>
        [Fact(DisplayName = "Deleting member 11")]
        public void DeleteMember11()
        {
            Model<Member>.Manager.Get(11).Delete();

            dynamic deletedMember = Model<Member>.Manager.Get(11);
            Assert.Null(deletedMember);

            ResetDatabase();
        }

        /// <summary>
        /// Deleting Project 7 related members
        /// </summary>
        [Fact(DisplayName = "Deleting Project 7 related members")]
        public void DeleteAllProject7Members()
        {
            Queryset p7Relations = Model<Project>.DManager.Get(7).ProjectMembers_set;
            Queryset p7members = Model<Project>.DManager.Get(7).Members;

            Assert.Equal(7, p7Relations.Count());
            Assert.Equal(7, p7members.Count());

            p7Relations.Clone().Delete();
            p7members.Clone().Delete();

            Assert.Equal(0, p7Relations.Clone().Count());
            Assert.Equal(0, p7members.Clone().Count());

            ResetDatabase();
        }

        /// <summary>
        /// Loading Developers ordered by rate DESC
        /// </summary>
        [Fact(DisplayName = "Loading Developers ordered by rate DESC")]
        public void LoadMembersOrderByRateDESC()
        {
            List<Model> members = Model<Member>.DManager.Filter(role:"developer")
                                                        .OrderBy(rate:Q.DESC)
                                                        .All();

            Assert.Equal(1.5m, members[0]["Rate"]);
        }
    }
}
