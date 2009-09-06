sn -q -k Keys.snk
attrib +r keys.snk

md .V1
csc /t:winexe /debug /out:.V1\AutoUpdateApp.exe UpdateApp.cs Updater.cs InteropBits.cs StrongName.cs

md .V2
csc /t:winexe /debug /d:V2 /out:.V2\AutoUpdateApp.exe UpdateApp.cs Updater.cs InteropBits.cs StrongName.cs

md .UPDATES
csc /res:_Cat.gif /res:_Dog.gif /res:_Monkey.gif /out:.UPDATES\Update1.dll /t:library strongname.cs
csc /res:.V2\AutoUpdateApp.exe /out:.UPDATES\Update2.dll /t:library strongname.cs

md .APP
xcopy .V1\AutoUpdateApp.exe .APP\
xcopy _Dog.gif .APP\

rename ___ AutoUpdate.csproj
rename __ AutoUpdate.sln


