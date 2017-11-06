Commands
------------
nuget setApiKey xxx-xxx-xxxx-xxxx

nuget push .\packages\Happer.1.0.0.0.nupkg

nuget pack ..\Happer\Happer.Hosting\Happer.Hosting.csproj -IncludeReferencedProjects -Symbols -Build -Prop Configuration=Release -OutputDirectory ".\packages"
nuget pack ..\Happer\Happer.Metrics\Happer.Metrics.csproj -IncludeReferencedProjects -Symbols -Build -Prop Configuration=Release -OutputDirectory ".\packages"
