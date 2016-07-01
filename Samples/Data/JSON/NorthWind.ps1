[CmdletBinding(SupportsShouldProcess=$true)]
Param()
#----------------------------------------------------------------------------------------------------
$paths =  $(Get-Location).Path.Split('\')
$condition  = ($paths[1] -ne '');
$conditionConnected = $paths[0] -eq 'NosDB:'

Write-Host "";
if($condition -and $conditionConnected)
{
	$clusterName = $paths[1] ;
	cd \;
	cd $clusterName;
	cd 'databases';
}
else
{
	Write-Host "Please connect with a database cluster to execute this script."
}

if($(Get-Location).Path.EndsWith('databases'))
{
    #----------------------------- Create Northwind Database Script ---------------------------------
    $scriptPath = split-path -parent $MyInvocation.MyCommand.Definition;
    $FolderName = Get-ChildItem -Path $scriptPath |?{ $_.PSIsContainer };
    $databseFolderName = $FolderName.Name;
    $Format = 'JSON';
    #------------------------------------------------------------------------------------------------
    $DatabaseContext = $(get-location).Path;
    $DatabasesName = $databseFolderName;#use folder name as database name or custom name
	
    cd -Path $DatabaseContext
    #---------------- Script for creating database, must bhe connected to database cluster ----------
    Write-Host "Creating database '$DatabasesName' ..."; 
    $query = ('CREATE DATABASE "' + $DatabasesName + '" ');
	Write-Verbose "Query >> $query";
	
    Invoke-SQL -Query $query
    Start-Sleep -Seconds 3;
    $junk = dir;

	Write-Host "";
    #------------------------------------------------------------------------------------------------
    cd ./$DatabasesName ; 
    $FilesName = @(Get-ChildItem -Path $scriptPath/$databseFolderName -Filter *.$Format |% {$_.BaseName});

    #---------------- Loop for creating collections, Database must exist before this loop -----------
    foreach($collection in $FilesName)
    {
        Write-Output "CREATING COLLECTION: $collection ...";
        $query2 = ('CREATE COLLECTION "' + $collection + '" {"Database":"' + $DatabasesName + '"}');
        Write-Verbose "Query >> $query2";
		
        Invoke-SQL -Query $query2		
    }

	Write-Host "";
    #------------------------------------------------------------------------------------------------
    cd $DatabaseContext\$DatabasesName'\collections\';
    $junk = dir;

    #---------- Loop for importing data in collections, collections must exist before this loop -----
    foreach($collection in $FilesName)
    {
        cd $DatabaseContext\$DatabasesName'\collections\'$collection;
        $fullPath = "$scriptPath\$databseFolderName\$collection.$Format";
	    Write-Host "Importing data into collection '$collection' ...";        
		Write-Verbose $(get-location).Path;
		
        Import-Data -Format $Format -Path $fullPath; 
    }
    foreach($chunk in $paths)
    {
        if($chunk -eq 'NosDB:')
        {
            cd \;
        }
        else
        {
            cd $chunk;
        }
    }
#----------------------------------------------------------------------------------------------------
}
else
{
    Write-Host "Change context to NosDb:\Cluster\Database> to execute the script"
}
Write-Host "";