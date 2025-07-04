-- Add GenreId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Movies]') AND name = 'GenreId')
BEGIN
    ALTER TABLE [dbo].[Movies] ADD [GenreId] INT NULL
END
GO

-- Create Genres Table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Genres]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Genres] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL
    )
END
GO

-- Create foreign key constraint for GenreId if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Movies_Genres_GenreId')
BEGIN
    ALTER TABLE [dbo].[Movies] ADD CONSTRAINT [FK_Movies_Genres_GenreId] 
    FOREIGN KEY ([GenreId]) REFERENCES [dbo].[Genres] ([Id])
END
GO 