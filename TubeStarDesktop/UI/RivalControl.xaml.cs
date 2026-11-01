using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TubeStar
{
    /// <summary>
    /// Interaction logic for RivalControl.xaml
    /// </summary>
    public partial class RivalControl : UserControl
    {
        private Channel _currentChannel;

        //Cores dos avatares (derivadas do nome do canal)
        private static readonly string[] AvatarColors = { "#E5534B", "#3EA66B", "#9B7BD6", "#E3A23B", "#3B8FE3", "#D65BA4", "#2FB3A6", "#C7743B" };

        public RivalControl()
        {
            InitializeComponent();
            PopulateRivals();
            Translate();
        }

        private void PopulateRivals()
        {
            foreach (var rival in Rivals.Current.All)
            {
                if (rival.Channel == null)
                {
                    rival.CreateChannel();
                }
            }
        }

        private void Translate()
        {
            txtTitle.Text = EnglishStrings.TopTubeStars.Translate();
            txtLead.Text = EnglishStrings.TopTubeStarsLead.Translate();
            txtSoonTwitch.Text = txtSoonPodcast.Text = EnglishStrings.Soon.Translate();
            thChannel.Text = EnglishStrings.Channel.Translate().ToUpper();
            thSubscribers.Text = EnglishStrings.Subscribers.Translate().ToUpper();
            thViews.Text = EnglishStrings.Views.Translate().ToUpper();
            btnBack.Content = "‹ " + EnglishStrings.BackToTop.Translate();
        }

        public void Update()
        {
            List<Channel> channels = new List<Channel>();
            foreach (var rival in Rivals.Current.All)
            {
                if (rival.Channel != null)
                {
                    channels.Add(rival.Channel);
                }
            }
            //Uma linha "voce" para cada canal do jogador
            foreach (var channel in Player.Current.Channels)
            {
                if (channel != null && channel != Channel.UnreleasedVideos)
                    channels.Add(channel);
            }

            //Maior numero de inscritos primeiro
            channels = channels.OrderByDescending(c => c.Subscribers).ToList();

            if (channels.Count > 0 && !channels[0].IsRivalChannel)
            {
                TrophyManager.UnlockTrophy(Trophy.King);
            }

            rankingPanel.Children.Clear();
            for (int i = 0; i < channels.Count; i++)
            {
                rankingPanel.Children.Add(CreateRow(i + 1, channels[i]));
            }

            if (channelView.Visibility == Visibility.Visible)
                ShowChannel(_currentChannel);
        }

        private Border CreateRow(int position, Channel channel)
        {
            bool isMe = !channel.IsRivalChannel;

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(44) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(46) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });

            var txtPosition = new TextBlock { Text = position.ToString(), FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center };
            grid.Children.Add(txtPosition);

            var avatar = CreateAvatar(channel.Name);
            Grid.SetColumn(avatar, 1);
            grid.Children.Add(avatar);

            var namePanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            namePanel.Children.Add(new TextBlock { Text = channel.Name, FontWeight = FontWeights.Bold, TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center });
            if (isMe)
            {
                var pill = new Border { Style = (Style)FindResource("PillRedStyle"), Margin = new Thickness(8, 0, 0, 0) };
                pill.Child = new TextBlock { Text = EnglishStrings.You.Translate(), Style = (Style)FindResource("InheritTextStyle") };
                namePanel.Children.Add(pill);
            }
            Grid.SetColumn(namePanel, 2);
            grid.Children.Add(namePanel);

            var txtSubs = new TextBlock { Text = channel.Subscribers.ToNumberString(), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(txtSubs, 3);
            grid.Children.Add(txtSubs);

            var txtViews = new TextBlock { Text = GetTotalViews(channel).ToNumberString(), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(txtViews, 4);
            grid.Children.Add(txtViews);

            var row = new Border { Style = (Style)FindResource(isMe ? "TableRowMeStyle" : "TableRowStyle"), Child = grid };
            //Clique no rival desativado: o perfil completo entra no Top TubeStars (roadmap item 2)
            return row;
        }

        private static int GetTotalViews(Channel channel)
        {
            long total = 0;
            if (channel.Videos != null)
            {
                foreach (var video in channel.Videos)
                    total += video.Views;
            }
            return (int)Math.Min(total, int.MaxValue);
        }

        private static FrameworkElement CreateAvatar(string name)
        {
            if (String.IsNullOrEmpty(name))
                name = "?";

            int hash = 0;
            foreach (char c in name)
                hash = (hash * 31 + c) & 0x7FFFFFFF;

            var color = (Color)ColorConverter.ConvertFromString(AvatarColors[hash % AvatarColors.Length]);
            var circle = new Border
            {
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(15),
                Background = new SolidColorBrush(color),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            circle.Child = new TextBlock
            {
                Text = GetInitial(name),
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            return circle;
        }

        private static string GetInitial(string name)
        {
            foreach (char c in name)
            {
                if (Char.IsLetterOrDigit(c))
                    return Char.ToUpper(c).ToString();
            }
            return "?";
        }

        private void ShowChannel(Channel channel)
        {
            if (channel == null)
                return;

            channelNameTextBlock.Text = channel.Name;
            channelSummaryTextBlock.Text = String.Format("{0}: {1}", EnglishStrings.Subscribers.Translate(), channel.Subscribers.ToNumberString());

            videoPanelGrid.Children.Clear();
            foreach (var video in channel.Videos)
            {
                var videoBlock = new VideoBlockGrid(channel, video);
                videoPanelGrid.Children.Insert(0, videoBlock);
            }
        }

        internal void NewDay()
        {
            if (RandomHelpers.Chance(75))
            {
                Rivals.Current.All[RandomHelpers.RandomInt(Rivals.Current.All.Count)].AddNewVideo();
            }
            if (RandomHelpers.Chance(75))
            {
                Rivals.Current.All[RandomHelpers.RandomInt(Rivals.Current.All.Count)].AddNewVideo();
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            channelView.Visibility = Visibility.Collapsed;
            rankingView.Visibility = Visibility.Visible;
            Update();
        }
    }
}
