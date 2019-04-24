CREATE TABLE [dbo].[Log] (
    [Id]        BIGINT        IDENTITY (1, 1) NOT NULL,
    [Date]      DATETIME      NOT NULL,
    [Thread]    VARCHAR (255) NOT NULL,
    [Level]     VARCHAR (50)  NOT NULL,
    [Logger]    VARCHAR (255) NOT NULL,
    [Message]   VARCHAR (MAX) NOT NULL,
    [Exception] VARCHAR (MAX) NULL,
    CONSTRAINT [PK_Log] PRIMARY KEY CLUSTERED ([Id] ASC)
);

