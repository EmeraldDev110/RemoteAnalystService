$Logfile = "C:\Users\Administrator\Desktop\CodeDeployLog_$(gc env:computername).log"

Function LogWrite
{
   Param ([string]$logstring)

   Add-content $Logfile -value $logstring
}

LogWrite "Start"

$loadCheckTimer = 30;
$instance = Invoke-WebRequest http://169.254.169.254/latest/meta-data/instance-id -UseBasicParsing
$instanceId = $instance.Content
$query = "SELECT COUNT(*) AS CurrentLoad FROM LoadingStatusDetail AS LS INNER JOIN System_Tbl AS ST ON LS.SystemSerial = ST.SystemSerial INNER JOIN Company_Tbl AS CT ON ST.CompanyId = CT.CompanyId 
INNER JOIN LoadingInfo AS LI ON LI.UWSID = LS.TempUWSID WHERE StartProcessingTime IS NOT NULL AND InstanceID = '$instanceId'"
$region = (invoke-restmethod -uri http://169.254.169.254/latest/meta-data/placement/availability-zone)
$region = $region.Substring(0,$region.Length-1)
$MySQLAdminUserName = 'sa'
$MySQLAdminPassword = 'goneb4uc'
$MySQLDatabase = 'RemoteAnalystdb'
$MySQLHost = ''

if ($region -eq 'us-east-2') {
$MySQLHost = 'db-profile-replica-test.c8ky8ygj2nql.us-east-2.rds.amazonaws.com'
} elseif ($region -eq 'us-east-1') {
$MySQLHost = 'db-profile-replica.cxydckxzcpqt.us-east-1.rds.amazonaws.com'
} elseif ($region -eq 'us-west-2') {
$MySQLHost = 'prod-profile-v8.cbjupytjzwxn.us-west-2.rds.amazonaws.com'
}

$ConnectionString = "server=" + $MySQLHost + ";port=19500;uid=" + $MySQLAdminUserName + ";pwd=" + $MySQLAdminPassword + ";database="+$MySQLDatabase

LogWrite $instance
LogWrite $instanceId
LogWrite $query
LogWrite $region
LogWrite $MySQLHost
LogWrite $ConnectionString

Try {
[void][System.Reflection.Assembly]::LoadWithPartialName("MySql.Data")
$Connection = New-Object MySql.Data.MySqlClient.MySqlConnection
$Connection.ConnectionString = $ConnectionString
$Connection.Open()
$Command = New-Object MySql.Data.MySqlClient.MySqlCommand($Query, $Connection)
$DataAdapter = New-Object MySql.Data.MySqlClient.MySqlDataAdapter($Command)
$DataSet = New-Object System.Data.DataSet
$RecordCount = $dataAdapter.Fill($dataSet, "data")
$currLoads = $DataSet.Tables[0].CurrentLoad
$CurrLoads

LogWrite $CurrLoads
LogWrite $Connection
LogWrite $DataSet

While ($currLoads -gt 0)
{
Start-Sleep -s $loadCheckTimer
$Command = New-Object MySql.Data.MySqlClient.MySqlCommand($Query, $Connection)
$DataAdapter = New-Object MySql.Data.MySqlClient.MySqlDataAdapter($Command)
$DataSet = New-Object System.Data.DataSet
$RecordCount = $dataAdapter.Fill($dataSet, "data")
$currLoads = $DataSet.Tables[0].CurrentLoad
$CurrLoads
} 

Stop-Service -DisplayName "Remote Analyst *"
}
Catch {
LogWrite "Error"
LogWrite "ERROR : Unable to run query : $query `n$Error[0]"
}
Finally {
$Connection.Close()
}

LogWrite "Finish"