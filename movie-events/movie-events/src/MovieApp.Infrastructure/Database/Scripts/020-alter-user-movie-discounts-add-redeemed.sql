USE [MovieApp];
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.UserMovieDiscounts')
      AND name = 'Redeemed'
)
BEGIN
    ALTER TABLE dbo.UserMovieDiscounts
        ADD Redeemed BIT NOT NULL
            CONSTRAINT DF_UserMovieDiscounts_Redeemed DEFAULT (0);
END;
GO
