using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Yuuki.Views.Pages;

namespace Yuuki
{
    /// <summary>
    /// 主窗口
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Title = "Yuuki - Minecraft 启动器";

            // Set default size
            var appWindow = this.AppWindow;
            appWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));

            // Navigate to home page on startup
            ContentFrame.Navigate(typeof(HomePage));
            NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems[0];
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else if (args.InvokedItemContainer != null)
            {
                var tag = args.InvokedItemContainer.Tag?.ToString();
                NavigateToPage(tag);
            }
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else if (args.SelectedItemContainer != null)
            {
                var tag = args.SelectedItemContainer.Tag?.ToString();
                NavigateToPage(tag);
            }
        }

        private void NavigateToPage(string? tag)
        {
            if (string.IsNullOrEmpty(tag))
                return;

            Type? pageType = tag switch
            {
                "Home" => typeof(HomePage),
                "Versions" => typeof(VersionsPage),
                "Mods" => typeof(ModsPage),
                "Accounts" => typeof(AccountsPage),
                _ => null
            };

            if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
            {
                ContentFrame.Navigate(pageType);
            }
        }
    }
}
