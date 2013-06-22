####Badr is a full-stack web framework, offering a new approach to the .net world, inspired by django (python web framework) and designed to:

- be easy to learn
- prevent stress/unnecessary brain usage with a simple, clear and powerful architecture
- offer fine grained control over every aspect of web development
- facilitate data synchronization, deployment and scaling

##Features

-    Powerful Template system
-    Integrated Orm (Based on .NET dynamic objects)
-    Reusable components
-    Ships with a lightweight web server for development and testing.
-    Support for the FastCGI protocol

####Website/Documentation:

[www.badrproject.com](http://www.badrproject.com)

##Supported Platforms

- Windows/.NET 4.0+,  --- _Solution file: Badr.sln_
- Linux/Mono 2.10.8+ &#160;--- _Solution file: Badr.mono.sln_

####Assemblies

- **Badr.Orm** :		Badr object-relational mapper assembly (can be used as a standalone orm in any project).
- **Badr.Net**	:		low-level server: http server, fastcgi, ...
- **Badr.Server**	:		high-level server containing the template system, middlewares, url manager, context-processors, ...

####Installation

download badr, unzip it, reference its assemblies, and start coding. 
  

####Development status

Version: 0.9 Beta

####Road map:

- _**[done]**_ _Https support_
- More template tags/filters
- Model forms
- Authentication app
- Admin interface
- More databases support
- Internationalization/Localization

##License

[MIT Licence](https://github.com/nnajm/badrproject/blob/master/LICENCE.TXT)
