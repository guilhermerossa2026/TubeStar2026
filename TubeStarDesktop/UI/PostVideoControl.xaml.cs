using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TubeStar
{
    /// <summary>
    /// Tela "Postar vídeo" (antes UploadVideoDialog): lista dos prontos, canal e compra de views.
    /// </summary>
    public partial class PostVideoControl : UserControl
    {
        //Vídeo postado: a MainPage atualiza cabeçalho/Meus vídeos
        public event Action VideoPosted;
        public event Action BackRequested;

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (BackRequested != null)
                BackRequested();
        }
        //Botão "Criar vídeo" do estado vazio
        public event Action CreateVideoRequested;

        private const int ThumbPageSize = 50;

        private Video _selected;
        private Channel _channel;
        private int _viewsOption;

        //Miniaturas: só busca quando o jogador clica em "Trocar miniatura"
        private List<string> _thumbIds = new List<string>();
        private int _thumbIndex;
        private int _searchGeneration;

        public PostVideoControl()
        {
            InitializeComponent();
            Translate();
        }

        private void Translate()
        {
            txtTitle.Text = EnglishStrings.NavPostVideo.Translate();
            btnBack.Content = "‹ " + EnglishStrings.NavMyVideos.Translate();
            txtLead.Text = EnglishStrings.PostVideoLead.Translate();
            txtEmpty.Text = EnglishStrings.PostNoVideos.Translate();
            btnGoCreate.Content = EnglishStrings.NavCreateVideo.Translate();
            lblReady.Text = EnglishStrings.PostReadyVideos.Translate();
            btnChangeThumb.Content = EnglishStrings.PostChangeThumb.Translate();
            lblChannel.Text = EnglishStrings.Channel.Translate();
            txtNoChannel.Text = EnglishStrings.PostNoChannel.Translate();
            btnAddChannel.Content = EnglishStrings.VideosNewChannel.Translate();
            lblPromote.Text = EnglishStrings.PostPromote.Translate();
            txtPost.Text = EnglishStrings.NavPostVideo.Translate();
        }

        private static List<Video> ReadyVideos()
        {
            return Player.Current.Videos.Where(v => v.ReadyForRelease()).ToList();
        }

        /// <summary>Redesenha a tela; se vier um vídeo, ele fica selecionado.</summary>
        public void Update(Video select = null)
        {
            var ready = ReadyVideos();
            if (select != null && ready.Contains(select))
                SelectVideo(select);
            else if (_selected == null || !ready.Contains(_selected))
                SelectVideo(ready.FirstOrDefault());
            else
                SelectVideo(_selected);
        }

        private void SelectVideo(Video video)
        {
            if (_selected != video)
            {
                if (_selected != null)
                    _selected.OnImageFetched -= ShowVideoImage;
                _selected = video;
                if (_selected != null)
                    _selected.OnImageFetched += ShowVideoImage;
                _viewsOption = 0;
                ResetThumbs();
            }

            var ready = ReadyVideos();
            bool empty = ready.Count == 0;
            emptyPanel.Visibility = empty ? Visibility.Visible : Visibility.Collapsed;
            mainGrid.Visibility = empty ? Visibility.Collapsed : Visibility.Visible;

            RenderList(ready);
            if (_selected == null)
                return;

            ShowVideoImage();
            txtVideoName.Text = _selected.Name;
            string info = _selected.Category.GetString();
            if (_selected.Type.HasValue)
                info += " · " + _selected.GetTypeString();
            if (Player.Current.CanViewQualityBeforeUpload)
                info += " · " + String.Format(EnglishStrings.VideosScore.Translate(), (_selected.GenerateQuality() / 10.0).ToString("0.0"));
            txtVideoInfo.Text = info;

            RenderChannels();
            RenderViewOptions();
            UpdateSummary();
        }

        private void RenderList(List<Video> ready)
        {
            videoList.Children.Clear();
            foreach (var video in ready)
            {
                var row = new Button();
                row.Style = (Style)FindResource(video == _selected ? "RowActiveButtonStyle" : "RowButtonStyle");
                System.Windows.Automation.AutomationProperties.SetName(row, video.Name);

                var dock = new DockPanel();
                var thumbBorder = new Border();
                thumbBorder.Width = 64;
                thumbBorder.Height = 36;
                thumbBorder.CornerRadius = new CornerRadius(4);
                thumbBorder.ClipToBounds = true;
                thumbBorder.Background = (Brush)FindResource("YtDarkSurfaceBrush");
                thumbBorder.Margin = new Thickness(0, 0, 10, 0);
                var img = new Image();
                img.Stretch = Stretch.UniformToFill;
                img.Source = video.ImageBytes != null ? video.GetThumbnail() : null;
                thumbBorder.Child = img;
                dock.Children.Add(thumbBorder);

                var texts = new StackPanel();
                texts.VerticalAlignment = VerticalAlignment.Center;
                var name = new TextBlock();
                name.Text = video.Name;
                name.FontWeight = FontWeights.SemiBold;
                name.TextTrimming = TextTrimming.CharacterEllipsis;
                texts.Children.Add(name);
                var sub = new TextBlock();
                sub.Style = (Style)FindResource("MutedTextStyle");
                sub.TextTrimming = TextTrimming.CharacterEllipsis;
                sub.Text = video.Category.GetString();
                if (video.Type.HasValue)
                    sub.Text += " · " + video.GetTypeString();
                if (Player.Current.CanViewQualityBeforeUpload)
                    sub.Text += " · " + String.Format(EnglishStrings.VideosScore.Translate(), (video.GenerateQuality() / 10.0).ToString("0.0"));
                texts.Children.Add(sub);
                dock.Children.Add(texts);

                row.Content = dock;
                var target = video;
                row.Click += (s, e) => { if (target != _selected) SelectVideo(target); };
                videoList.Children.Add(row);
            }
        }

        private void RenderChannels()
        {
            var channels = Player.Current.Channels.Where(c => c != Channel.UnreleasedVideos && !c.IsSuspended).ToList();

            if (_channel == null || !channels.Contains(_channel))
            {
                _channel = null;
                if (channels.Count == 1)
                    _channel = channels[0];
                else if (Settings.LastChannel != null && channels.Contains(Settings.LastChannel))
                    _channel = Settings.LastChannel;
            }

            noChannelPanel.Visibility = channels.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            channelPanel.Children.Clear();
            foreach (var channel in channels)
            {
                var chip = new Button();
                chip.Content = channel.Niche.HasValue ? channel.Name + " · " + channel.Niche.Value.GetString() : channel.Name;
                chip.Style = (Style)FindResource(channel == _channel ? "ChipActiveButtonStyle" : "ChipButtonStyle");
                var target = channel;
                chip.Click += (s, e) =>
                {
                    _channel = target;
                    RenderChannels();
                    UpdateSummary();
                };
                channelPanel.Children.Add(chip);
            }
        }

        //Campanhas de "Promover vídeo" (item 5d): nome, preço e views estimadas
        private string ViewOptionText(int option)
        {
            if (option <= 0)
                return EnglishStrings.None.Translate();
            return String.Format("{0} · ${1:N0} · ~{2} {3}", Promotion.GetName(option), Promotion.Costs[option],
                ((long)Promotion.Views[option]).ToShortNumber(), EnglishStrings.Views.Translate().ToLower());
        }

        //"≈ 8 mil views em 3 dias · 40 a 90 inscritos"
        private string PromotionEstimate()
        {
            if (_viewsOption <= 0 || _selected == null)
                return EnglishStrings.PromoteNoneInfo.Translate();
            int views = Promotion.Views[_viewsOption];
            string text = String.Format(EnglishStrings.PromoteEstimateViews.Translate(), ((long)views).ToShortNumber(), Promotion.Days);
            if (_channel != null)
            {
                int subs = VideoViewer.EstimateSubscribers(_channel, _selected, views);
                text += " · " + String.Format(EnglishStrings.PromoteEstimateSubs.Translate(),
                    ((long)(subs * 0.7)).ToShortNumber(), ((long)Math.Ceiling(subs * 1.3)).ToShortNumber());
            }
            return text + "\n" + EnglishStrings.PromoteNoAds.Translate();
        }

        private void RenderViewOptions()
        {
            viewsPanel.Children.Clear();
            for (int i = 0; i < Promotion.Costs.Length; i++)
            {
                int option = i;
                var chip = new Button();
                chip.Content = ViewOptionText(i);
                chip.Style = (Style)FindResource(i == _viewsOption ? "ChipActiveButtonStyle" : "ChipButtonStyle");
                chip.Click += (s, e) =>
                {
                    _viewsOption = option;
                    RenderViewOptions();
                    UpdateSummary();
                };
                viewsPanel.Children.Add(chip);
            }
        }

        private void UpdateSummary()
        {
            if (_selected == null)
                return;
            txtSummary.Text = String.Format("{0}: {1}\n{2}: {3}",
                EnglishStrings.Channel.Translate(), _channel != null ? _channel.Name : "-",
                EnglishStrings.PostPromote.Translate(), ViewOptionText(_viewsOption));
            txtPromoteEstimate.Text = PromotionEstimate();

            //Aviso de vídeo fora do nicho do canal
            bool outOfNiche = _channel != null && !_selected.IsInNiche(_channel);
            txtNicheWarning.Visibility = outOfNiche ? Visibility.Visible : Visibility.Collapsed;
            if (outOfNiche)
                txtNicheWarning.Text = "⚠ " + String.Format(EnglishStrings.NicheOutWarning.Translate(), _channel.Niche.Value.GetString());
        }

        #region Miniatura

        private void ShowVideoImage()
        {
            if (_selected == null)
                return;
            if (_selected.ImageBytes != null)
            {
                imgThumb.Source = _selected.GetThumbnail();
                txtThumbStatus.Text = "";
            }
            else
            {
                imgThumb.Source = null;
            }
            //A miniatura da lista também muda
            if (IsVisible)
                RenderList(ReadyVideos());
        }

        private void ResetThumbs()
        {
            _searchGeneration++;
            _thumbIds.Clear();
            _thumbIndex = 0;
            txtCounter.Text = "";
            txtThumbStatus.Text = "";
            btnChangeThumb.IsEnabled = true;
            UpdateArrows();
        }

        private void UpdateArrows()
        {
            btnPrev.Visibility = _thumbIndex > 0 ? Visibility.Visible : Visibility.Hidden;
            btnNext.Visibility = _thumbIndex < _thumbIds.Count - 1 ? Visibility.Visible : Visibility.Hidden;
            txtCounter.Text = _thumbIds.Count > 0 ? String.Format(EnglishStrings.ThumbCounter.Translate(), _thumbIndex + 1, _thumbIds.Count) : "";
        }

        //Uma busca pelo título (cai na categoria se não achar), igual ao diálogo antigo
        private void btnChangeThumb_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null)
                return;
            if (!YouTubeAPI.HasApiKey)
            {
                txtCounter.Text = EnglishStrings.ThumbOffline.Translate();
                return;
            }

            var video = _selected;
            _searchGeneration++;
            int generation = _searchGeneration;
            _thumbIds.Clear();
            if (!String.IsNullOrEmpty(video.YouTubeVideoId))
                _thumbIds.Add(video.YouTubeVideoId);
            _thumbIndex = 0;
            btnChangeThumb.IsEnabled = false;
            txtCounter.Text = EnglishStrings.ThumbSearching.Translate();

            Action onError = () =>
            {
                if (generation != _searchGeneration) return;
                txtCounter.Text = EnglishStrings.ThumbOffline.Translate();
                if (video.ImageBytes == null)
                    video.SetErrorImage();
            };

            Func<YouTubeSearchResponse, bool> add = (response) =>
            {
                if (response == null || response.Entries == null)
                    return false;
                int before = _thumbIds.Count;
                foreach (var entry in response.Entries)
                {
                    if (entry != null && entry.Id != null && !String.IsNullOrEmpty(entry.Id.VideoId) && !_thumbIds.Contains(entry.Id.VideoId))
                        _thumbIds.Add(entry.Id.VideoId);
                }
                return _thumbIds.Count > before;
            };

            Action showFirstNew = () =>
            {
                //Mostra já a próxima opção (a atual fica na posição 0, se houver)
                _thumbIndex = _thumbIds.Count > 1 && !String.IsNullOrEmpty(video.YouTubeVideoId) ? 1 : 0;
                video.SetImageFromId(_thumbIds[_thumbIndex]);
                UpdateArrows();
            };

            WebClientHelpers.Download<YouTubeSearchResponse>(YouTubeAPI.SearchVideos(video.Name, ThumbPageSize, null), (response) =>
            {
                if (generation != _searchGeneration) return;
                if (add(response))
                {
                    showFirstNew();
                    return;
                }
                WebClientHelpers.Download<YouTubeSearchResponse>(YouTubeAPI.SearchVideos(video.Category.GetString(), ThumbPageSize, null), (r) =>
                {
                    if (generation != _searchGeneration) return;
                    if (add(r))
                        showFirstNew();
                    else
                        txtCounter.Text = EnglishStrings.ThumbNoResults.Translate();
                }, onError);
            }, onError);
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null || _thumbIndex <= 0) return;
            _thumbIndex--;
            _selected.SetImageFromId(_thumbIds[_thumbIndex]);
            UpdateArrows();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null || _thumbIndex >= _thumbIds.Count - 1) return;
            _thumbIndex++;
            _selected.SetImageFromId(_thumbIds[_thumbIndex]);
            UpdateArrows();
        }

        #endregion

        private void btnGoCreate_Click(object sender, RoutedEventArgs e)
        {
            if (CreateVideoRequested != null)
                CreateVideoRequested();
        }

        //Mesma lógica do "+ Novo canal" de Meus vídeos
        private void btnAddChannel_Click(object sender, RoutedEventArgs e)
        {
            AddChannelDialog channelDialog = new AddChannelDialog();
            channelDialog.ShowDialog(() =>
            {
                if (channelDialog.Channel != null)
                {
                    Player.Current.Channels.Add(channelDialog.Channel);
                    _channel = channelDialog.Channel;
                    Update();
                }
            });
        }

        private bool HasMoney(double amount)
        {
            if (Player.Current.Money - amount < 0)
            {
                CustomMessageBox.ShowDialog(EnglishStrings.LowCashHeader.Translate(), EnglishStrings.LowCashMessage.Translate(), MessagePicture.Money);
                return false;
            }
            return true;
        }

        //Mesma lógica de UploadVideoDialog.OKButton_Click + VideoManagerControl.UploadVideo
        private void btnPost_Click(object sender, RoutedEventArgs e)
        {
            var video = _selected;
            if (video == null)
                return;

            if (Player.Current.Channels.Count == 1)
            {
                CustomMessageBox.ShowDialog(EnglishStrings.WhereTo.Translate(), EnglishStrings.ChannelNeeded.Translate(), MessagePicture.Puzzle);
                return;
            }

            if (_channel == null)
            {
                CustomMessageBox.ShowDialog(EnglishStrings.MissingValueHeader.Translate(), EnglishStrings.MissingChannel.Translate(), MessagePicture.Puzzle);
                return;
            }

            //Promover vídeo: cobra no dia da postagem (OnceOffCost) e entrega as views em Promotion.Days dias
            if (_viewsOption > 0)
            {
                if (!HasMoney(Promotion.Costs[_viewsOption])) return;
                Promotion.Start(video, _viewsOption);
            }

            var channel = Settings.LastChannel = _channel;

            //Subscriber views
            VideoViewer.SubscriberView(channel, video);

            video.GenerateQuality();

            //Consultant fee
            if (StoreItems.Current.Consultant.Purchased)
            {
                video.OnceOffCost += StoreItems.Current.Consultant.AdditionalCost;
            }

            //Special attribute - CopyCat
            if (video.Attributes.Contains(VideoAttributes.CopyCat))
            {
                VideoViewer.FreeViews(video, VideoAttributes.CopyCat.FreeViews);
            }

            //Special attribute - FanBoyBait
            if (video.Quality >= 75 && video.Attributes.Contains(VideoAttributes.FanBoyBait))
            {
                channel.MinimumSubsribers = channel.Subscribers;
                TrophyManager.UnlockTrophy(Trophy.HookLineAndSinker);
            }

            //Trophy - PoopStar
            if (video.Quality <= 1)
            {
                TrophyManager.UnlockTrophy(Trophy.PoopStar);
            }

            channel.Videos.Add(video);
            Player.Current.Videos.Remove(video);

            //Próximo pronto (ou o vazio)
            video.OnImageFetched -= ShowVideoImage;
            _selected = null;
            Update();

            if (VideoPosted != null)
                VideoPosted();
        }
    }
}
