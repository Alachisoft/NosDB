#------------------------------Create Northwind Databse Script-----------------------------------
#------------------------------------------------------------------------------------------------
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition;
$FolderName = Get-ChildItem -Path $scriptPath |?{ $_.PSIsContainer };
$databseFolderName = $FolderName.Name;
$Format = 'csv';
$ClusterContext =  $(get-location).Path;
$DatabaseContext = ($ClusterContext) + '\databases';
#$DatabasesName = $databseFolderName;#use folder name as database name or custom name
$DatabasesName ='Custom Name';#use folder name as database name or custom name
cd -Path $DatabaseContext

#------------------------------------------------------------------------------------------------
#-----------------Script for creating database, must bhe connected to database cluster ----------
$query = ("CREATE DATABASE $"  + $DatabasesName + "$ {}");
#Write-Host $query;
Invoke-SQL -Query $query
$junk = dir;

#------------------------------------------------------------------------------------------------
cd ./$DatabasesName ; 
$FilesName = @(Get-ChildItem -Path $scriptPath/$databseFolderName -Filter *.$Format |% {$_.BaseName});

#------------------------------------------------------------------------------------------------
#-----------------loop for creating collections, Database must exist before this loop------------
foreach($collection in $FilesName)
{
    Write-Output "CREATING COLLECTION: $collection ...";
    $query2 = ('CREATE COLLECTION $' + $collection + '$ {"Database":"' + $DatabasesName + '"}');
    Write-Host $query2;
    Invoke-SQL -Query $query2
}
    
#------------------------------------------------------------------------------------------------
cd $DatabaseContext\$DatabasesName'\collections\';
$junk = dir;

#------------------------------------------------------------------------------------------------
#---------- loop for importing data in collections, collections must exist before this loop------
foreach($collection in $FilesName)
{
    
    
    cd $DatabaseContext\$DatabasesName'\collections\'$collection;
    $fullPath = "$scriptPath\$databseFolderName\$collection.$Format";
    Write-Host $(get-location).Path;	
    Write-Host "importing collection"  $collection;
    Import-Data -Format $Format -Path $fullPath; 
}
    

#------------------------------------------------------------------------------------------------
