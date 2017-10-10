$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$share = $env:share
$storagename = $env:storagename
$passcode = $env:passcode
$instanceid = $env:Fabric_ServicePackageInstanceSeqNum

net use F: $share /u:AZURE\$storagename $passcode
if (!(Test-Path "F:\$instanceid")) {  
	New-Item "F:\$instanceid" -type Directory
}

cmd /c mklink /d "c:\data" "f:\$instanceid"

Start-Job -ScriptBlock {
  while((Select-String -Pattern 'RCON running' -Path C:\minecraft\minecraft.out) -eq $null) { Write-Output "nothing"; Start-Sleep -Seconds 1 }
} | Out-Null

powershell -File C:\minecraft\start.ps1 | Tee-Object C:\minecraft\minecraft.out
