using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MovieApp.Core.Models;
using MovieApp.Ui.Services;
using Windows.UI;
using System;
using System.Threading.Tasks;

namespace MovieApp.Ui.Views;

public static class EventDialogViewBuilder
{
    public static UIElement Create(EventDialogViewModel model)
    {
        StackPanel layout = new StackPanel { Spacing = 12 };

        layout.Children.Add(new TextBlock { Text = model.Description, TextWrapping = TextWrapping.WrapWholeWords });
        layout.Children.Add(new TextBlock { Text = model.FormattedDate });
        layout.Children.Add(new TextBlock { Text = model.Location });
        layout.Children.Add(new TextBlock { Text = model.PriceText });
        layout.Children.Add(new TextBlock { Text = model.RatingText });
        layout.Children.Add(new TextBlock { Text = model.CapacityText });

        if (model.ShowJackpotBanner)
        {
            StackPanel bannerStack = new StackPanel { Spacing = 4 };

            StackPanel titleStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            titleStack.Children.Add(new FontIcon { Glyph = "\uEB52", FontSize = 14, Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xC8, 0x97, 0x1A)) });
            titleStack.Children.Add(new TextBlock { Text = model.JackpotBannerText, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });

            bannerStack.Children.Add(titleStack);
            bannerStack.Children.Add(new TextBlock { Text = model.JackpotDiscountedPriceText, FontWeight = Microsoft.UI.Text.FontWeights.Bold, FontSize = 16 });

            layout.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xC1, 0x07)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 4, 0, 4),
                Child = bannerStack
            });
        }
        else if (model.ShowRegularDiscountBanner)
        {
            StackPanel discountStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            discountStack.Children.Add(new FontIcon { Glyph = "\uE8EC", FontSize = 14 });
            discountStack.Children.Add(new TextBlock { Text = model.RegularDiscountedPriceText, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });

            layout.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xC1, 0x07)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 8, 12, 8),
                Child = discountStack
            });
        }

        TextBox referralTextBox = new TextBox { PlaceholderText = "Optional referral code", Width = 200 };
        Button validationButton = new Button { Content = new FontIcon { Glyph = "\uE73E", FontSize = 14 } };

        validationButton.Click += async (object sender, RoutedEventArgs e) =>
        {
            if (model.ValidateReferralAction is not null)
            {
                bool isValid = await model.ValidateReferralAction(referralTextBox.Text);
                referralTextBox.BorderBrush = new SolidColorBrush(isValid ? Microsoft.UI.Colors.Green : Microsoft.UI.Colors.Red);
                referralTextBox.BorderThickness = new Thickness(2);
            }
        };

        layout.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { referralTextBox, validationButton }
        });

        Button willAttendButton = new Button { Content = "Will attend", Tag = "Joined!" };
        Button buyTicketButton = new Button { Content = "Buy ticket", Tag = "Ticket purchased!" };

        willAttendButton.Click += async (object sender, RoutedEventArgs e) => await HandleJoinClick(willAttendButton, model.Event.Id);
        buyTicketButton.Click += async (object sender, RoutedEventArgs e) => await HandleJoinClick(buyTicketButton, model.Event.Id);

        Button seatGuideButton = new Button { Content = "Seat guide" };
        seatGuideButton.Click += (object sender, RoutedEventArgs e) => model.ShowSeatGuideAction?.Invoke();

        layout.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { willAttendButton, buyTicketButton, seatGuideButton }
        });

        if (model.HasFreePass)
        {
            Button freePassButton = new Button { HorizontalAlignment = HorizontalAlignment.Left };

            StackPanel buttonContent = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            buttonContent.Children.Add(new FontIcon { Glyph = "\uE7BF", FontSize = 14 });
            buttonContent.Children.Add(new TextBlock { Text = $"Use Free Pass ({model.FreePassBalance} left)" });
            freePassButton.Content = buttonContent;

            freePassButton.Click += async (object sender, RoutedEventArgs e) =>
            {
                if (model.UseFreePassAction is not null)
                {
                    bool success = await model.UseFreePassAction();
                    if (success)
                    {
                        StackPanel appliedContent = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                        appliedContent.Children.Add(new FontIcon { Glyph = "\uE73E", FontSize = 14, Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green) });
                        appliedContent.Children.Add(new TextBlock { Text = "Free Pass applied!" });

                        freePassButton.Content = appliedContent;
                        freePassButton.IsEnabled = false;
                    }
                }
            };

            layout.Children.Add(freePassButton);
        }

        return layout;
    }

    private static async Task HandleJoinClick(Button button, int eventId)
    {
        button.IsEnabled = false;

        if (App.EventJoinService is not null)
        {
            string tag = button.Tag?.ToString() ?? string.Empty;
            JoinEventResult result = await App.EventJoinService.JoinEventAsync(eventId, tag);
            button.Content = result.Message;
        }
    }
}