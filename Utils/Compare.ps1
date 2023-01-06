#cmd /c Powershell.exe -ExecutionPolicy Bypass -Command D:\repo\Compare.ps1 \\srv\d$\OLD \\srv\d$\NEW
$foo = Get-ChildItem -Recurse -path $args[0]
$bar = Get-ChildItem -Recurse -path $args[1]
Compare-Object $foo $bar -Property Name, Length
#, LastWriteTime