using GospelSub.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GospelSub.WPF
{
    /// <summary>
    /// DisplayWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DisplayWindow : Window
    {
        private Gospel gospel;
        private int current = 0;
        private int index = 0;
        private string raw = string.Empty;
        private readonly List<TextBlock> texts;
        private readonly List<Storyboard> emergeBoards;
        private readonly List<Storyboard> shrinkBoards;
        private readonly List<DoubleAnimation> emerges;
        private readonly List<DoubleAnimation> shrinks;
        private bool[] emerged;

        internal int Index
        {
            get => index;

            set
            {
                try
                {
                    shrinkBoards.ForEach(b => b.Begin(this, true));
                    shrinkBoards.ForEach(b => b.SkipToFill(this));
                    emerged = Enumerable.Repeat(false, 3).ToArray();
                    Text = gospel[value].IsTag() == 1 ? gospel[value].Substring(3) : gospel[value];
                    index = value;
                }
                catch (IndexOutOfRangeException e)
                {
                    try
                    {
                        switch (e.Message)
                        {
                            case "Over": Current++; Index = 0; break;
                            case "Below": Current--; Index = 0; break;
                        }
                    }
                    catch (ArgumentException) { }
                    catch (IndexOutOfRangeException) {  }
                }
            }
        }

        internal int Current
        {
            get => current;
            set
            {
                if (value < 0 || value >= Setting.Current.Sequence.Count) throw new IndexOutOfRangeException();
                current = value;
                Cursor = Cursors.Wait;
                DisposeTags();
                gospel = Setting.Current.Sequence[current];
                if (gospel.Tagged) RegisterTags();
                Cursor = Cursors.None;
            }
        }

        private string Text
        {
            get => raw;
            set
            {
                raw = value;
                if (raw == gospel.Name)
                {
                    (texts[0].Text, texts[1].Text) = (string.Empty, string.Empty);
                    texts[2].Text = raw;
                    linePanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    linePanel.Visibility = Visibility.Visible;
                    string[] vs = raw.Split(new string[1] { @"
" }, StringSplitOptions.RemoveEmptyEntries);
                    texts[2].Text = vs.Length == 2 ? vs[1] : string.Empty;
                    double sigma = vs[0].Length / 2;
                    int ideal = (int)Math.Round(sigma);

                    var res = from i in Enumerable.Range(0, vs[0].Length)
                              where vs[0][i] == ' '
                              orderby Math.Abs(sigma - i)
                              select i;
                    int tocut = res.First();
                    texts[0].Text = vs[0].Substring(0, tocut);
                    texts[1].Text = vs[0].Substring(tocut);
                }
            }
        }

        private void DisposeTags()
        {
            _ = from key in Setting.Current.KeyMaps
                where key.Value.StartsWith("Go")
                select Methods.Remove(key.Value);

            var res = from k in Setting.Current.KeyMaps
                      where k.Value.StartsWith("Go")
                      select k;
            res.ToList().ForEach(k => Setting.Current.KeyMaps.Remove(k.Key));
        }

        private void RegisterTags()
        {
            string nameofmethod;
            foreach (var item in gospel.Tags)
            {
                Setting.Current.KeyMaps[item.Key] = nameofmethod = $"Go{(int)item.Key}To{item.Value}";
                Methods[nameofmethod] = () => Index = item.Value;
            }
        }

        private Dictionary<string, Action> Methods;

        public DisplayWindow()
        {
            InitializeComponent();
            switch (Setting.Current.Background)
            {
                case ImageBrush i:
                    i.Stretch = Stretch.UniformToFill;
                    Background = i; break;
                case Brush b:
                    Background = b; break;
            }
            texts = new List<TextBlock>(new TextBlock[3]{ Beta, Gamma, Alpha });
            texts.ForEach(t => t.FontFamily = Setting.Current.Font);
            texts.ForEach(t => (t.Foreground, t.FontWeight) = (new SolidColorBrush(Setting.Current.Color), FontWeights.UltraBold));
            Setting.Current.KeyMaps = new Dictionary<Key, string>()
            {
                [Key.Home] = nameof(First),
                [Key.End] = nameof(Last),
                //[Key.Up] = nameof(FontUp),
                //[Key.Down] = nameof(FontDown),
                [Key.Left] = nameof(Prev),
                [Key.Space] = nameof(Prev),
                [Key.Right] = nameof(Next),
                [Key.Enter] = nameof(Next),
                [Key.PageUp] = nameof(PrevGospel),
                [Key.PageDown] = nameof(NextGospel),
                [Key.Escape] = nameof(Close),
                [Key.Insert] = nameof(Anima),
                [Key.Delete] = nameof(Kill),
            };
            Methods = new Dictionary<string, Action>()
            {
                [nameof(First)] = First,
                [nameof(Last)] = Last,
                //[nameof(FontUp)] = FontUp,
                //[nameof(FontDown)] = FontDown,
                [nameof(Prev)] = Prev,
                [nameof(Next)] = Next,
                [nameof(PrevGospel)] = PrevGospel,
                [nameof(NextGospel)] = NextGospel,
                [nameof(Close)] = Close,
                [nameof(Anima)] = Anima,
                [nameof(Kill)] = Kill,
            };
            emerges = new List<DoubleAnimation>(3);
            shrinks = new List<DoubleAnimation>(3);
            emergeBoards = new List<Storyboard>(3);
            shrinkBoards = new List<Storyboard>(3);
            for (int i = 0; i < 3; i++)
            {
                emerges.Add(new DoubleAnimation(0, 1, new Duration(new TimeSpan(0, 0, 0, 1, 500))));
                shrinks.Add(new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 1))));

                emergeBoards.Add(new Storyboard());
                emergeBoards[i].Children.Add(emerges[i]);
                Storyboard.SetTargetName(emerges[i], i == 0 ? "Beta" : i == 1 ? "Gamma" : "Alpha");
                Storyboard.SetTargetProperty(emerges[i], new PropertyPath(TextBlock.OpacityProperty));

                shrinkBoards.Add(new Storyboard());
                shrinkBoards[i].Children.Add(shrinks[i]);
                Storyboard.SetTargetName(shrinks[i], i == 0 ? "Beta" : i == 1 ? "Gamma" : "Alpha");
                Storyboard.SetTargetProperty(shrinks[i], new PropertyPath(TextBlock.OpacityProperty));
            }
            emerged = Enumerable.Repeat(false, 3).ToArray();
            Current = 0;
            Index = 0;
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left: Next(); break;
                case MouseButton.Right: Prev(); break;
            }
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                Methods[Setting.Current.KeyMaps[e.Key]].Invoke();
            }
            catch (KeyNotFoundException) { }
        }

        private void First() => Index = 0;
        private void Last() => Index = gospel.Count - 1;
        //private void FontUp() => Parallel.ForEach(texts, t => t.FontSize += 5);
        //private void FontDown() => Parallel.ForEach(texts, t => t.FontSize -= (Alpha.FontSize > 5) ? 5 : 0);
        private void Prev() => Index--;
        private void Next() => Index++;
        private void PrevGospel() => Index = -1;
        private void NextGospel() => Index = gospel.Count;
        private void Anima()
        {
            for (int i = 0; i < 3; i++)
            {
                if (string.IsNullOrEmpty(texts[i].Text)) continue;
                if (!emerged[i])
                {
                    emergeBoards[i].Begin(this, true);
                    emerged[i] = true;
                    break;
                }
            }
        }
        private void Kill()
        {
            for (int i = 0; i < 3; i++)
            {
                if (emerged[i])
                {
                    shrinkBoards[i].Begin(this, true);
                    emerged[i] = false;
                }
            }
        }
    }
}
