IF EXISTS (SELECT name FROM sys.databases WHERE name = 'MovieApp')
BEGIN
    DROP DATABASE [MovieApp];
END