using GospelSub.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace GospelSub.WPF
{
    /// <summary>
    /// MainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainPage : Page
    {
        private ObservableCollection<Gospel> searched;
        private Brush border;
        private int i;

        public MainPage()
        {
            InitializeComponent();
            listBox.DataContext = Setting.Current.Gospels;
            searched = new ObservableCollection<Gospel>();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e) => border = listBox.BorderBrush;

        private void Startbutton_Click(object sender, RoutedEventArgs e)
        {
            if (!Setting.Current.Gospels.Exists((g) => g.Chosen) || !searched.Any((g) => g.Chosen)) MessageBox.Show("Select!!");
            else if (Setting.Current.Gospels.Count((g) => g.Chosen) > 9) MessageBox.Show("Select Less than 10");
            else
            {
                Setting.Current.Sequence.Clear();
                Setting.Current.Sequence.AddRange(from Gospel g in searched
                                                  where g.Chosen
                                                  select g);
                NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            searched = new ObservableCollection<Gospel>(from g in Setting.Current.Gospels
                                                        where g.Name.ToLower().Contains(TextBox.Text.ToLower())
                                                        orderby g.Chosen descending
                                                        select g);
            listBox.BorderBrush = searched.Count == 0 ? Brushes.Red : border;
            listBox.DataContext = searched;
        }
        
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (FigureList())
                {
                    case 0: (ButtonUp.IsEnabled, ButtonDown.IsEnabled) = (false, false); break;
                    case -1: (ButtonUp.IsEnabled, ButtonDown.IsEnabled) = (true, false); break;
                    case 1: (ButtonUp.IsEnabled, ButtonDown.IsEnabled) = (false, true); break;
                }
            }
            catch (ArgumentException) { (ButtonUp.IsEnabled, ButtonDown.IsEnabled) = (true, true); }
        }

        private int FigureList()
        {
            if (listBox.SelectedIndex == -1 || !(listBox.SelectedItem as Gospel).Chosen) return 0;
            else if (searched[listBox.SelectedIndex + 1]?.Chosen == false) return -1;
            else if (listBox.SelectedIndex == 0) return 1;
            else throw new ArgumentException();
        }

        private void ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            i = listBox.SelectedIndex;
            searched.Move(i, --i);
            searched[i].Chosen = true;
        }

        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            i = listBox.SelectedIndex;
            searched.Move(i, ++i);
            searched[i].Chosen = true;
        }
    }
}
