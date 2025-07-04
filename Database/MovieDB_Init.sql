-- Create Genres Table if not exists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Genres]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Genres] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL
    )
END
GO

-- Create Movies Table if not exists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Movies]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Movies] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [TmdbId] INT NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Overview] NVARCHAR(MAX) NOT NULL DEFAULT '',
        [ReleaseDate] DATETIME2 NOT NULL,
        [Duration] INT NOT NULL DEFAULT 0,
        [PosterPath] NVARCHAR(500) NOT NULL DEFAULT '',
        [BackdropPath] NVARCHAR(500) NOT NULL DEFAULT '',
        [VoteAverage] FLOAT NOT NULL DEFAULT 0,
        [Status] NVARCHAR(50) NOT NULL DEFAULT '',
        [GenreIds] NVARCHAR(MAX) NOT NULL DEFAULT '[]',
        [GenreNames] NVARCHAR(MAX) NOT NULL DEFAULT '[]',
        [Language] NVARCHAR(50) NOT NULL DEFAULT 'Unknown',
        [TrailerUrl] NVARCHAR(200) NOT NULL DEFAULT '',
        [Cast] NVARCHAR(MAX) NOT NULL DEFAULT '',
        [Director] NVARCHAR(100) NOT NULL DEFAULT 'Unknown',
        [IsNowShowing] BIT NOT NULL DEFAULT 0,
        [IsComingSoon] BIT NOT NULL DEFAULT 0,
        [IsFeatured] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    )
END
ELSE
BEGIN
    -- Add new columns if they don't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Movies]') AND name = 'GenreNames')
    BEGIN
        ALTER TABLE [dbo].[Movies] ADD [GenreNames] NVARCHAR(MAX) NOT NULL DEFAULT '[]'
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Movies]') AND name = 'Language')
    BEGIN
        ALTER TABLE [dbo].[Movies] ADD [Language] NVARCHAR(50) NOT NULL DEFAULT 'Unknown'
    END
END
GO

-- Add GenreId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Movies]') AND name = 'GenreId')
BEGIN
    ALTER TABLE [dbo].[Movies] ADD [GenreId] INT NULL
END
GO

-- Create foreign key constraint for GenreId if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Movies_Genres_GenreId')
BEGIN
    ALTER TABLE [dbo].[Movies] ADD CONSTRAINT [FK_Movies_Genres_GenreId] 
    FOREIGN KEY ([GenreId]) REFERENCES [dbo].[Genres] ([Id])
END
GO

-- Create index on TmdbId for faster lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Movies_TmdbId')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Movies_TmdbId] ON [dbo].[Movies]([TmdbId])
END
GO

-- Create indexes for common queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Movies_IsNowShowing')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Movies_IsNowShowing] ON [dbo].[Movies]([IsNowShowing])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Movies_IsComingSoon')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Movies_IsComingSoon] ON [dbo].[Movies]([IsComingSoon])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Movies_IsFeatured')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Movies_IsFeatured] ON [dbo].[Movies]([IsFeatured])
END
GO

-- Create index for language
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Movies_Language')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Movies_Language] ON [dbo].[Movies]([Language])
END
GO

-- Create procedure to update movie
CREATE OR ALTER PROCEDURE [dbo].[UpdateMovie]
    @TmdbId INT,
    @Title NVARCHAR(200),
    @Overview NVARCHAR(MAX),
    @ReleaseDate DATETIME2,
    @Duration INT,
    @PosterPath NVARCHAR(500),
    @BackdropPath NVARCHAR(500),
    @VoteAverage FLOAT,
    @Status NVARCHAR(50),
    @GenreIds NVARCHAR(MAX),
    @GenreNames NVARCHAR(MAX),
    @Language NVARCHAR(50),
    @TrailerUrl NVARCHAR(200),
    @Cast NVARCHAR(MAX),
    @Director NVARCHAR(100),
    @IsNowShowing BIT,
    @IsComingSoon BIT,
    @IsFeatured BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM Movies WHERE TmdbId = @TmdbId)
    BEGIN
        UPDATE Movies
        SET 
            Title = @Title,
            Overview = ISNULL(@Overview, ''),
            ReleaseDate = @ReleaseDate,
            Duration = @Duration,
            PosterPath = ISNULL(@PosterPath, ''),
            BackdropPath = ISNULL(@BackdropPath, ''),
            VoteAverage = @VoteAverage,
            Status = ISNULL(@Status, ''),
            GenreIds = ISNULL(@GenreIds, '[]'),
            GenreNames = ISNULL(@GenreNames, '[]'),
            Language = ISNULL(@Language, 'Unknown'),
            TrailerUrl = ISNULL(@TrailerUrl, ''),
            Cast = ISNULL(@Cast, ''),
            Director = ISNULL(@Director, 'Unknown'),
            IsNowShowing = @IsNowShowing,
            IsComingSoon = @IsComingSoon,
            IsFeatured = @IsFeatured,
            UpdatedAt = GETUTCDATE()
        WHERE TmdbId = @TmdbId
    END
    ELSE
    BEGIN
        INSERT INTO Movies (
            TmdbId, Title, Overview, ReleaseDate, Duration, 
            PosterPath, BackdropPath, VoteAverage, Status, GenreIds,
            GenreNames, Language, TrailerUrl, Cast, Director, 
            IsNowShowing, IsComingSoon, IsFeatured
        )
        VALUES (
            @TmdbId, @Title, ISNULL(@Overview, ''), @ReleaseDate, @Duration,
            ISNULL(@PosterPath, ''), ISNULL(@BackdropPath, ''), @VoteAverage, 
            ISNULL(@Status, ''), ISNULL(@GenreIds, '[]'),
            ISNULL(@GenreNames, '[]'), ISNULL(@Language, 'Unknown'),
            ISNULL(@TrailerUrl, ''), ISNULL(@Cast, ''), ISNULL(@Director, 'Unknown'),
            @IsNowShowing, @IsComingSoon, @IsFeatured
        )
    END
END
GO

-- Create procedure to get movies by status
CREATE OR ALTER PROCEDURE [dbo].[GetMoviesByStatus]
    @IsNowShowing BIT = NULL,
    @IsComingSoon BIT = NULL,
    @IsFeatured BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT *
    FROM Movies
    WHERE 
        (@IsNowShowing IS NULL OR IsNowShowing = @IsNowShowing)
        AND (@IsComingSoon IS NULL OR IsComingSoon = @IsComingSoon)
        AND (@IsFeatured IS NULL OR IsFeatured = @IsFeatured)
    ORDER BY 
        CASE 
            WHEN @IsNowShowing = 1 THEN ReleaseDate
            WHEN @IsComingSoon = 1 THEN ReleaseDate
            ELSE VoteAverage
        END DESC
END
GO

-- Create procedure to search movies
CREATE OR ALTER PROCEDURE [dbo].[SearchMovies]
    @SearchTerm NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT *
    FROM Movies
    WHERE 
        Title LIKE '%' + @SearchTerm + '%'
        OR Overview LIKE '%' + @SearchTerm + '%'
        OR Cast LIKE '%' + @SearchTerm + '%'
        OR Director LIKE '%' + @SearchTerm + '%'
        OR GenreNames LIKE '%' + @SearchTerm + '%'
    ORDER BY 
        CASE 
            WHEN Title LIKE @SearchTerm + '%' THEN 1
            WHEN Title LIKE '%' + @SearchTerm + '%' THEN 2
            ELSE 3
        END,
        VoteAverage DESC
END
GO

-- Create procedure to get movie details
CREATE OR ALTER PROCEDURE [dbo].[GetMovieDetails]
    @MovieId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT *
    FROM Movies
    WHERE Id = @MovieId
END
GO

-- Create procedure to get similar movies
CREATE OR ALTER PROCEDURE [dbo].[GetSimilarMovies]
    @MovieId INT,
    @Count INT = 4
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @GenreIds NVARCHAR(MAX)
    SELECT @GenreIds = GenreIds
    FROM Movies
    WHERE Id = @MovieId
    
    SELECT TOP(@Count) *
    FROM Movies
    WHERE 
        Id != @MovieId
        AND GenreIds = @GenreIds
    ORDER BY VoteAverage DESC
END
GO

-- Create procedure to get movies by language
CREATE OR ALTER PROCEDURE [dbo].[GetMoviesByLanguage]
    @Language NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT *
    FROM Movies
    WHERE Language = @Language
    ORDER BY VoteAverage DESC
END
GO

-- Create procedure to get movies by genre
CREATE OR ALTER PROCEDURE [dbo].[GetMoviesByGenre]
    @GenreName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT *
    FROM Movies
    WHERE GenreNames LIKE '%' + @GenreName + '%'
    ORDER BY VoteAverage DESC
END
GO 