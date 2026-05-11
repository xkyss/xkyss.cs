using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core.Components.Card;
using MewPad.Core.Interfaces;
using MewPad.Core.Shell;
using QuickLaunch.Plugin.Models;
using QuickLaunch.Plugin.Services;
using System.Collections.Generic;
using System.Linq;

namespace QuickLaunch.Plugin.UI
{
    internal sealed class QuickLaunchContent : IContentItem
    {
        private readonly ShellContext _shell;
        private readonly LaunchService _service;
        private readonly string? _categoryId;
        private string? _selectedItemId;

        public string Id => "quicklaunch.content";
        public string Title => "快速启动";
        public object? Icon => "⚡";
        public bool CanClose => false;

        public QuickLaunchContent(ShellContext shell, LaunchService service, string? categoryId = null)
        {
            _shell = shell;
            _service = service;
            _categoryId = categoryId;
        }

        public FrameworkElement CreateContent()
        {
            var rootPanel = new StackPanel().Vertical();

            if (string.IsNullOrEmpty(_categoryId))
            {
                rootPanel.Children(new Label
                {
                    Text = "请选择左侧二级分类查看启动项",
                    FontSize = 12,
                    Margin = new Thickness(20),
                });
                return rootPanel;
            }

            var config = _service.GetCurrentConfig();
            var allItems = config.GetItemsByCategory(_categoryId).ToList();

            var category = config.FindCategory(_categoryId);

            if (allItems.Count == 0)
            {
                rootPanel.Children(new Label
                {
                    Text = "该分类暂无启动项",
                    FontSize = 12,
                    Margin = new Thickness(20),
                });
                return rootPanel;
            }

            var headerTitle = new Label
            {
                Text = category == null ? "QuickLaunch" : $"{category.Icon} {category.Name}",
                FontSize = 14,
                FontWeight = FontWeight.SemiBold,
                Margin = new Thickness(12, 10, 12, 6),
            };
            rootPanel.Children(headerTitle);

            var activeTabKey = "all";
            var tabButtons = new Dictionary<string, (Label Label, string BaseText)>(StringComparer.OrdinalIgnoreCase);
            var bodyHost = new Border { Margin = new Thickness(8, 0, 8, 8) };

            void RefreshTabStyles()
            {
                foreach (var pair in tabButtons)
                {
                    var isActive = string.Equals(pair.Key, activeTabKey, StringComparison.OrdinalIgnoreCase);
                    pair.Value.Label.Text = isActive ? $"[{pair.Value.BaseText}]" : pair.Value.BaseText;
                    pair.Value.Label.FontWeight = isActive ? FontWeight.SemiBold : FontWeight.Normal;
                }
            }

            IEnumerable<LaunchItem> FilterItems()
            {
                return activeTabKey switch
                {
                    "web" => allItems.Where(i => i.Type == LaunchItemType.Web),
                    "app" => allItems.Where(i => i.Type == LaunchItemType.Application),
                    "script" => allItems.Where(i => i.Type == LaunchItemType.Script),
                    _ => allItems
                };
            }

            void RenderCardBody()
            {
                var filtered = FilterItems().ToList();
                var body = new StackPanel().Vertical();

                if (filtered.Count == 0)
                {
                    body.Children(new Label
                    {
                        Text = "当前页签暂无启动项",
                        FontSize = 11,
                        Margin = new Thickness(8),
                    });
                    bodyHost.Child = body;
                    return;
                }

                var rowPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
                var cardCount = 0;

                foreach (var item in filtered)
                {
                    var cardButton = CreateCardButton(item);
                    rowPanel.Children(cardButton);
                    cardCount++;

                    if (cardCount >= 3)
                    {
                        body.Children(rowPanel);
                        rowPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
                        cardCount = 0;
                    }
                }

                if (cardCount > 0)
                {
                    body.Children(rowPanel);
                }

                bodyHost.Child = body;
            }

            Button CreateTabButton(string key, string text)
            {
                var label = new Label { Text = text, FontSize = 11 };
                tabButtons[key] = (label, text);

                var button = new Button
                {
                    Content = label,
                    Margin = new Thickness(0, 0, 6, 0),
                    MinHeight = 26,
                    Padding = new Thickness(8, 4),
                };

                button.Click += () =>
                {
                    activeTabKey = key;
                    RefreshTabStyles();
                    RenderCardBody();
                };

                return button;
            }

            var tabsRow = new StackPanel().Horizontal().Children(
                CreateTabButton("all", "全部"),
                CreateTabButton("web", "网页"),
                CreateTabButton("app", "应用"),
                CreateTabButton("script", "脚本")
            );

            rootPanel.Children(
                new StackPanel().Horizontal().Children(
                    new Label { Text = "", MinWidth = 12 },
                    tabsRow
                ),
                new Label { Text = "────────────────────────────────────────", FontSize = 9, Margin = new Thickness(12, 4, 12, 8) },
                bodyHost
            );

            RefreshTabStyles();
            RenderCardBody();

            return rootPanel;
        }

        private FrameworkElement CreateCardButton(LaunchItem item)
        {
            var typeText = item.Type switch
            {
                LaunchItemType.Web => "网址",
                LaunchItemType.Application => "应用",
                LaunchItemType.Script => "脚本",
                _ => "项目"
            };

            // Body: large icon on left, desc + target on right
            var iconLabel = new Label
            {
                Text = string.IsNullOrWhiteSpace(item.Icon) ? "📦" : item.Icon,
                FontSize = 32,
                Width = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
            };

            var descLabel = new Label
            {
                Text = string.IsNullOrWhiteSpace(item.Description) ? item.Name : item.Description,
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            descLabel.WithTheme((t, l) => l.Foreground = t.Palette.DisabledText);

            var targetLabel = new Label
            {
                Text = item.Target,
                FontSize = 9,
                VerticalAlignment = VerticalAlignment.Top,
            };
            targetLabel.WithTheme((t, l) => l.Foreground = t.Palette.DisabledText);

            var info = new StackPanel { Orientation = Orientation.Vertical, Spacing = 3 };
            info.Add(descLabel);
            info.Add(targetLabel);

            var bodyRow = new DockPanel();
            DockPanel.SetDock(iconLabel, Dock.Left);
            bodyRow.Add(iconLabel);
            bodyRow.Add(info);

            // Extra: type badge with accent color
            var typeExtra = new Label { Text = typeText, FontSize = 10 };
            typeExtra.WithTheme((t, l) => l.Foreground = t.Palette.Accent);

            // Actions
            var launchBtn = new Button { Content = new Label { Text = "启动", FontSize = 11 } };
            launchBtn.Click += () =>
            {
                _selectedItemId = item.Id;
                _service.ExecuteItem(item);
            };

            var editBtn = new Button { Content = new Label { Text = "编辑", FontSize = 11 } };
            editBtn.Click += () =>
            {
                _selectedItemId = item.Id;
                System.Diagnostics.Debug.WriteLine($"QuickLaunch edit requested: {item.Name}");
            };

            return new Card { Width = 260 }
                .Title(item.Name)
                .Extra(typeExtra)
                .Size(CardSize.Small)
                .Hoverable()
                .Body(bodyRow)
                .Actions(launchBtn, editBtn);
        }
    }
}
