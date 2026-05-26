using Mewoo.Abstractions.Commands;
using Mewoo.Abstractions.Views;

namespace Mewoo.Abstractions.Contributions;

public interface IMewooContributionRegistry
{
    IActivityContributionBuilder Activity(string id);

    IViewContainerContributionBuilder ViewContainer(string id);

    IMainViewContributionBuilder MainView(string id);

    ICommandContributionBuilder Command(string id);

    IStatusBarItemContributionBuilder StatusBarItem(string id);

    IThemeTokenOverrideContributionBuilder ThemeTokenOverride(string id);
}

public interface IActivityContributionBuilder
{
    IActivityContributionBuilder Title(string title);

    IActivityContributionBuilder Icon(string icon);

    IActivityContributionBuilder ViewContainer(string viewContainerId);

    IActivityContributionBuilder Order(int order);
}

public interface IViewContainerContributionBuilder
{
    IViewContainerContributionBuilder Title(string title);

    IViewContainerContributionBuilder AddView(string id, Action<ISidebarViewContributionBuilder> configure);
}

public interface ISidebarViewContributionBuilder
{
    ISidebarViewContributionBuilder Title(string title);

    ISidebarViewContributionBuilder Create(Func<IMewooViewContext, IMewooView> createView);
}

public interface IMainViewContributionBuilder
{
    IMainViewContributionBuilder Title(string title);

    IMainViewContributionBuilder CanOpenMultiple(bool canOpenMultiple);

    IMainViewContributionBuilder Create(Func<IMewooViewContext, IMewooView> createView);
}

public interface ICommandContributionBuilder
{
    ICommandContributionBuilder Title(string title);

    ICommandContributionBuilder Category(string category);

    ICommandContributionBuilder Icon(string icon);

    ICommandContributionBuilder DefaultKeyBinding(string defaultKeyBinding);

    ICommandContributionBuilder CanExecute(Func<IMewooCommandContext, bool> canExecute);

    ICommandContributionBuilder Execute(Func<IMewooCommandContext, CancellationToken, ValueTask> executeAsync);
}

public interface IStatusBarItemContributionBuilder
{
    IStatusBarItemContributionBuilder AlignLeft();

    IStatusBarItemContributionBuilder AlignRight();

    IStatusBarItemContributionBuilder Text(string text);

    IStatusBarItemContributionBuilder Command(string commandId);
}

public interface IThemeTokenOverrideContributionBuilder
{
    IThemeTokenOverrideContributionBuilder Token(string key, object value);
}

