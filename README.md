# K3PO for .NET

[![Build Status][build-status-image]][build-status]
[![Issue Stats][pull-requests-image]][pull-requests]
[![Issue Stats][issues-closed-image]][issues-closed]

[build-status-image]: https://ci.appveyor.com/api/projects/status/erbxtvrxc3j59nqs/branch/develop?svg=true
[build-status]: https://ci.appveyor.com/project/jfallows/k3po-dotnet/branch/develop
[pull-requests-image]: http://www.issuestats.com/github/k3po/k3po.dotnet/badge/pr
[pull-requests]: http://www.issuestats.com/github/k3po/k3po.dotnet
[issues-closed-image]: http://www.issuestats.com/github/k3po/k3po.dotnet/badge/issue
[issues-closed]: http://www.issuestats.com/github/k3po/k3po.dotnet

- .Net K3PO Testing Framework
- NUnit Extension for K3PO Testing


### Requirement


- Visual Studio 2013. The community edition can be downloaded from [here](https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx)
- NUnit 3.0.1

### Steps to run the sample test

- Open K3PO.sln in Visual Studio
- Build the solution.
    - If the build failed because of missing `nunit.framework`, make sure Visual Studio is configured to [Allow NuGet to download missing packages](https://docs.nuget.org/consume/package-restore)
- The sample test is available in Sample\K3po.NUnit.Sample project
- You can use [NUnit Test Adapter](https://visualstudiogallery.msdn.microsoft.com/6ab922d0-21c0-4f06-ab5f-4ecd1fe7175d) to run the NUnit test
- Before running the test, make sure that the K3PO server is launched.
	- Download the latest K3PO launcher jar file from [maven central](http://search.maven.org/#search%7Cga%7C1%7Ca%3A%22k3po.launcher%22)
		- Save the jar file in K3PO-Launcher directory
	- Use the following command to launch K3PO from the Sample\K3PO-Launcher directory
		- **java -jar K3PO.jar --scriptpath ..\Scripts**
