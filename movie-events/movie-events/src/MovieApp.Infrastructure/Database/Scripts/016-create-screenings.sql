USE [MovieApp];
GO

IF OBJECT_ID(N'dbo.Screenings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Screenings
    (
        Id INT IDENTITY(1,1) NOT NULL,
        EventId INT NOT NULL,
        MovieId INT NOT NULL,
        ScreeningTime DATETIME NOT NULL,

        CONSTRAINT PK_Screenings PRIMARY KEY (Id),
        CONSTRAINT UQ_Screenings_EventMovie UNIQUE (EventId, MovieId),
        CONSTRAINT FK_Screenings_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(Id),
        CONSTRAINT FK_Screenings_Movies FOREIGN KEY (MovieId) REFERENCES dbo.Movies(Id)
    );
END;
GO
