/*
Post-Deployment Script Template                     
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.      
 Use SQLCMD syntax to include a file in the post-deployment script.         
 Example:      :r .\myfile.sql                        
 Use SQLCMD syntax to reference a variable in the post-deployment script.      
 Example:      :setvar TableName MyTable                     
               SELECT * FROM [$(TableName)]               
--------------------------------------------------------------------------------------
*/

DECLARE @itemCount AS INT

BEGIN TRAN
IF (SELECT COUNT([MigrationId]) AS Id FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = '00000000000000_CreateIdentitySchema') = 0
BEGIN
   INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId],[ProductVersion]) VALUES ('00000000000000_CreateIdentitySchema','2.2.4-servicing-10062')
END
COMMIT TRAN