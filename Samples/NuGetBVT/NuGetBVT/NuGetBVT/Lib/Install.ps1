$id=$args[0]
#Do a conditional update all.
if($args[1] -eq 'True')
{
Update-Package -Source https://nuget.org/api/v2/
}
Install-Package $id -Pre 
#Get the installed package
$var = @(Get-Package -Filter $id )
$var
Write-Host($var.Count)
#Check if package has been installed properly
#based on the outcome create a results file.
if ($var.Count -ge 1 )
{
$outstring = "Pass"
$filename = $id + "Pass.txt"
$outstring > $filename
}
else
{
$outstring ="Fail"
$filename = $id + "Fail.txt"
$outstring > $filename
}


