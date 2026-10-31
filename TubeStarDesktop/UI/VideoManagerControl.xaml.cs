using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TubeStar
{
    /// <summary>
    /// Tela "Meus videos": chips de canal, filtros, busca, grade paginada e painel do canal.
    /// </summary>
    public partial class VideoManagerControl : UserControl
    {
        private const int PageSize = 9;

        private enum VideoFilter { Recent, MostViewed, BestScore }

        private Channel _currentChannel;
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
            if (_currentChannel == null || (_currentChannel != Channel.UnreleasedVideos && !Player.Current.Channels.Contains(_currentChannel)))
            {
                SelectChannel(Channel.UnreleasedVideos);
                return;
            }

            RenderChannelChips();
            ShowChannel();
        }

        private void SelectChannel(Channel channel)
        {
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
            foreach (var channel in OrderedChannels())
            {
                var chip = new Button();
                bool active = channel == _currentChannel;
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
                    if (target != _currentChannel)
                        SelectChannel(target);
                };
                channelChips.Children.Add(chip);
            }
        }

        private List<Video> FilteredVideos()
        {
            var channel = _currentChannel;
            //Ordem de insercao = mais antigo primeiro; "mais recentes" inverte
            IEnumerable<Video> videos = channel.Videos.Reverse();

            string search = (txtSearch.Text ?? String.Empty).Trim();
            if (search.Length > 0)
            {
                videos = videos.Where(v => v.Name != null && v.Name.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0);
            }

            var list = videos.ToList();
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

            if (channel.Videos.Count >= 100)
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
                var videoBlock = new VideoBlockGrid(channel, video);
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
            bool unreleased = channel == Channel.UnreleasedVideos;

            txtPanelTitle.Text = String.Format(EnglishStrings.VideosPanel.Translate(),
                unreleased ? EnglishStrings.VideosUnreleasedChip.Translate() : channel.Name);

            txtPanelStatus.Visibility = (!unreleased && channel.IsSuspended) ? Visibility.Visible : Visibility.Collapsed;
            txtPanelStatus.Text = EnglishStrings.Suspended.Translate().ToUpper();

            panelKpis.Visibility = unreleased ? Visibility.Collapsed : Visibility.Visible;
            panelChannelButtons.Visibility = unreleased ? Visibility.Collapsed : Visibility.Visible;
            txtUnreleasedHint.Visibility = unreleased ? Visibility.Visible : Visibility.Collapsed;

            lblVideos.Text = (unreleased ? EnglishStrings.VideosReady.Translate() : EnglishStrings.Videos.Translate()).ToUpper();
            txtKpiVideos.Text = channel.Videos.Count.ToNumberString();

            if (!unreleased)
            {
                long totalViews = channel.Videos.Sum(v => (long)v.Views);
                txtKpiViews.Text = ((int)Math.Min(totalViews, int.MaxValue)).ToNumberString();
                txtKpiIncome.Text = channel.Income.ToCurrencyString();
                txtKpiSubs.Text = channel.Subscribers.ToNumberString();
            }
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
            if (_currentChannel == null || Player.Current == null) return;
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
            if (_currentChannel.IsSuspended)
            {
                CustomMessageBox.ShowDialog(EnglishStrings.LawyerStoreItem.Translate(), String.Format(EnglishStrings.RemoveChannelSuspension.Translate(), StoreItems.Current.Lawyer.AdditionalCost.ToCurrencyString()), MessagePicture.Legal, (result) =>
                {
                    if (result == true)
                    {
                        _currentChannel.IsSuspended = false;
                        foreach (var v in _currentChannel.Videos)
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
            if (video.Category == VideoCategory.Cats)
                TrophyManager.UnlockTrophy(Trophy.CatInBin);

            if (_currentChannel == Channel.UnreleasedVideos)
            {
                Player.Current.Videos.Remove(video);
            }
            else
            {
                _currentChannel.Videos.Remove(video);
            }

            Update();
        }
    }
}
