nuget restore
md artifacts\bin
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe ThatLogExtension\ThatLogExtension.csproj /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="..\artifacts";AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release;SolutionDir="."
rem copy "%ProgramW6432%\IIS\Microsoft Web Deploy V3\Microsoft.Web.Deployment.dll" artifacts\bin
rem copy "%ProgramW6432%\IIS\Microsoft Web Deploy V3\Microsoft.Web.Deployment.Tracing.dll" artifacts\bin
rem copy "%ProgramW6432%\IIS\Microsoft Web Deploy V3\Microsoft.Web.Delegation.dll" artifacts\bin
nuget pack
