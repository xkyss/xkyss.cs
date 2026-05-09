namespace MewPad.Hosting.Extensions;

using Aprillz.MewUI.Controls;
using MewPad.Core.Interfaces;

public class WelcomeActivity : IActivityItem
{
    public string Id => "welcome";
    public object Icon => "📋";
    public string Title => "Welcome";
    public int Order => 10;

    public FrameworkElement CreateContent() =>
        new Label().Text("Welcome to MewPad - Universal Desktop Framework");
}