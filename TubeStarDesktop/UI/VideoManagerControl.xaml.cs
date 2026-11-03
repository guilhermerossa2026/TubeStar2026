using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TubeStar
{
    /// <summary>
    /// Tela "Meus videos": chips de canal, filtros, busca, grade paginada e painel do canal.
    /// </summary>
    public partial class VideoManagerControl : UserControl
    {
        private const int PageSize = 9;

        private enum VideoFilter { Recent, MostViewed, BestScore }

        //_currentChannel nulo + _overview = Visão geral (todos os canais)
        private Channel _currentChannel;
        private bool _overview;
        private VideoFilter _filter = VideoFilter.Recent;
        private int _page;
        private bool _showAll;

        public event Action<Video> PostRequested;

        public VideoManagerControl()
        {
            InitializeComponent();
            Translate();
            _currentChannel = Channel.UnreleasedVideos;
        }

        private void Translate()
        {
            txtTitle.Text = EnglishStrings.NavMyVideos.Translate();
            txtLead.Text = EnglishStrings.VideosLead.Translate();
            btnAddChannel.Content = EnglishStrings.VideosNewChannel.Translate();
            btnFilterRecent.Content = EnglishStrings.VideosFilterRecent.Translate();
            btnFilterViews.Content = EnglishStrings.VideosFilterMostViewed.Translate();
            btnFilterScore.Content = EnglishStrings.VideosFilterBestScore.Translate();
            txtSearchPlaceholder.Text = EnglishStrings.VideosSearch.Translate();
            System.Windows.Automation.AutomationProperties.SetName(txtSearch, txtSearchPlaceholder.Text);
            btnPrev.Content = EnglishStrings.VideosPrevious.Translate();
            btnNext.Content = EnglishStrings.VideosNext.Translate();
            txtEmpty.Text = EnglishStrings.VideosNone.Translate();
            lblViews.Text = EnglishStrings.VideosTotalViews.Translate().ToUpper();
            lblIncome.Text = EnglishStrings.VideosTotalEarnings.Translate().ToUpper();
            lblSubs.Text = EnglishStrings.Subscribers.Translate().ToUpper();
            lblRpm.Text = EnglishStrings.ChannelRpm.Translate().ToUpper();
            lblRank.Text = EnglishStrings.ChannelRank.Translate().ToUpper();
            lblChart.Text = EnglishStrings.ChannelSubscribersChart.Translate().ToUpper();
            txtUnreleasedHint.Text = EnglishStrings.VideosUnreleasedHint.Translate();
            btnStats.Content = EnglishStrings.VideosStatistics.Translate();
            btnEdit.Content = EnglishStrings.VideosEditChannel.Translate();
            UpdateViewAllText();
        }

        private void AddChannel_Click(object sender, RoutedEventArgs e)
        {
            AddChannelDialog channelDialog = new AddChannelDialog();
            channelDialog.ShowDialog(() =>
                {
                    if (channelDialog.Channel != null)
                    {
                        Player.Current.Channels.Add(channelDialog.Channel);
                        Update();
                        //Canal novo vira o ativo (atualiza o seletor e o cabeçalho)
                        ActiveChannel.Select(channelDialog.Channel);
                    }
                });
        }

        /// <summary>
        /// Redesenha a tela (fim do dia, troca de tela...). Mantem canal, filtro, busca e pagina se ainda validos.
        /// </summary>
        public void Update()
        {
            //Update unreleased videos channel...
            Channel.UnreleasedVideos.Videos.Clear();
            foreach (var video in Player.Current.Videos)
            {
                if (video.ReadyForRelease())
                {
                    Channel.UnreleasedVideos.Videos.Add(video);
                }
            }

            //O canal atual pode ter sumido (novo jogo / jogo carregado)
            if (!_overview && (_currentChannel == null || (_currentChannel != Channel.UnreleasedVideos && !Player.Current.Channels.Contains(_currentChannel))))
            {
                SelectChannel(Channel.UnreleasedVideos);
                return;
            }

            RenderChannelChips();
            ShowChannel();
        }

        //Seletor do menu lateral mudou: mostra o mesmo canal (ou a Visão geral)
        public void ShowActiveChannel()
        {
            var active = ActiveChannel.Current;
            if (active == null ? !_overview : active != _currentChannel)
                SelectChannel(active, true);
        }

        //Canal dono do vídeo (na Visão geral a grade mistura canais)
        private Channel ChannelOf(Video video)
        {
            if (!_overview)
                return _currentChannel;
            return Player.Current.Channels.FirstOrDefault(c => c != Channel.UnreleasedVideos && c.Videos.Contains(video));
        }

        private void SelectChannel(Channel channel, bool overview = false)
        {
            _overview = overview && channel == null;
            if (channel == null && !_overview)
                channel = Channel.UnreleasedVideos;
            _currentChannel = channel;
            _page = 0;
            _filter = VideoFilter.Recent;
            _showAll = false;
            txtSearch.Text = String.Empty;
            UpdateFilterChips();
            UpdateViewAllText();
            RenderChannelChips();
            ShowChannel();
        }

        private IEnumerable<Channel> OrderedChannels()
        {
            var channels = Player.Current.Channels.Where(c => c != Channel.UnreleasedVideos).ToList();
            return new[] { Channel.UnreleasedVideos }
                .Concat(channels.Where(c => !c.IsSuspended))
                .Concat(channels.Where(c => c.IsSuspended));
        }

        private void RenderChannelChips()
        {
            channelChips.Children.Clear();

            //Visão geral (só faz sentido com canal criado)
            if (Player.Current.Channels.Any(c => c != Channel.UnreleasedVideos))
            {
                var overviewChip = new Button { Content = EnglishStrings.ChannelOverview.Translate() };
                overviewChip.Style = (Style)FindResource(_overview ? "ChipActiveButtonStyle" : "ChipButtonStyle");
                overviewChip.Click += (s, e) =>
                {
                    if (!_overview)
                    {
                        SelectChannel(null, true);
                        ActiveChannel.Select(null);
                    }
                };
                channelChips.Children.Add(overviewChip);
            }

            foreach (var channel in OrderedChannels())
            {
                var chip = new Button();
                bool active = !_overview && channel == _currentChannel;
                chip.Style = (Style)FindResource(active ? "ChipActiveButtonStyle" : "ChipButtonStyle");

                if (channel == Channel.UnreleasedVideos)
                {
                    chip.Content = String.Format("{0} · {1}", EnglishStrings.VideosUnreleasedChip.Translate(), channel.Videos.Count);
                }
                else if (channel.IsSuspended)
                {
                    chip.Content = String.Format("{0} · {1}", channel.Name, EnglishStrings.Suspended.Translate());
                    if (!active) chip.Opacity = 0.55;
                }
                else
                {
                    chip.Content = String.Format("{0} · {1}", channel.Name, channel.Videos.Count);
                }

                var target = channel;
                chip.Click += (s, e) =>
                {
                    if (_overview || target != _currentChannel)
                    {
                        SelectChannel(target);
                        //Chip de canal também troca o seletor do menu lateral ("Não lançados" não é canal)
                        if (target != Channel.UnreleasedVideos)
                            ActiveChannel.Select(target);
                    }
                };
                channelChips.Children.Add(chip);
            }
        }

        private List<Video> FilteredVideos()
        {
            var channel = _currentChannel;
            //Ordem de insercao = mais antigo primeiro; "mais recentes" inverte
            IEnumerable<Video> videos = _overview
                ? Player.Current.Channels.Where(c => c != Channel.UnreleasedVideos).SelectMany(c => c.Videos.Reverse())
                : channel.Videos.Reverse();

            string search = (txtSearch.Text ?? String.Empty).Trim();
            if (search.Length > 0)
            {
                videos = videos.Where(v => v.Name != null && v.Name.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0);
            }

            var list = videos.ToList();
            //Visão geral: "mais recentes" pelos dias no ar (cada canal vem em blocos)
            if (_overview && _filter == VideoFilter.Recent)
                list = list.OrderBy(v => v.Iterations).ToList();
            if (_filter == VideoFilter.MostViewed)
            {
                list = list.OrderByDescending(v => v.Views).ToList();
            }
            else if (_filter == VideoFilter.BestScore)
            {
                bool canView = channel != Channel.UnreleasedVideos || Player.Current.CanViewQualityBeforeUpload;
                if (canView)
                    list = list.OrderByDescending(v => v.GenerateQuality()).ToList();
            }
            return list;
        }

        private void ShowChannel()
        {
            var channel = _currentChannel;

            if (channel != null && channel.Videos.Count >= 100)
                TrophyManager.UnlockTrophy(Trophy.OCD);

            UpdatePanel(channel);

            var list = FilteredVideos();
            int pages = Math.Max(1, (int)Math.Ceiling(list.Count / (double)PageSize));
            if (_page >= pages) _page = pages - 1;
            if (_page < 0) _page = 0;

            IEnumerable<Video> visible = _showAll ? list : list.Skip(_page * PageSize).Take(PageSize);

            videoPanelGrid.Children.Clear();
            foreach (var video in visible)
            {
                var videoBlock = new VideoBlockGrid(ChannelOf(video), video);
                videoBlock.UploadClick += (s, e) =>
                {
                    UploadVideo(videoBlock.Video);
                };
                videoBlock.DeleteClick += (s, e) =>
                {
                    DeleteVideo(videoBlock.Video);
                };
                videoBlock.LawyerClick += (s, e) =>
                {
                    HandleLawyerClick(videoBlock.Video);
                };
                videoPanelGrid.Children.Add(videoBlock);
            }

            txtEmpty.Visibility = list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            pagerPanel.Visibility = (!_showAll && pages > 1) ? Visibility.Visible : Visibility.Collapsed;
            txtPage.Text = String.Format(EnglishStrings.VideosPageOf.Translate(), _page + 1, pages);
            btnPrev.IsEnabled = _page > 0;
            btnNext.IsEnabled = _page < pages - 1;
        }

        private void UpdatePanel(Channel channel)
        {
            bool unreleased = !_overview && channel == Channel.UnreleasedVideos;
            bool single = !_overview && !unreleased;

            if (_overview)
                txtPanelTitle.Text = EnglishStrings.ChannelOverview.Translate();
            else
                txtPanelTitle.Text = String.Format(EnglishStrings.VideosPanel.Translate(),
                    unreleased ? EnglishStrings.VideosUnreleasedChip.Translate() : channel.Name);

            txtPanelNiche.Visibility = single && channel.Niche.HasValue ? Visibility.Visible : Visibility.Collapsed;
            if (single && channel.Niche.HasValue)
                txtPanelNiche.Text = channel.Niche.Value.GetString() + " · " + AddChannelDialog.GetNicheInfo(channel.Niche.Value);
            txtPanelNiche.TextWrapping = TextWrapping.Wrap;

            txtPanelStatus.Visibility = (single && channel.IsSuspended) ? Visibility.Visible : Visibility.Collapsed;
            txtPanelStatus.Text = EnglishStrings.Suspended.Translate().ToUpper();

            panelKpis.Visibility = single ? Visibility.Visible : Visibility.Collapsed;
            panelChart.Visibility = single ? Visibility.Visible : Visibility.Collapsed;
            panelChannelButtons.Visibility = single ? Visibility.Visible : Visibility.Collapsed;
            panelOverview.Visibility = _overview ? Visibility.Visible : Visibility.Collapsed;
            txtUnreleasedHint.Visibility = unreleased ? Visibility.Visible : Visibility.Collapsed;

            lblVideos.Text = (unreleased ? EnglishStrings.VideosReady.Translate() : EnglishStrings.Videos.Translate()).ToUpper();
            txtKpiVideos.Text = _overview
                ? Player.Current.Channels.Where(c => c != Channel.UnreleasedVideos).Sum(c => c.Videos.Count).ToNumberString()
                : channel.Videos.Count.ToNumberString();

            if (single)
            {
                long totalViews = channel.Videos.Sum(v => (long)v.Views);
                txtKpiViews.Text = totalViews.ToShortNumber();
                txtKpiIncome.Text = channel.Income.ToShortCurrencyString();
                txtKpiIncome.ToolTip = channel.Income.ToCurrencyString();
                txtKpiSubs.Text = channel.Subscribers.ToShortNumber();
                txtKpiRpm.Text = ActiveChannel.GetRpm(channel).ToShortCurrencyString();
                txtKpiRpm.ToolTip = ActiveChannel.GetRpm(channel).ToCurrencyString();
                Ranking.Current.Refresh(false);
                var entry = Ranking.Current.Channels.FirstOrDefault(c => c.IsPlayer && c.PlayerChannelId == channel.RankId);
                txtKpiRank.Text = entry != null && entry.Position > 0 ? "#" + entry.Position : "-";
                DrawChart();
            }

            if (_overview)
                BuildOverview();
        }

        //2B: uma linha por canal (inscritos, ganhos, RPM) e o total
        private void BuildOverview()
        {
            panelOverview.Children.Clear();
            var channels = Player.Current.Channels.Where(c => c != Channel.UnreleasedVideos).ToList();
            foreach (var channel in channels)
                panelOverview.Children.Add(CreateOverviewRow(channel.Name, channel.Niche.HasValue ? channel.Niche.Value.GetString() : "",
                    channel.Subscribers, channel.Income, ActiveChannel.GetRpm(channel).ToCurrencyString(), false));

            panelOverview.Children.Add(CreateOverviewRow(EnglishStrings.ChannelTotal.Translate(), "",
                channels.Sum(c => (long)c.Subscribers), channels.Sum(c => c.Income), null, true));
        }

        private UIElement CreateOverviewRow(string name, string niche, long subscribers, double income, string rpm, bool total)
        {
            var grid = new Grid { Margin = new Thickness(0, 6, 0, 0) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var left = new StackPanel();
            left.Children.Add(new TextBlock { Text = name, FontWeight = total ? FontWeights.Bold : FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis });
            string info = String.IsNullOrEmpty(niche) ? "" : niche;
            if (rpm != null)
                info += (info.Length > 0 ? " · " : "") + EnglishStrings.ChannelRpm.Translate() + " " + rpm;
            if (info.Length > 0)
                left.Children.Add(new TextBlock { Text = info, Style = (Style)FindResource("MutedTextStyle"), FontSize = 11, TextTrimming = TextTrimming.CharacterEllipsis });
            grid.Children.Add(left);

            var right = new StackPanel { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(8, 0, 0, 0) };
            right.Children.Add(new TextBlock { Text = subscribers.ToShortNumber(), HorizontalAlignment = HorizontalAlignment.Right, FontWeight = total ? FontWeights.Bold : FontWeights.Normal });
            right.Children.Add(new TextBlock { Text = income.ToCurrencyString(), HorizontalAlignment = HorizontalAlignment.Right, FontSize = 11, Foreground = (Brush)FindResource("YtGainBrush") });
            Grid.SetColumn(right, 1);
            grid.Children.Add(right);

            return new Border { BorderBrush = (Brush)FindResource("YtDarkBorderBrush"), BorderThickness = new Thickness(0, total ? 1 : 0, 0, 0), Padding = new Thickness(0, total ? 6 : 0, 0, 0), Child = grid };
        }

        //2A: inscritos dia a dia (histórico que o canal já guarda)
        private void DrawChart()
        {
            chartCanvas.Children.Clear();
            var channel = _currentChannel;
            if (channel == null || _overview || channel == Channel.UnreleasedVideos)
                return;

            var data = channel.SubscribersOverTime;
            double width = chartCanvas.ActualWidth, height = chartCanvas.Height;
            if (data == null || data.Count < 2 || width <= 0)
            {
                chartCanvas.Children.Add(new TextBlock { Text = EnglishStrings.ChannelChartEmpty.Translate(), Style = (Style)FindResource("MutedTextStyle"), FontSize = 11 });
                return;
            }

            double max = Math.Max(1, data.Max()), min = data.Min();
            double range = Math.Max(1, max - min);
            var line = new Polyline { Stroke = (Brush)FindResource("YtRedBrush"), StrokeThickness = 2 };
            for (int i = 0; i < data.Count; i++)
            {
                double x = width * i / (data.Count - 1);
                double y = height - 4 - (height - 8) * (data[i] - min) / range;
                line.Points.Add(new Point(x, y));
            }
            chartCanvas.Children.Add(new Line { X1 = 0, X2 = width, Y1 = height - 1, Y2 = height - 1, Stroke = (Brush)FindResource("YtDarkBorderBrush"), StrokeThickness = 1 });
            chartCanvas.Children.Add(line);
        }

        private void chartCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawChart();
        }


        private void UpdateFilterChips()
        {
            btnFilterRecent.Style = (Style)FindResource(_filter == VideoFilter.Recent ? "ChipActiveButtonStyle" : "ChipButtonStyle");
            btnFilterViews.Style = (Style)FindResource(_filter == VideoFilter.MostViewed ? "ChipActiveButtonStyle" : "ChipButtonStyle");
            btnFilterScore.Style = (Style)FindResource(_filter == VideoFilter.BestScore ? "ChipActiveButtonStyle" : "ChipButtonStyle");
        }

        private void UpdateViewAllText()
        {
            btnViewAll.Content = _showAll ? EnglishStrings.VideosViewPages.Translate() : EnglishStrings.VideosViewAll.Translate();
            btnViewAll.Style = (Style)FindResource(_showAll ? "ChipActiveButtonStyle" : "ChipButtonStyle");
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnFilterViews) _filter = VideoFilter.MostViewed;
            else if (sender == btnFilterScore) _filter = VideoFilter.BestScore;
            else _filter = VideoFilter.Recent;
            _page = 0;
            UpdateFilterChips();
            ShowChannel();
            gridViewer.ScrollToTop();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtSearchPlaceholder.Visibility = String.IsNullOrEmpty(txtSearch.Text) ? Visibility.Visible : Visibility.Collapsed;
            if ((_currentChannel == null && !_overview) || Player.Current == null) return;
            _page = 0;
            ShowChannel();
        }

        private void btnViewAll_Click(object sender, RoutedEventArgs e)
        {
            _showAll = !_showAll;
            _page = 0;
            UpdateViewAllText();
            ShowChannel();
            gridViewer.ScrollToTop();
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            _page--;
            ShowChannel();
            gridViewer.ScrollToTop();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            _page++;
            ShowChannel();
            gridViewer.ScrollToTop();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var channel = _currentChannel;
            if (channel == null || channel == Channel.UnreleasedVideos) return;

            AddChannelDialog channelDialog = new AddChannelDialog(channel);
            channelDialog.ShowDialog(() =>
            {
                if (channelDialog.DialogResult == true)
                {
                    channel.Name = channelDialog.Channel.Name;
                    channel.Advertising = channelDialog.Channel.Advertising;
                    Update();
                }
            });
        }

        private void HandleLawyerClick(Video video)
        {
            var owner = ChannelOf(video);
            if (owner != null && owner.IsSuspended)
            {
                CustomMessageBox.ShowDialog(EnglishStrings.LawyerStoreItem.Translate(), String.Format(EnglishStrings.RemoveChannelSuspension.Translate(), StoreItems.Current.Lawyer.AdditionalCost.ToCurrencyString()), MessagePicture.Legal, (result) =>
                {
                    if (result == true)
                    {
                        owner.IsSuspended = false;
                        foreach (var v in owner.Videos)
                        {
                            v.IsSuspended = false;
                        }
                        Player.Current.Money -= StoreItems.Current.Lawyer.AdditionalCost;
                        Update();
                    }
                });
            }
            else if (video.IsSuspended)
            {
                CustomMessageBox.ShowDialog(EnglishStrings.LawyerStoreItem.Translate(), String.Format(EnglishStrings.RemoveVideoSuspension.Translate(), StoreItems.Current.Lawyer.AdditionalCost.ToCurrencyString()), MessagePicture.Legal, (result) =>
                {
                    if (result == true)
                    {
                        video.IsSuspended = false;
                        Player.Current.Money -= StoreItems.Current.Lawyer.AdditionalCost;
                        Update();
                    }
                });
            }
        }

        //"Postar" de um não lançado: a MainPage abre a tela Postar vídeo com ele
        private void UploadVideo(Video video)
        {
            if (PostRequested != null)
                PostRequested(video);
        }

        private void btnStats_Click(object sender, RoutedEventArgs e)
        {
            ChannelStatsDialog dialog = new ChannelStatsDialog(_currentChannel);
            dialog.ShowDialog();
        }

        private void DeleteVideo(Video video)
        {
            if (video.Attributes.Contains(VideoAttributes.Cats))
                TrophyManager.UnlockTrophy(Trophy.CatInBin);

            if (_currentChannel == Channel.UnreleasedVideos)
            {
                Player.Current.Videos.Remove(video);
            }
            else
            {
                var owner = ChannelOf(video);
                if (owner != null)
                    owner.Videos.Remove(video);
            }

            Update();
        }
    }
}
