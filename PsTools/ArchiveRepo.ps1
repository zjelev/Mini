# Set the source and destination folders
$ParentFolder = Split-Path -Path $PSScriptRoot -Parent
$SourceFolder = Split-Path -Path $ParentFolder -Parent
$FolderName = Split-Path -Path $SourceFolder -Leaf
$DestinationFolder = "D:\Archives\$FolderName\$(Get-Date -f yy.M.d_HH-mm)"

# Create the destination folder if it does not exist
if (-not (Test-Path $DestinationFolder)) {
New-Item -ItemType Directory -Path $DestinationFolder
}

# Get the files that have been modified in the last day
$Files = Get-ChildItem -Path $SourceFolder -Recurse | Where-Object {
$_.LastWriteTime -ge (Get-Date).AddHours(-16) -and
$_.FullName -notlike "*\bin\*" -and
$_.FullName -notlike "*\obj\*" -and
$_.FullName -notlike "*\bin" -and
$_.FullName -notlike "*\obj"-and
$_.FullName -notlike "*\.vs"
}

# Copy the files to the destination folder, preserving the folder structure
foreach ($File in $Files) {
# Get the relative path of the file
$RelativePath = $File.FullName.Substring($SourceFolder.Length + 1)

# Create the target path for the file
$TargetPath = Join-Path -Path $DestinationFolder -ChildPath $RelativePath

# Create the target directory if it does not exist
$TargetDirectory = Split-Path -Path $TargetPath -Parent
if (-not (Test-Path $TargetDirectory)) {
New-Item -ItemType Directory -Path $TargetDirectory
}

# Copy the file to the target path
Copy-Item -Path $File.FullName -Destination $TargetPath
}

# Set the alias for 7-Zip command line tool
set-alias sz "$env:ProgramFiles\7-Zip\7z.exe"

# Compress the destination folder into a 7z archive, skipping the top-level directory
Push-Location $DestinationFolder
sz a -t7z "$DestinationFolder.7z" * -mx0
Pop-Location

# Delete the destination folder
Remove-Item -Path $DestinationFolder -Recurse -Force

# Run with #PS D:\> Powershell.exe -ExecutionPolicy Bypass -Command .\ArchiveRepo.ps1
# To Debug: ExecutionPolicy -List
# Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process
# As Administrator				  
# Install-Module ps2exe
# Invoke-PS2EXE D:\repo\PsTools\ArchiveRepo.ps1 D:\repo\PsTools\ArchiveRepo.exe
#
# Win-R -> PowerShell -> Ctrl-Shift-Enter
# (Get-PackageProvider NuGet).ProviderPath | Set-Clipboard
# $PathNuget285208 = Get-Clipboard
# Remove-Item -Path $PathNuget285208 -Force
# Remove-Item -Path "C:\Program Files\PackageManagement\ProviderAssemblies\nuget\2.8.5.208\Microsoft.PackageManagement.NuGetProvider.dll" -Force