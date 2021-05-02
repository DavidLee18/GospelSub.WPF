using System;
using System.Windows;

namespace GospelSub.WPF
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow() => InitializeComponent();

        private void Window_Closed(object sender, EventArgs e) => Application.Current.Shutdown();
    }
}
