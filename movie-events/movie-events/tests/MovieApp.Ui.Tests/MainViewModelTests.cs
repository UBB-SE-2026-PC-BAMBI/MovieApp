using MovieApp.Core.Models;
using MovieApp.Ui.ViewModels;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class MainViewModelTests
{
    [Fact]
    public void CreateStartupError_WithErrorMessage_SetsDatabaseUnavailableState()
    {
        MainViewModel viewModel = MainViewModel.CreateStartupError("Database connection failed.");

        Assert.Null(viewModel.CurrentUser);
        Assert.Equal("Database unavailable", viewModel.UserLabel);
        Assert.Equal("?", viewModel.UserBadgeText);
        Assert.False(viewModel.UserFoundInDatabase);
        Assert.Equal(1, viewModel.UserDatabaseStateIndex);
        Assert.Equal("Database connection failed.", viewModel.Description);
    }

    [Fact]
    public void Constructor_ValidUser_SetsPropertiesFromUserIdentity()
    {
        User currentUser = new User
        {
            Id = 7,
            Username = "alex",
            AuthProvider = "local",
            AuthSubject = "alex-7", 
        };

        MainViewModel viewModel = new MainViewModel(currentUser);

        Assert.Same(currentUser, viewModel.CurrentUser);
        Assert.Equal("alex", viewModel.UserLabel);
        Assert.Equal("A", viewModel.UserBadgeText);
        Assert.True(viewModel.UserFoundInDatabase);
        Assert.Equal(0, viewModel.UserDatabaseStateIndex);
        Assert.Equal("local:alex-7", viewModel.Description);
    }
}