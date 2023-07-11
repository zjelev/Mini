#D:\Documents>Powershell.exe -ExecutionPolicy Bypass -Command .\DeleteFilesFolders.ps1 D:\Documents\57438y iu54ry
$dir=$args[0]
$deletePattern=$args[1]
Get-Childitem -path $dir -Recurse | where-object {$_.Name -ilike "$deletePattern"} | Remove-Item -Force -WhatIf