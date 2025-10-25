using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Yuuki.Views.Pages
{
    /// <summary>
    /// 设置页面
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            // TODO: Load settings from ConfigManager
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Implement theme switching
        }
    }
}
