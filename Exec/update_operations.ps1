$begin_date = Get-Date -Month (Get-Date).Month -Day 1 -Format "yyyyMMdd"
$end_date = Get-Date -Format "yyyyMMdd"

Set-Location c:\projects\test

$command_text = 'UpdateOperations'
$arg1 = '--Card=0'
$arg2 = '--BeginDate=' + $begin_date
$arg3 = '--EndDate=' + $end_date
$arg4 = '--OnContract=Y'
$arg5 = '--ContractID=117'

Start-Process -FilePath '.\ApiServiceEngine' -ArgumentList $command_text, $arg1, $arg2, $arg3, $arg4, $arg5 -NoNewWindow -Wait
