using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TubeStar
{
    /// <summary>
    /// Top TubeStars: destaques da semana, abas Geral / Cresceu na semana e tabela paginada
    /// </summary>
    public partial class RivalControl : UserControl
    {
        private const int PageSize = 50;

        private bool _weekTab;
        private int _page;

        //Cores dos avatares (derivadas do nome do canal)
        private static readonly string[] AvatarColors = { "#E5534B", "#3EA66B", "#9B7BD6", "#E3A23B", "#3B8FE3", "#D65BA4", "#2FB3A6", "#C7743B" };

        public RivalControl()
        {
            InitializeComponent();
            Translate();
        }

        private void Translate()
        {
            txtTitle.Text = EnglishStrings.TopTubeStars.Translate();
            txtLead.Text = EnglishStrings.TopTubeStarsLead.Translate();
            txtSoonTwitch.Text = txtSoonPodcast.Text = EnglishStrings.Soon.Translate();
            thChannel.Text = EnglishStrings.Channel.Translate().ToUpper();
            thSubscribers.Text = EnglishStrings.Subscribers.Translate().ToUpper();
            thWeek.Text = EnglishStrings.RankSubscribersWeek.Translate().ToUpper();
            btnTabAll.Content = EnglishStrings.RankTabAll.Translate();
            btnTabWeek.Content = EnglishStrings.RankTabWeek.Translate();
            btnGoToMe.Content = EnglishStrings.RankGoToMe.Translate();
            btnGoToTop.Content = EnglishStrings.RankGoToTop.Translate();
        }

        public void Update()
        {
            var ranking = Ranking.Current;
            ranking.Refresh(false);

            if (ranking.Channels.Count > 0 && ranking.Channels[0].IsPlayer)
                TrophyManager.UnlockTrophy(Trophy.King);

            UpdateHighlights(ranking);
            UpdateTable();
        }

        private List<RankChannel> GetList()
        {
            var channels = Ranking.Current.Channels;
            if (_weekTab)
                return channels.Where(c => c.WeekGrowth > 0).OrderByDescending(c => c.WeekGrowth).ToList();
            return channels;
        }

        private void UpdateTable()
        {
            btnTabAll.Style = (Style)FindResource(_weekTab ? "ChipButtonStyle" : "ChipActiveButtonStyle");
            btnTabWeek.Style = (Style)FindResource(_weekTab ? "ChipActiveButtonStyle" : "ChipButtonStyle");
            //Geral: views totais; Cresceu na semana: views da semana
            thViews.Text = (_weekTab ? EnglishStrings.RankViewsWeek : EnglishStrings.Views).Translate().ToUpper();

            var list = GetList();
            int pages = Math.Max(1, (list.Count + PageSize - 1) / PageSize);
            _page = Math.Max(0, Math.Min(_page, pages - 1));

            rankingPanel.Children.Clear();
            for (int i = _page * PageSize; i < Math.Min(list.Count, (_page + 1) * PageSize); i++)
                rankingPanel.Children.Add(CreateRow(i + 1, list[i]));

            txtPage.Text = String.Format(EnglishStrings.RankPage.Translate(), _page + 1, pages);
            btnPrevPage.IsEnabled = _page > 0;
            btnNextPage.IsEnabled = _page < pages - 1;
            btnGoToMe.Visibility = list.Any(c => c.IsPlayer) ? Visibility.Visible : Visibility.Collapsed;
        }

        //Maior alta e maior queda da semana (posições) e o melhor canal do jogador
        private void UpdateHighlights(Ranking ranking)
        {
            highlights.Children.Clear();

            var rise = ranking.Channels.Where(c => !c.IsPlayer).OrderByDescending(c => c.WeekChange).FirstOrDefault();
            var fall = ranking.Channels.Where(c => !c.IsPlayer).OrderBy(c => c.WeekChange).FirstOrDefault();
            var me = ranking.Channels.FirstOrDefault(c => c.IsPlayer);

            highlights.Children.Add(CreateHighlight(EnglishStrings.RankBiggestRise.Translate(), rise != null && rise.WeekChange > 0 ? rise.Name : "-", rise != null && rise.WeekChange > 0 ? rise.WeekChange : 0));
            highlights.Children.Add(CreateHighlight(EnglishStrings.RankBiggestFall.Translate(), fall != null && fall.WeekChange < 0 ? fall.Name : "-", fall != null && fall.WeekChange < 0 ? fall.WeekChange : 0));
            if (me != null)
                highlights.Children.Add(CreateHighlight(EnglishStrings.You.Translate(), "#" + me.Position + " · " + me.Name, me.WeekChange));
            else
                highlights.Children.Add(CreateHighlight(EnglishStrings.You.Translate(), EnglishStrings.RankNoChannel.Translate(), 0));
        }

        private Border CreateHighlight(string label, string value, int change)
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = label, Style = (Style)FindResource("KpiLabelStyle") });
            panel.Children.Add(new TextBlock { Text = value, FontWeight = FontWeights.Bold, FontSize = 15, Margin = new Thickness(0, 4, 0, 2), TextTrimming = TextTrimming.CharacterEllipsis });
            panel.Children.Add(CreateChangeText(change, EnglishStrings.RankThisWeek.Translate()));
            return new Border { Style = (Style)FindResource("CardBorderStyle"), Margin = new Thickness(6, 0, 6, 0), Child = panel };
        }

        //▲ verde, ▼ vermelho ou "–"
        private TextBlock CreateChangeText(int change, string suffix)
        {
            var text = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
            if (change > 0)
            {
                text.Text = "▲ " + change + suffix;
                text.Foreground = FindBrush("YtGainBrush");
            }
            else if (change < 0)
            {
                text.Text = "▼ " + (-change) + suffix;
                text.Foreground = FindBrush("YtLossBrush");
            }
            else
            {
                text.Text = "–";
                text.Foreground = FindBrush("YtTextSecondaryBrush");
            }
            return text;
        }

        //Variação da semana: "+1,234" verde, negativo vermelho
        private static TextBlock CreateGrowthText(long value)
        {
            return new TextBlock
            {
                Text = (value > 0 ? "+" : "") + value.ToShortNumber(),
                Foreground = FindBrush(value > 0 ? "YtGainBrush" : (value < 0 ? "YtLossBrush" : "YtTextSecondaryBrush")),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private static Brush FindBrush(string key)
        {
            return (Brush)Application.Current.FindResource(key);
        }

        private Border CreateRow(int position, RankChannel channel)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(52) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(46) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

            grid.Children.Add(new TextBlock { Text = position.ToString(), FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center });

            //Variação desde ontem (só no ranking geral)
            if (!_weekTab)
            {
                var change = CreateChangeText(channel.Change, "");
                change.FontSize = 12;
                Grid.SetColumn(change, 1);
                grid.Children.Add(change);
            }

            var avatar = CreateAvatar(channel.Name);
            Grid.SetColumn(avatar, 2);
            grid.Children.Add(avatar);

            var namePanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var titleLine = new StackPanel { Orientation = Orientation.Horizontal };
            titleLine.Children.Add(new TextBlock { Text = channel.Name, FontWeight = FontWeights.Bold, TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center });
            if (channel.IsPlayer)
            {
                var pill = new Border { Style = (Style)FindResource("PillRedStyle"), Margin = new Thickness(8, 0, 0, 0) };
                pill.Child = new TextBlock { Text = EnglishStrings.You.Translate(), Style = (Style)FindResource("InheritTextStyle") };
                titleLine.Children.Add(pill);
            }
            namePanel.Children.Add(titleLine);
            if (!String.IsNullOrEmpty(channel.Theme))
                namePanel.Children.Add(new TextBlock { Text = channel.Theme, Style = (Style)FindResource("MutedTextStyle"), FontSize = 11, TextTrimming = TextTrimming.CharacterEllipsis });
            Grid.SetColumn(namePanel, 3);
            grid.Children.Add(namePanel);

            var txtSubs = new TextBlock { Text = channel.Subscribers.ToShortNumber(), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(txtSubs, 4);
            grid.Children.Add(txtSubs);

            var txtWeek = CreateGrowthText(channel.WeekGrowth);
            Grid.SetColumn(txtWeek, 5);
            grid.Children.Add(txtWeek);

            var txtViews = _weekTab ? CreateGrowthText(channel.WeekViews) : new TextBlock { Text = channel.Views.ToShortNumber(), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(txtViews, 6);
            grid.Children.Add(txtViews);

            //Clique no canal: perfil entra na etapa 2b
            return new Border { Style = (Style)FindResource(channel.IsPlayer ? "TableRowMeStyle" : "TableRowStyle"), Child = grid };
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

        private void btnTabAll_Click(object sender, RoutedEventArgs e)
        {
            _weekTab = false;
            _page = 0;
            UpdateTable();
        }

        private void btnTabWeek_Click(object sender, RoutedEventArgs e)
        {
            _weekTab = true;
            _page = 0;
            UpdateTable();
        }

        //Abre a página onde está o primeiro canal do jogador
        private void btnGoToMe_Click(object sender, RoutedEventArgs e)
        {
            int index = GetList().FindIndex(c => c.IsPlayer);
            if (index < 0)
                return;
            _page = index / PageSize;
            UpdateTable();
            rankingView.UpdateLayout();
            ((FrameworkElement)rankingPanel.Children[index % PageSize]).BringIntoView();
        }

        private void btnGoToTop_Click(object sender, RoutedEventArgs e)
        {
            _page = 0;
            UpdateTable();
            rankingView.ScrollToHome();
        }

        private void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            _page--;
            UpdateTable();
            rankingView.ScrollToHome();
        }

        private void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            _page++;
            UpdateTable();
            rankingView.ScrollToHome();
        }
    }
}
