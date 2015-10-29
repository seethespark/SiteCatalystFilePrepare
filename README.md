# SiteCatalystFilePrepare
Remove the commas and quotes in Site Catalyst's exported files and then change encoding.  This allows importing into SQL Server.  
----------

Works with .NET 2 and later.

Remove commas and double quotes quotes from a file.
Change format from UTF-8.  Change from UNIX to PC style if necessary.
This allows SQL Bulk Insert to work.

Argument 1 should be the source file.

Argument 2 should be the destination file.

File names can be local to this program or can be a full Windows file path.

Any file given in argument 2 will be overwritten.

Commas and new lines in quoted text are replaced with HTML escape strings.
