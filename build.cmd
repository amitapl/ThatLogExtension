nuget restore
md artifacts\bin
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe ThatLogExtension\ThatLogExtension.csproj /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="..\artifacts";AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release;SolutionDir="."
nuget pack
