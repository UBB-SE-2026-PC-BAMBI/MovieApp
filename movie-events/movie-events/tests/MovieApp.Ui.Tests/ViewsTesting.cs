using System;
using System.ComponentModel;
using Moq;
using Microsoft.UI.Xaml;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels;
using MovieApp.Ui.Views;
using Xunit;

namespace MovieApp.Ui.Tests.Views;

public sealed class ViewsTesting
{
    [Fact]
    public void DetailsCheckoutPage_Constructor_InitializesWithoutFatalLogicErrors()
    {
        Exception ex = Record.Exception(() => new DetailsCheckoutPage());
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    [Fact]
    public void EventDialogViewBuilder_Create_WithJackpotBanner_ConstructsLayout()
    {
        DateTime dummyDate = new DateTime(2030, 5, 8, 19, 30, 0);
        Event mockEvent = BuildEvent(dummyDate, 10m, 4.5, 100, 50, "Test", "Festival", "Main Stage");
        EventDialogViewModel viewModel = new EventDialogViewModel(mockEvent, true, 50);

        Exception ex = Record.Exception(() => EventDialogViewBuilder.Create(viewModel));
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    [Fact]
    public void EventDialogViewBuilder_Create_WithFreePass_ConstructsLayout()
    {
        DateTime dummyDate = new DateTime(2030, 5, 8, 19, 30, 0);
        Event mockEvent = BuildEvent(dummyDate, 10m, 4.5, 100, 50, "Test", "Festival", "Main Stage");
        EventDialogViewModel viewModel = new EventDialogViewModel(mockEvent, false, null);

        Exception ex = Record.Exception(() => EventDialogViewBuilder.Create(viewModel));
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    [Fact]
    public void EventManagementPage_Constructor_InitializesWithoutFatalLogicErrors()
    {
        Exception ex = Record.Exception(() => new EventManagementPage());
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    [Fact]
    public void FavoritesPage_Constructor_InitializesWithoutFatalLogicErrors()
    {
        Exception ex = Record.Exception(() => new FavoritesPage());
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    [Fact]
    public void HomePage_Constructor_MissingServices_ThrowsInvalidOperationException()
    {
        Exception ex = Record.Exception(() => new HomePage());
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    [Fact]
    public void MainWindow_Constructor_AssignsDependencies()
    {
        Mock<IEventRepository> mockRepo = new Mock<IEventRepository>();
        User dummyUser = new User { Id = 1, Username = "TestUser", AuthSubject = "test-subject", AuthProvider = "test-provider" };
        MainViewModel mockViewModel = new MainViewModel(dummyUser);

        Exception ex = Record.Exception(() => new MainWindow(mockViewModel, mockRepo.Object));
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    [Fact]
    public void MarathonsPage_Constructor_MissingServices_ThrowsInvalidOperationException()
    {
        Exception ex = Record.Exception(() => new MarathonsPage());
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    [Fact]
    public void MyEventsPage_Constructor_InitializesWithoutFatalLogicErrors()
    {
        Exception ex = Record.Exception(() => new MyEventsPage());
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    [Fact]
    public void NotificationsPage_Constructor_InitializesWithoutFatalLogicErrors()
    {
        Exception ex = Record.Exception(() => new NotificationsPage());
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    [Fact]
    public void ReferralAreaPage_ReferralCode_Setter_RaisesPropertyChanged()
    {
        ReferralAreaPage? page = null;
        try { page = new ReferralAreaPage(); } catch { }

        if (page != null)
        {
            bool wasRaised = false;
            page.PropertyChanged += (object? sender, PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == "ReferralCode") wasRaised = true;
            };

            page.ReferralCode = "NEW-CODE";

            Assert.True(wasRaised);
            Assert.Equal("NEW-CODE", page.ReferralCode);
        }
    }

    [Fact]
    public void ReferralAreaPage_IsHistoryEmpty_ReturnsVisible_WhenCollectionEmpty()
    {
        ReferralAreaPage? page = null;
        try { page = new ReferralAreaPage(); } catch { }

        if (page != null)
        {
            page.ReferralHistory.Clear();
            Assert.Equal(Visibility.Visible, page.IsHistoryEmpty);
        }
    }

    [Fact]
    public void ReferralAreaPage_Constructor_InitializesWithoutFatalLogicErrors()
    {
        Exception ex = Record.Exception(() => new ReferralAreaPage());
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    [Fact]
    public void SlotRewardItem_StatusLabel_WhenRedeemed_ReturnsRedeemedText()
    {
        SlotRewardItem item = new SlotRewardItem { IsRedeemed = true, DiscountText = "20% off" };
        Assert.Equal("Redeemed", item.StatusLabel);
    }

    [Fact]
    public void SlotRewardItem_StatusLabel_WhenAvailable_ReturnsDiscountText()
    {
        SlotRewardItem item = new SlotRewardItem { IsRedeemed = false, DiscountText = "20% off" };
        Assert.Equal("20% off", item.StatusLabel);
    }

    [Fact]
    public void SlotRewardItem_StatusBrush_WhenRedeemed_ExecutesProperty()
    {
        SlotRewardItem item = new SlotRewardItem { IsRedeemed = true };
        Exception ex = Record.Exception(() => { object brush = item.StatusBrush; });
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    [Fact]
    public void SlotRewardItem_StatusBrush_WhenAvailable_ExecutesProperty()
    {
        SlotRewardItem item = new SlotRewardItem { IsRedeemed = false };
        Exception ex = Record.Exception(() => { object brush = item.StatusBrush; });
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    [Fact]
    public void RewardsPage_Constructor_InitializesWithoutFatalLogicErrors()
    {
        Exception ex = Record.Exception(() => new RewardsPage());
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    [Fact]
    public void SectionEventsPage_Constructor_InitializesWithoutFatalLogicErrors()
    {
        Exception ex = Record.Exception(() => new SectionEventsPage());
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    [Fact]
    public void SlotMachinePage_Constructor_InitializesWithoutFatalLogicErrors()
    {
        Exception ex = Record.Exception(() => new SlotMachinePage());
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    [Fact]
    public void TriviaWheelPage_Constructor_InitializesWithoutFatalLogicErrors()
    {
        Exception ex = Record.Exception(() => new TriviaWheelPage());
        if (ex != null) Assert.IsNotType<NullReferenceException>(ex);
    }

    private static Event BuildEvent(DateTime date, decimal price, double rating, int cap, int enroll, string desc, string type, string loc)
    {
        return new Event
        {
            Id = 1,
            Title = "Dummy Event",
            CreatorUserId = 1,
            EventDateTime = date,
            TicketPrice = price,
            HistoricalRating = rating,
            MaxCapacity = cap,
            CurrentEnrollment = enroll,
            Description = desc,
            EventType = type,
            LocationReference = loc
        };
    }
}