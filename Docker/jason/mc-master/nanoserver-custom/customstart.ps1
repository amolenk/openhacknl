$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$worldtolink = $env:worldtolink
$storagename = $env:storagename
$passcode = $env:passcode

net use F: $worldtolink  /u:AZURE\$storagename  $passcode

cmd /c mklink /d "c:\data" "$worldtolink"

Start-Job -ScriptBlock {
  while((Select-String -Pattern 'RCON running' -Path C:\minecraft\minecraft.out) -eq $null) { Write-Output "nothing"; Start-Sleep -Seconds 1 }
  rcon-cli --host 127.0.0.1 --port 25575 --password cheesesteakjimmys ban b973ece7-93e7-477e-a69a-d22554953e89
} | Out-Null

powershell -File C:\minecraft\start.ps1 | Tee-Object C:\minecraft\minecraft.out