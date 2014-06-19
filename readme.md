## Azure Websites Log Browser - Site Extension ##

This is an implementation of a site extension for Azure Websites that knows how to read the site's different logs including: http logs and application logs from different sources - file system azure blob storage and azure table storage.

You can also use the code in this site extension as a sample for building your own site extension

Things to note:

* This is a Web API project that also contains some .cshtml (razor) files, a site extension can be any kind ofsite (plain html, node.js, php, etc).
* `WebSiteLogs.nuspec` contains the metadata for the nuget package that is later installed as the site extension, this is used to build the package later uploaded to [http://siteextensions.net](http://siteextensions.net).
* `build.cmd` contains the logic to build the project and the nuget package.
* `ThatLogExtension\applicationHost.xdt` contains the logic to transform the azure site in order to include the site extension, it mainly has the prefix url for the site extension.
