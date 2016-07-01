			------------------------------------------
			Importing sample Northwind data into NosDB
			------------------------------------------
=============
INTRODUCTION
=============

This module contains sample data in JSON format which is supported by NosDB. 
This readme explains how the provided sample data (%InstallationDir%\NoSDB\samples\data\json\northwind) can be imported into your configured database to get you started with NosDB features.

The provided "NorthWind.ps1" script contains all steps - creating a database, collections and importing the sample northwind data into them. However, the script file should be tweaked and executed according to your requirements as explained in this document:

=====================
DATABASE NAME OPTIONS
=====================

Provide the value for the 'DatabasesName' variable for the database to be created.

The database can either be created by the filename of the data folder (northwind) or you can provide a custom unqiue name as well.

-------------------------
Using Default Folder Name
-------------------------
If you wish to create the database with the default folder name (northwind), make sure that there is no database existing by the same name in the cluster.

---------------------
Providing Custom Name 
---------------------

If you wish to provide a custom name, comment line 13 and uncomment line 14 (mentioned below) in the script to modify it according to your requirement:
#$DatabasesName ='CustomName'; 				 

NOTE: Make sure you are connected to the database cluster at this stage. 

======================================
WHAT DOES THE NORTHWIND.PS1 SCRIPT DO? 
======================================

Creating collections and importing data into them does not require any user intervention after providing the database name. The script proceeds to perform the following operations once the changes have been made to NorthWind.ps1:

1. Create the database of the specified name in the cluster.

2. Get filenames from the respective folder in "%InstallationDir%\NoSDB\samples\data\json\northwind\" which contains the input JSON files.

3. Create collections against each filename.

4. Import the data from the files into the corresponding collections.

================================
RUNNING THE NORTHWIND.PS1 SCRIPT
================================

Once the script file has been edited according to your requirements:

1. Make sure you are connected to the cluster and within the context "NosDB:\$cluster\>".

2. Type the following command in the context:

   & "%InstallationDir%\NoSDB\samples\data\json\NorthWind.ps1"  (INCLUDING the & sign)

The data is imported into the collections which can be verified by querying or API calls. Please refer to the Programmers' Guide or Administrators' Guide in online documentation for more detail.

===================
NOSDB DOCUMENTATION
===================

The complete online documentation for NosDB is available at:
http://www.alachisoft.com/resources/docs/nosdb/help/main.html

=================
TECHNICAL SUPPORT
=================

Alachisoft © provides various sources of technical support. 

** Please refer to http://www.alachisoft.com/support.html to select a support resource you
   find suitable for your issue.

** To request additional features in the future, or if you notice any discrepancy
   regarding this document, please drop an email to support@alachisoft.com.

==========
COPYRIGHT
==========

© Copyright 2016 Alachisoft 

