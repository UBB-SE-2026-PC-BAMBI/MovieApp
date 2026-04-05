### To Make The Project Runnable

# 1
- Open MovieApp.sln from Solution Manager
- Properties -> Configure Startup Projects -> Single Startup Project -> MovieApp.Ui

# 2
- Toolbar (Top) -> Solution Platforms (Dropdown where you can find "Any CPU"/"x64"/...) -> x64

# 3
- Toolbar (Top) -> In-between Solution Platforms and Start Without Debugging -> Dropdown -> MovieApp.Ui (Unpackaged)

### Database

- Solution Manager -> MovieApp.sln
- (if not visible) Right-Click -> Add -> Existing Project -> DbSetup/DbSetup.csproj
- Solution Explorer -> Right-Click on DbSetup -> Set as startup project -> Run the project
- Solution Explorer -> Right-Click on MovieApp.Ui -> Set as startup project

# After this the app should be runnable
