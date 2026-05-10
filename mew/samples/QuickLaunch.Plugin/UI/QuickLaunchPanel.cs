using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core.Shell;
using QuickLaunch.Plugin.Models;
using QuickLaunch.Plugin.Services;
using System.Collections.Generic;

namespace QuickLaunch.Plugin.UI
{
    internal sealed class QuickLaunchPanel
    {
        private readonly ShellContext _shell;
        private readonly LaunchService _service;
        private LaunchConfig _config;
        private string _selectedCategoryId = string.Empty;
        private readonly HashSet<string> _expandedRootIds = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Label> _subCategoryLabels = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, LaunchCategory> _subCategoryMap = new(StringComparer.OrdinalIgnoreCase);

        public event Action<string>? CategorySelected;

        public QuickLaunchPanel(ShellContext shell, LaunchService service)
        {
            _shell = shell;
            _service = service;
            _config = _service.GetCurrentConfig();
        }

        public FrameworkElement CreateContent()
        {
            var panel = new StackPanel().Vertical();

            var rootCategories = _config.GetRootCategories();
            foreach (var category in rootCategories)
            {
                if (!_expandedRootIds.Contains(category.Id))
                {
                    _expandedRootIds.Add(category.Id);
                }

                var rootLabel = new Label
                {
                    Text = $"▼ {category.Icon} {category.Name}",
                    FontSize = 12,
                };

                var rootButton = new Button
                {
                    Content = rootLabel,
                    Margin = new Thickness(5, 2, 5, 2),
                    MinHeight = 32,
                };

                panel.Children(rootButton);

                var subButtons = new List<Button>();

                var subCategories = _config.GetSubCategories(category.Id);
                foreach (var sub in subCategories)
                {
                    var subId = sub.Id;
                    _subCategoryMap[subId] = sub;

                    var subLabel = new Label
                    {
                        Text = $"  • {sub.Icon} {sub.Name}",
                        FontSize = 11,
                    };
                    _subCategoryLabels[subId] = subLabel;

                    var btn = new Button
                    {
                        Content = subLabel,
                        Margin = new Thickness(20, 2, 5, 2),
                        MinHeight = 28,
                    };

                    btn.Click += () =>
                    {
                        _selectedCategoryId = subId;
                        UpdateSubCategorySelection();
                        CategorySelected?.Invoke(subId);
                    };

                    subButtons.Add(btn);
                    panel.Children(btn);
                }

                rootButton.Click += () =>
                {
                    var isExpanded = ToggleExpanded(category.Id);
                    rootLabel.Text = $"{(isExpanded ? "▼" : "▶")} {category.Icon} {category.Name}";
                    foreach (var subButton in subButtons)
                    {
                        subButton.IsVisible = isExpanded;
                    }
                };
            }

            UpdateSubCategorySelection();

            return panel;
        }

        private bool ToggleExpanded(string rootCategoryId)
        {
            if (_expandedRootIds.Contains(rootCategoryId))
            {
                _expandedRootIds.Remove(rootCategoryId);
                return false;
            }

            _expandedRootIds.Add(rootCategoryId);
            return true;
        }

        private void UpdateSubCategorySelection()
        {
            foreach (var pair in _subCategoryLabels)
            {
                if (!_subCategoryMap.TryGetValue(pair.Key, out var subCategory))
                {
                    continue;
                }

                var isSelected = string.Equals(pair.Key, _selectedCategoryId, StringComparison.OrdinalIgnoreCase);
                pair.Value.Text = isSelected
                    ? $"  ▸ {subCategory.Icon} {subCategory.Name}"
                    : $"  • {subCategory.Icon} {subCategory.Name}";
                pair.Value.FontWeight = isSelected ? FontWeight.SemiBold : FontWeight.Normal;
            }
        }

        public string GetSelectedCategoryId() => _selectedCategoryId;
    }
}
