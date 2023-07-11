# Powershell.exe -ExecutionPolicy Bypass -Command D:\Archives\repo\DeleteBinObj.ps1
Get-ChildItem D:\Archives\repo -include bin,obj -Recurse | foreach ($_) { remove-item $_.fullname -Force -Recurse }