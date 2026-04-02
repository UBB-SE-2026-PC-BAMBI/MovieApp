USE [MovieApp];
GO

IF OBJECT_ID(N'dbo.Participations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Participations
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        UserId INT NOT NULL,
        EventId INT NOT NULL,
        Status NVARCHAR(32) NOT NULL,                
        JoinedAt DATETIME2 NOT NULL 
            CONSTRAINT DF_Participations_JoinedAt DEFAULT GETUTCDATE(),

        CONSTRAINT PK_Participations PRIMARY KEY CLUSTERED (Id),
        
        CONSTRAINT UQ_Participations_User_Event UNIQUE (UserId, EventId),
        
        CONSTRAINT FK_Participations_Users FOREIGN KEY (UserId) 
            REFERENCES dbo.Users(Id),
        CONSTRAINT FK_Participations_Events FOREIGN KEY (EventId) 
            REFERENCES dbo.Events(Id) ON DELETE CASCADE
    );
END;
GO