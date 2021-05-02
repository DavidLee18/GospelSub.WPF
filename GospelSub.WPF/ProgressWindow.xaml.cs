using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.IO;
using GospelSub.Core;
using System.Collections.Generic;

namespace GospelSub.WPF
{
    /// <summary>
    /// ProgressWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProgressWindow : Window
    {
        private int index = 0;
        private int Index
        {
            get => index;
            set
            {
                index = value > count ? count : value;
                progressBar.Value = value / count;
            }
        }
        private int count = 0;
        public ProgressWindow() => InitializeComponent();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            DirectoryInfo info = new DirectoryInfo(Environment.CurrentDirectory);
            textProgress.Text = "Searching files...";
            FileInfo[] files = info.EnumerateFiles("*.txt", SearchOption.AllDirectories).ToArray();
            count = files.Length * 2;
            textProgress.Text = "Reading Gospels...";
            foreach (var item in files)
            {
                textProgress.Text = $"Reading {item.Name}...";
                Setting.Current.Gospels.Add(Gospel.Load(item.OpenText())); Index++;
                textProgress.Text = $"Tagging {Setting.Current.Gospels.Last().Name}...";
                Setting.Current.Gospels.Last().ExtractTags(); Index++;
            }
            Setting.Current.Gospels = new List<Gospel>(Setting.Current.Gospels.OrderBy(g => g.Name));
            textProgress.Text = "Loading Complete.";
            Cursor = Cursors.Arrow;
            Close();
        }

        private void Window_Closing(object sender, EventArgs e) => new MainWindow().Show();
    }
}
