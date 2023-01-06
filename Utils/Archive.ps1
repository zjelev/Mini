#D:\Documents>Powershell.exe -ExecutionPolicy Bypass -Command .\ArchiveRepo.ps1 repo
param ($param1)
set-alias sz "$env:ProgramFiles\7-Zip\7z.exe"
sz a -t7z Archives\$param1\$(get-date -f yyyyMMdd_HH-mm).7z $PSScriptRoot\$param1 -mx0 -xr!bin -xr!obj -xr!Archives -xr!DBs -xr!"*.xlsx" -xr!"*.xls" -xr!"*.csv" -xr!"*.pdf" -xr!"*.txt" -xr!"*.exe" -xr!"*.zip" -xr!"*.7z" -xr!"*.ted" -xr!"*SPR73_6*.xml"