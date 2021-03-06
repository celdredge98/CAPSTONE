﻿CREATE TABLE [dbo].[EmailList]
(
   [Id]           UNIQUEIDENTIFIER  NOT NULL DEFAULT NewId(),
   [Email]        VARCHAR(256)      NOT NULL,
   [IsValidated]  BIT               NOT NULL DEFAULT 0, 
   [DateCreated]  DATETIME          NOT NULL DEFAULT GetUtcDate(), 
    CONSTRAINT [PK_dbo.EmailList] PRIMARY KEY CLUSTERED ([Id] ASC)
      WITH 
      (
         PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY],
)

GO

-- Only allow a single email address in the database.
CREATE UNIQUE NONCLUSTERED INDEX [UX_EmailList_Email] ON [dbo].[EmailList]
   (
	   [Email] ASC
   )
   WITH 
   (
      PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON
   ) ON [PRIMARY]
