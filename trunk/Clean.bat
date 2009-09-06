rd /s /q .V1
rd /s /q .V2
rd /s /q .APP
rd /s /q .UPDATES
rd /s /q obj

del /ah *.suo
del *.user
del *.resx

rename AutoUpdate.csproj ___
rename AutoUpdate.sln __