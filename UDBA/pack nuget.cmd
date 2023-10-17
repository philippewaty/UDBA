if not exist NuGet mkdir NuGet

del /Q NuGet\*.*

NuGet.exe pack UDBA.nuspec -OutputDirectory NuGet -IncludeReferencedProjects -Properties Configuration=Release;Platform=AnyCPU -Build

rem pause