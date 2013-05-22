Badr project
============

What is it?
---------------------

Badr is a .NET web development framework and also:

-    **lightweight**: simple configuration, small memory footprint
-    **ready to go**: integrated ORM, configured from the start for most web developpment use cases (but also customizable to meet particular needs)
-    **clean and powerful architecture**: [model, template, view] make a badr application, a set of applications make a site.
-    **DRY, reusability**: a badr application can be packaged and used with different sites.

Why?
---------------------

The need for a high level .NET web framework that helps on obtainning quick and clean results.

Features
---------------------

Many features in Badr are inspired from the django web framework:

-    Powerful Template system
-    Integrated Orm (Based on .NET dynamic objects)
-    Reusable components
-    Ships with a lightweight web server for development and testing.
-    Support for middleware classes (request/response processing)
-    Support for the FastCGI protocol

<br/>
<hr/>
Supported Platforms
===================

- Windows/.NET 4.0+
  * Solution file: Badr.sln  

- Linux/Mono 2.10.8+
  * Solution file: Badr.mono.sln

Using Badr is like using any packaged assemblies, reference it and start coding. The minimum requirements are:

>     - .NET framework 4
>     - log4net, to activate server logs (1.2.11.0, available via NuGet)

Website
=======

[badrproject.com](http://www.badrproject.com)


License
=========

[MIT Licence](https://github.com/nnajm/badrproject/blob/master/LICENCE.TXT)


Contribution
=================

[badrproject@twitter](http://twitter.com/badrproject)

<br/>
<hr/>
Documentation
===================

Let's create a simple web page that reacts to user inputs.

A general Badr solution is as folows:

<pre>
&#9673;&nbsp;<strong>Badr solution</strong>
&#9474;
&#9500;&nbsp;&#9654;&nbsp;<strong>Main project</strong>
&#9474;&nbsp;&#9474;
&#9474;&nbsp;&#9474;<em><sup>&#9472;&#9472;&nbsp;references</sup></em>
&#9474;&nbsp;&#9474;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<em><sup>. Badr.Net .Orm .Server .Apps</sup></em>
&#9474;&nbsp;&#9474;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<em><sup>. log4net.dll  (server logs)</sup></em>
&#9474;&nbsp;&#9474;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<em><sup>.  &lt;&lt; Apps project.dll &gt;&gt;</sup></em>
&#9474;&nbsp;&#9474;
&#9474;&nbsp;&#9500;&#9476;&#9476;&#9476;&nbsp;<i><b>App.config</b></i>
&#9474;&nbsp;&#9500;&#9476;&#9476;&#9476;&nbsp;<i><b>MySiteSettings.cs</b></i>
&#9474;&nbsp;&#9584;&#9476;&#9476;&#9476;&nbsp;<i><b>Program.cs</b></i>
&#9474;
&#9584;&nbsp;&#9654;&nbsp;<strong>Apps project</strong>
&nbsp;&nbsp;&#9474;
&nbsp;&nbsp;&#9474;<em><sup>&#9472;&#9472;&nbsp;references</sup></em>
&nbsp;&nbsp;&#9474;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<em><sup>. Badr.Net .Orm .Server</sup></em>
&nbsp;&nbsp;&#9474;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<em><sup>. log4net.dll  (server logs)</sup></em>
&nbsp;&nbsp;&#9474;
&nbsp;&nbsp;&#9584;&#9472;&#9472;&nbsp;<sub>[_apps_staticfiles]</sub>
&nbsp;&nbsp;&#9474;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<sub>&#9584;&#9472;&#9472;&nbsp;[app1]</sub>
&nbsp;&nbsp;&#9474;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&#9584;&#9476;&#9476;&nbsp;<i><b>home.css</b></i>
&nbsp;&nbsp;&#9584;&#9472;&#9472;&nbsp;<sub>[_apps_templates]</sub>
&nbsp;&nbsp;&#9474;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<sub>&#9584;&#9472;&#9472;&nbsp;[app1]</sub>
&nbsp;&nbsp;&#9474;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&#9584;&#9476;&#9476;&nbsp;<i><b>home.html</b></i>
&nbsp;&nbsp;&#9584;&#9472;&#9472;&nbsp;<sub>[App1]</sub>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&#9500;&#9476;&#9476;&#9476;&nbsp;<i><b>App1.cs</b></i>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&#9500;&#9476;&#9476;&#9476;&nbsp;<i><b>Urls.cs</b></i>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&#9500;&#9476;&#9476;&#9476;&nbsp;<i><b>Models.cs</b></i>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&#9584;&#9476;&#9476;&#9476;&nbsp;<i><b>Views.cs</b></i>
</pre>


### App1 creation:


This application will contain a simple web page with a form (a text input and a button). When the user sends the form, the input text will be displayed in reverse.

#### _declaring the application?_

***To tell badr it's an application, simply create a class derived from `Badr.Server.Settings.AppRoot` in the app folder***

>     AppRoot.cs 
>     ----------

>     using System;
>     using Badr.Server.Settings;

>     namespace MyApps.App1
>     {
>         public partial class App1: AppRoot
>         {
>             protected override void Set()
>             {
>                 AppName = "My App1";
>             }
>         }
>     }

>     Default AppName (if not defined in the 'Set' function) is the namespace of App1 => "MyApps.App1".
>     The name is important when working with models. To be part of the application, any model class 
>     (we'll see this later) must be under the App1Root namespace or its sub namespaces.

#### _The page url?_

***Let's choose 'your.site.domain/home/'***  
***-> Derive from `Badr.Server.Urls.SiteUrls` and set (url, view) pairs***

>     Urls.cs 
>     -------

>     using System;
>     using Badr.Server.Settings;
>     using Badr.Server.Urls;

>     namespace MyApps.App1
>     {
>         public partial class App1
>         {
>             public class Urls: SiteUrls
>             {
>                 public Urls (SiteSettings settings)
>                     :base(settings)
>                 {
>                 }

>                 protected override void Set()
>                 {
>                     Add("^home/", Views.Home, "home");
>                 }
>             }
>         }
>     }

`Add(url pattern, view function, url name)`: adds a pattern defining the url, a function to call and a name for the url.
- The regex pattern allows to handle a set of urls with a single view. "^product/(?<product_id>\d+)/$" will match 'product/1/', 'product/2/', 'product/(any number)/'  
  The pattern must not contain a leading slash (it will match what's after 'your.site.domain/')
- The view function will be called when there is a match and will have all url arguments (in the precedent example: product_id). We'll create it soon.
- The name of the url is used within template files to construct urls based on target views (reverse urls)

*N.B.: Usage of partial classes (class App1 inheriting from AppRoot and containing class Urls) is not mandatory and serves only in facilitating class naming.*

#### _The page renderer?_  

***-> A combination of a template file and a function having the signature of `Badr.Server.Views.ViewHandler`:***  
  
    BadrResponse ViewHandler(BadrRequest request, UrlArgs args)

>     Views.cs
>     --------

>     using System;
>     using System.Linq;
>     using Badr.Server.Templates;
>     using Badr.Server.Net;
>     using Badr.Net.Http.Request;
>     using Badr.Server.Urls;

>     namespace MyApps.App1
>     {
>         public static class Views
>         {
>             [Template("app1/home.html")]
>             public static BadrResponse Home (BadrRequest request, UrlArgs args)
>             {
>                 dynamic templateCtx = new TemplateContext ();

>                 if (request.Method == HttpRequestMethods.POST)
>                 {
>                     string userinput;
>                     if (request.POST.TryGet<string> ("user_input", out userinput))
>                     {
>                         templateCtx.UserInput = userinput;
>                         templateCtx.RevUserInput = new string (userinput.Reverse ().ToArray ());
>                     }
>                 }
>                 return BadrResponse.CreateResponse (request, templateCtx);
>             }
>         }
>     }
  
The template file (html file containing custom markups defined in Badr) to use is specified in the TemplateAttribute.
Badr will look for template files in SiteSettings.TEMPLATE__DIRS (we'll see this soon).  

>     home.html
>     ---------

>     <!DOCTYPE html>
>     <html>
>       <head>
>           <title>Home</title>
>           <link rel="stylesheet" href="{{ STATIC_URL }}app1/home.css" type="text/css"/>
>       </head>
>       <body>
>     	  <div>
>     	      <form action="." method="post">
>     	          {% @csrftoken %}
>     	          <input name="user_input" type="text" value="{{ UserInput }}"/>
>     	          <input type="submit" value="reverse"/>
>     	      </form>
>     	      <p>{% if RevUserInput %}{{ RevUserInput }}{% else %}please enter a word then click 'reverse'{% endif %}</p>
>     	  </div>
>       </body>
>     </html>

>     home.css (minimal styling, to demonstrate that static files are recognized)
>     --------------------------------------------------------------------------------

>     div {
>         width: 50%;
>         margin: 10% auto;
>     }

>     input[type="text"] {
>         width: 81%;
>     }

>     input[type="submit"] {
>         width: 15%;
>     }

>     input, p {
>         font-family: courier;
>         font-size: 1.3591405em;
>     }

***ok, now how will Badr render the page?***    

It's easy, it will compile the template file and renders each instruction taking into account the TemplateContext object supplied by the view. 

Meaning for home.html:

- `{% if RevUserInput %}<true_output>{% else %}<false_output>{% endif %}`:  
--> this tells Badr to render <true_output> if *RevUserInput* is not null in the TemplateContext, otherwise render <false_output>

>  - `<true_output>` here is `{{ RevUserInput }}`, which is also a special instruction that tells Badr to simply render the value of *RevUserInput*
>  (containing the reversed word, see the view function)

>  - `<false_output>` here is a literal string which will be rendered without changes.

#### ***Supported instruction tags:***    

> ***`{{ var_name }}`***    

renders a varibale named 'var_name'.

> ***`{% if lhs [operator rhs] %}<true_output>{% else %}<false_output>{% endif %}`***    

evaluates the condition "lhs operator rhs" and renders `<true_output>` if the result = true otherwise `<false_output>`. When [operator rhs] is omitted, lhs is converted to bool or compared to null.
operator can be: =, !=, <, <=, >, >=, in, not in


> ***`{% for item in list %}<inner_output>{% endfor %}`***    

loops over 'list' and rendres <inner_output> each time. Inside the loop, {{ item }} can be used and holds current looping item, along with a special variable {{ for.counter }} which holds current looping index.

> ***`{% @csrftoken %}`***    

rendres a hidden input containing a token used by Badr to prevent against Cross-Site request forgery. Any `<form method="post">` must contain a `{% @csrftoken %}` for Badr to render it.

> ***`{% include "sub/template/path" %}`***    

includes a template file (specified by "sub/template/path") and render it using the context of the container template.

> ***`{% load FilterContainer %}`***    

loads a Filter Container Class (class implementing Badr.Server.Templates.Filters.IFilterContainer) to be used in the current template.
A filter is a small transformation function that can be applied to context variable inside the template.

In Badr, there is some builtin filters: 
- 'Add': `{{ var|Add:1 }}` ==> renders var+1
- 'Odd': `{% if var|Odd %}` evaluates to true when var is and odd number, otherwise false.
- 'Even': `{% if var|Even %}` evaluates to true when var is and even number, otherwise false.

to use them, {% load BadrFilters %}.

> ***`{% url url_name arg1 arg2 ... %}`***    

renders the url named 'url_name' defined in app Urls.cs. Arguments can be passed separated by spaces. Arguments can be a context variables or a literal strings enclosed in quotes.

#### And more is in the way...

### Site project creation:

The project will contains one site using App1.

To configure a site, create a class derived from `Badr.Server.Settings.SiteSettings`

>     MySiteSettings.cs
>     -----------------

>     using System;
>     using Badr.Server.Settings;
>     using Badr.Orm;
>     using Badr.Orm.DbEngines;
>     using Badr.Server.ContextProcessors;
>     using Badr.Server.Middlewares;
>     using Badr.Apps.Static;
>     using MyApps.App1;

>     namespace Project1
>     {
>         public class MySiteSettings: SiteSettings
>         {
>             protected override void Set ()
>             {
>                 DEBUG = true;

>                 SITE_HOST_NAME = "127.0.0.1:8080";

>                 DATABASES [DbSettings.DEFAULT_DBSETTINGS_NAME] = new DbSettings
>                 {
>                   ENGINE = DbEngine.DB_SQLITE3,
>                   DB_NAME = "simple_site.db"
>                 };

>                 MIDDLEWARE_CLASSES = new[] {
>                   typeof(CsrfMiddleware),
>                   typeof(SessionMiddleware)
>                 };

>                 CONTEXT_PROCESSORS = new[]{
>                   typeof(StaticFilesContextProcessor)
>                 };

>                 STATIC_URL = "static/";
>                 STATIC_ROOT = "_apps_staticfiles/";

>                 TEMPLATE_DIRS = new[] {
>                   "_apps_templates/"
>                 };

>                 SITE_URLS = new[] {
>                   typeof(StaticFilesApp.Urls),
>                   typeof(App1.Urls)
>                 };

>                 INSTALLED_APPS = new[] {
>                   typeof(StaticFilesApp),
>                   typeof(App1)
>                 };
>             }
>         }
>     }

Ok, let's describe these settings:

- **DEBUG**              : when set to true, a debug page is rendered whenever there is an exception.

- **SITE_HOST_NAME**     : the host name of this site. Any request with this host name is redirected to this site.

- **DATABASES**          : a dictionary of `DbSettings`, a class describing a database to use with Badr.
                       For now, Badr can only use one database (the one named `DbSetting.DEFAULT_DB_NAME` if defined or the first one in the list)
                       Also, only sqlite databases are supported (coming soon all the others)

- **MIDDLEWARE_CLASSES** : an array of types implementing `Badr.Server.Middlewares.MiddlewareBase`. A middleware in Badr,
                       is an object that pre-processes the request before sending it to the view and post-processes the response before sending it to the client.
                       Example: `CsrfMiddleware`: checks for the presence of a csrf token inside a POST request (pre-process),
                       and injects it in a form for a GET request (post-process). 

- **CONTEXT_PROCESSORS** : an array of types implementing `Badr.Server.ContextProcessors.ContextProcessorBase`. A contextProcessor in Badr,
                       is an object that processes the ContextTemplate instance (injecting items for example) before passing it to the template engine.
                       Example: `StaticFilesContextProcessor`: injects an variable called "STATIC_URL" containing the value of SiteSettings.STATIC_URL.
                       (See 'home.html').

- **STATIC_URL**         : the prefix of static resources urls (without leading slash).
                       e.g.: 'static/' means any url of the form `my.site.domain/static/<anything>` will be treated as a static resource request

- **STATIC_ROOT**        : path to a directory that will contain all static files. Either absolute path or relative path to project executable.

- **TEMPLATE_DIRS**      : directories where to look for template files. Either absolute paths or relative paths to project executable.

- **SITE_URLS**          : an array of types implementing `Badr.Server.Urls.SiteUrls`. These classes will define all possible urls of the site.
                       A request url will be matching againt all url definitions in all of these classes in the order they were added 
                       to the array (and the order they were added inside a SiteUrls class) and stops at the first match. 
                       If no match is found, a 404 Not Found response is sent (or the Debug page when `DEBUG = true`)

- **INSTALLED_APPS**     : an array of types implementing `Badr.Server.Settings.AppRoot`. Each installed application will have it's models recognized by Badr.

*N.B.: for file paths, always use forward slashes (windows or linux).*

### How to launch the server?

Very easy:

>     app.config:
>     -----------

>     ...
>     <configSections>
>         <section name="badrserver" type="Badr.Server.Settings.BadrServerConfigSection, Badr.Server" />
>     ...
>     </configSections>

>     <badrserver>
>         <endpoint ipaddress="127.0.0.1" port="8080" />
>     </badrserver>


>     Program.cs
>     ----------

>     public static class Program
>     {
>         public static void Main(string[] args)
>         {
>             new BadrServer().XmlConfigure()
>                             .RegisterSite<MySiteSettings>()
>                             .Start();
>         }
>     }

XmlConfigure() tells Badr to read server configuration in App.config.  
The configuration can also be done using command line args (-s or --SERVER for ipaddress, -p or --PORT for port) by calling Configure() in place of XmlConfigure().  
It can also be done directly by calling an overload of BadrServer constructor: new BadrServer(ipaddress, port).Register<MySiteSettings>().Start();


Now you can check the result at: [`http://127.0.0.1:8080/home/`](http://127.0.0.1:8080/home/)


## Badr Orm:

suppose we defined 3 models:

        public class Member: Model
        {
            protected override void Configure(dynamic self)
            {
                base.Configure(this);

                this.ModelDbName = "member";
                this.PKField.DbName = "id";

                self.Role = new CharField() { MaxLength = 255, DbName = "role" };
                self.Rate = new DecimalField() { DbName = "rate" };
            }
        }

        public class Project: Model
        {
            protected override void Configure(dynamic self)
            {
                base.Configure(this);

                this.ModelDbName = "project";
                this.PKField.DbName = "id";

                self.Name = new CharField() { MaxLength = 255, DbName = "name" };
                self.StartDate = new DateTimeField() { DbName = "start_date" };
                self.DueDate = new DateTimeField() { DbName = "due_date" };
                self.Members = new ManyToManyField(typeof(Member), typeof(ProjectMembers));
            }
        }

        public class ProjectMembers: Model
        {
            protected override Field CreatePrimaryKey()
            {
                return null;
            }
        
            protected override void Configure(dynamic self)
            {
                base.Configure(this);

                this.ModelDbName = "project_member";

                self.Project = new ForeignKeyField(typeof(Project)) { DbName = "project_id" };
                self.Member = new ForeignKeyField(typeof(Member)) { DbName = "member_id" };
                self.JoinDate = new DateTimeField() { DbName = "join_date" };
            }
        }
    
    

- Counting Projects

        int projectsCount = Model<Project>.Manager.Count();

- Loading project

        Model project = Model<Project>.Manager.Get(1);

- Loading Project-Member relation

        Model project5Member3Rel = Model<ProjectMembers>.DManager.Filter(project_id: 5, member_id: 3).Get();

- Loading project 1 M2M Members

        List<Model> members = Model<Project>.DManager.Get(1).Members.All();

- Loading member 7 M2M Projects

        List<Model> projects = Model<Member>.DManager.Get(7).Project_set.All();

- Inserting a new member

        dynamic newMember = new Member();
        newMember.Role = "nothing";
        newMember.Rate = "101";
        newMember.Save(); // newMember.Id is updated from database (last_insert_id)

- Deleting member 11

        Model<Member>.Manager.Get(11).Delete();


- Deleting Project 7 related members

        Model<Project>.DManager.Get(7).Members.Delete();


- Loading Developers ordered by rate DESC

        List<Model> members = Model<Member>.DManager.Filter(role:"developer")
                                           .OrderBy(rate:Q.DESC)
                                           .All();


