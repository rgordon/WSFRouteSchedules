WSFRouteSchedules
===============

The WSFRouteSchedules code is an example of how to query the Washington State Ferries schedule web service. The returned results are dumped to a compressed JSON file. The program is currently used to package the ferry schedule data which is used in our Android and iPhone mobile apps. We are publishing it in hopes that others might find it useful and be able to learn from it.

The code is not meant to illustrate best practices nor is it necessarily bug free. It is by no means static and is updated and improved upon as time allows or new features are needed.

###Development

The development environment was on a Mac mini running Snow Leopard, using <a href="http://monodevelop.com/">MonoDevelop</a> and <a href="http://www.mono-project.com/">Mono</a> / .NET 3.5. <a href="http://www.mono-project.com/">Mono</a> is an open source, cross-platform, implementation of C# and the CLR that is binary compatible with Microsoft.NET

###Dependencies

The only dependencies the project relies upon is <a href="http://james.newtonking.com/projects/json-net.aspx">James Newton-King's Json.NET library</a>.

>The Json.NET library makes working with JavaScript and JSON formatted data in .NET simple. Quickly read and write JSON using the JsonReader and JsonWriter or serialize your .NET objects with a single method call using the JsonSerializer. 

Once you download the latest version simply add a reference to the dll from within your project. We used the .NET 2.0 version of the library so we could run the executable on a Linux server under Mono, however, the other versions appeared to work fine on my Mac as well.

###Traveler Information API

The <a href="http://www.wsdot.wa.gov/traffic/api/">Traveler Information Application Programming Interface</a> is designed to provide third parties with a single gateway to all of WSDOT's traveler information data. To use the WSDL services you must be assigned an Access Code. You can obtain an Access Code by simply providing your email address at the <a href="http://www.wsdot.wa.gov/traffic/api/">Traveler Information API</a> site.

###Contributing

Find a bug? Got an idea? Send us a patch or contact us and we will take a look at it.

