using GospelSub.Core;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GospelSub.WPF
{
    /// <summary>
    /// SettingsWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingsPage : Page
    {
        private readonly System.Windows.Forms.FontDialog f;
        private readonly System.Windows.Forms.ColorDialog c;
        private readonly Microsoft.Win32.OpenFileDialog fileDialog;

        public SettingsPage()
        {
            InitializeComponent();
            f = new System.Windows.Forms.FontDialog() { FontMustExist = true, ShowColor = false };
            c = new System.Windows.Forms.ColorDialog() { ShowHelp = true, AnyColor = true, AllowFullOpen = true };
            fileDialog = new Microsoft.Win32.OpenFileDialog() { CheckFileExists = true, CheckPathExists = true, AddExtension = true };
        }

        private void Fontbutton_Click(object sender, RoutedEventArgs e)
        {
            Setting.Current.Font = f.ShowDialog() == System.Windows.Forms.DialogResult.OK ? new FontFamily(f.Font.Name) : new FontFamily();
        }

        private void Gobutton_Click(object sender, RoutedEventArgs e) => new DisplayWindow().Show();

        private void BackFilebutton_Click(object sender, RoutedEventArgs e)
        {
            fileDialog.Title = "Open File for Background Picture";
            if (fileDialog.ShowDialog() == true) Setting.Current.Background = new ImageBrush(new BitmapImage(new Uri(fileDialog.FileName)));
        }

        private void Colorbutton_Click(object sender, RoutedEventArgs e)
        {
            if (c.ShowDialog() == System.Windows.Forms.DialogResult.OK) Setting.Current.Color = c.Color.ToWindowsColor();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) => Setting.Current.Black = true;

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) => Setting.Current.Black = false;
    }
}
