# Prerequisites for running this script:
# 1. Docker must be installed and in path
# 2. You must have a valid account for Oracle Docker Registry
#    (https://container-registry.oracle.com)
# 3. Username / password from Oracle account must be set in environment variables DA_OR_UID / DA_OR_PWD


$imageName = "OracleDapperTest"


if([string]::IsNullOrEmpty($uid) -or [string]::IsNullOrEmpty($pwd))
{
    Write-Error "Oracle username and password must be set in environment variables DA_OR_UID / DA_OR_PWD"
    return;
}

if ((docker ps -a | where-object {$_ -match $imageName }) -eq $null) 
{ 
    Write-Host "Downloading image from oracle docker registry..."
    #Login to Oracle account and download image
    docker login -u $env:DA_OR_UID -p $env:DA_OR_PWD container-registry.oracle.com    

    docker run -d --env-file db_env.dat -p 1521:1521 -p 5500:5500 -it --name $imageName container-registry.oracle.com/database/enterprise:12.2.0.1-slim | out-null
 } else 
 {
    if ((docker ps -a | where-object {$_ -match $imageName -and $_ -match "Exited" }) -ne $null)
    {
        Write-Host "Starting docker image..."
        docker start $imageName | out-null
    }
 }

 $sleeps = 0
 while ((docker ps | where-object {$_ -match $imageName -and $_ -match "healthy" }) -eq $null)
 {
     $sleeps += 1
     Write-Host "Waiting for image to boot, attempt $sleeps"
     Start-Sleep -Seconds 10
     if($sleeps -gt 10)
     {
         Write-Host "I'm tired of waiting for docker image to boot, aborting."
         throw "Timeout waiting for image to boot"
     }
 }

 Write-Host "Docker image is alive and kickin'"


 
 