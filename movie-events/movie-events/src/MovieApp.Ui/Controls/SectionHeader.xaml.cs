using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MovieApp.Ui.Controls;

public sealed partial class SectionHeader : UserControl
{
    public SectionHeader()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(SectionHeader),
            new PropertyMetadata(string.Empty));

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public static readonly DependencyProperty ActionTextProperty =
        DependencyProperty.Register(
            nameof(ActionText),
            typeof(string),
            typeof(SectionHeader),
            new PropertyMetadata("See all"));
}