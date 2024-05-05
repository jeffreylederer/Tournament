CREATE TABLE [dbo].[User] (
    [id]         INT           IDENTITY (1, 1) NOT NULL,
    [username]   VARCHAR (200) NOT NULL,
    [password]   VARCHAR (100) NOT NULL,
    [Roles]      VARCHAR (50)  NULL,
    [rowversion] ROWVERSION    NOT NULL,
    CONSTRAINT [PK_User_1] PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_User]
    ON [dbo].[User]([username] ASC);

