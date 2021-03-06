$begin_date = Get-Date -Month (Get-Date).Month -Day 1 -Format "yyyyMMdd"
$end_date = Get-Date -Format "yyyyMMdd"

Set-Location c:\projects\test

$task = '--task UpdateOperations'
$param = '--param'
$arg = 'Card=0,BeginDate=' + $begin_date + ',EndDate=' + $end_date + ',OnContract=Y,ContractID=117'
$database = '--database home'

Start-Process -FilePath '.\ApiServiceEngine' -ArgumentList $task, $param, $arg, $database -NoNewWindow -Wait
