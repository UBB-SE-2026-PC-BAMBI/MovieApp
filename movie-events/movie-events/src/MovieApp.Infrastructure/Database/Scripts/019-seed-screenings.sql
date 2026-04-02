USE [MovieApp];
GO

-- Seed additional events if needed
DECLARE @CreatorId INT = (SELECT TOP 1 Id FROM dbo.Users ORDER BY Id);

IF @CreatorId IS NOT NULL
BEGIN
    -- Add more events to ensure we have enough diversity
    IF NOT EXISTS (SELECT 1 FROM dbo.Events WHERE Title = 'Summer Action Blockbuster')
    BEGIN
        INSERT INTO dbo.Events (Title, Description, PosterUrl, EventDateTime, LocationReference, TicketPrice, EventType, HistoricalRating, MaxCapacity, CurrentEnrollment, CreatorUserId)
        VALUES
        ('Summer Action Blockbuster', 'The hottest action movies of the summer.', '', DATEADD(day, 3, GETDATE()), 'Multiplex Theater', 20.00, 'Special', 4.7, 150, 80, @CreatorId),
        ('Sci-Fi Extravaganza', 'Journey through space and time with the best sci-fi films.', '', DATEADD(day, 4, GETDATE()), 'IMAX Theater', 30.00, 'Special', 4.8, 100, 60, @CreatorId),
        ('Comedy Night Out', 'Laugh-out-loud comedies for a fun evening.', '', DATEADD(day, 6, GETDATE()), 'Downtown Cinema', 18.00, 'Screening', 4.6, 120, 90, @CreatorId),
        ('Drama & Art House', 'Critically acclaimed dramas and artistic films.', '', DATEADD(day, 8, GETDATE()), 'Art House Cinema', 22.00, 'Screening', 4.9, 80, 40, @CreatorId),
        ('Horror Night Special', 'Thrilling horror movies for brave souls.', '', DATEADD(day, 9, GETDATE()), 'Gothic Theater', 16.00, 'Special', 4.4, 100, 70, @CreatorId);
    END
END
GO

-- Seed Screenings for each event with multiple movies
DECLARE @EventId INT;
DECLARE @MovieId INT;
DECLARE @ScreeningTime DATETIME;

-- Get all existing events
DECLARE @Events TABLE (EventId INT, EventDateTime DATETIME);
INSERT INTO @Events
SELECT Id, EventDateTime FROM dbo.Events;

DECLARE EventCursor CURSOR FOR SELECT EventId, EventDateTime FROM @Events;

OPEN EventCursor;

FETCH NEXT FROM EventCursor INTO @EventId, @ScreeningTime;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- For each event, insert 3 or more movies
    DECLARE @MovieCount INT = 0;
    
    DECLARE MovieCursor CURSOR FOR 
        SELECT TOP 4 Id FROM dbo.Movies ORDER BY NEWID();
    
    OPEN MovieCursor;
    
    FETCH NEXT FROM MovieCursor INTO @MovieId;
    
    WHILE @@FETCH_STATUS = 0 AND @MovieCount < 4
    BEGIN
        -- Check if screening already exists
        IF NOT EXISTS (SELECT 1 FROM dbo.Screenings WHERE EventId = @EventId AND MovieId = @MovieId)
        BEGIN
            INSERT INTO dbo.Screenings (EventId, MovieId, ScreeningTime)
            VALUES (@EventId, @MovieId, @ScreeningTime);
            
            SET @MovieCount = @MovieCount + 1;
        END
        
        FETCH NEXT FROM MovieCursor INTO @MovieId;
    END
    
    CLOSE MovieCursor;
    DEALLOCATE MovieCursor;
    
    FETCH NEXT FROM EventCursor INTO @EventId, @ScreeningTime;
END

CLOSE EventCursor;
DEALLOCATE EventCursor;

GO
