using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TubeStar
{
    /// <summary>
    /// Cartao de video em grade (.card.vcard): miniatura, titulo, views, nota, comentarios e acoes.
    /// </summary>
    public partial class VideoBlockGrid : UserControl
    {
        public event EventHandler UploadClick;
        public event EventHandler LawyerClick;
        public event EventHandler DeleteClick;

        private Channel _channel;

        private Video _video;
        public Video Video
        {
            get { return _video; }
            private set
            {
                _video = value;
                if (_video != null)
                {
                    if (_channel == null)
                        throw new NotSupportedException("Channel cannot be null");

                    bool isUnreleasedChannel = (_channel == Channel.UnreleasedVideos);

                    txtName.Text = _video.Name;
                    txtName.ToolTip = _video.Name;
                    string views = GetNumberOfViews();
                    txtViewsLine.Text = String.Format(EnglishStrings.VideosViewsCount.Translate(), views);
                    txtLikes.Text = _video.Likes.ToShortNumber();
                    txtDislikes.Text = _video.Dislikes.ToShortNumber();
                    txtComments.Text = _video.GetCommentsCount().ToShortNumber();
                    txtDays.Text = String.Format(EnglishStrings.VideoDaysOnline.Translate(), _video.Iterations);

                    var released = isUnreleasedChannel ? Visibility.Collapsed : Visibility.Visible;
                    txtViewsLine.Visibility = panelStats.Visibility = released;

                    bool canViewQuality = !isUnreleasedChannel || Player.Current.CanViewQualityBeforeUpload;
                    scorePill.Visibility = canViewQuality ? Visibility.Visible : Visibility.Collapsed;
                    if (canViewQuality)
                        txtScore.Text = String.Format(EnglishStrings.VideosScore.Translate(), (_video.GenerateQuality() / 10.0).ToString("0.0"));

                    btnUpload.Visibility = isUnreleasedChannel ? Visibility.Visible : Visibility.Collapsed;
                    btnDelete.Visibility = _channel.IsRivalChannel ? Visibility.Collapsed : Visibility.Visible;

                    SuspendedAdorner.Visibility = _video.IsSuspended ? Visibility.Visible : Visibility.Collapsed;
                    txtSuspendedSub.Visibility = Visibility.Collapsed;
                    if (_video.IsSuspended && StoreItems.Current.Lawyer.Purchased)
                    {
                        txtSuspendedSub.Visibility = Visibility.Visible;
                    }

                    imgVideo.Source = _video.GetThumbnail();
                    UpdateLuckBadge();
                }
            }
        }

        //Selo de sorte no canto da miniatura: so videos do jogador, com pelo menos 1 dia no ar
        private void UpdateLuckBadge()
        {
            luckBadge.Visibility = Visibility.Collapsed;
            if (_video == null || _channel == null || _channel.IsRivalChannel || _channel == Channel.UnreleasedVideos || _video.Iterations < 1)
                return;

            string text;
            string back;
            string fore = "#FFFFFF";
            if (_video.LuckTier == 3) { text = "VIRAL"; back = "#E53935"; }
            else if (_video.LuckTier == 2) { text = "EM ALTA"; back = "#2E7D32"; }
            else if (_video.LuckTier == 1) { text = "FLOPOU"; back = "#3A3A3A"; fore = "#CFCFCF"; }
            else return;

            BrushConverter conv = new BrushConverter();
            txtLuckBadge.Text = text;
            txtLuckBadge.Foreground = (Brush)conv.ConvertFromString(fore);
            luckBadge.Background = (Brush)conv.ConvertFromString(back);
            luckBadge.Visibility = Visibility.Visible;
        }

        private bool _subscribed;

        public VideoBlockGrid(Channel channel, Video video)
        {
            InitializeComponent();
            Loaded += Control_Loaded;
            Unloaded += Control_Unloaded;
            Translate();
            _channel = channel;
            Video = video;
        }

        private void Translate()
        {
            txtSuspended.Text = EnglishStrings.Suspended.Translate().ToUpper();
            txtSuspendedSub.Text = EnglishStrings.SuspendedHireLawyer.Translate();
            btnDelete.Content = EnglishStrings.VideosDelete.Translate();
            btnUpload.Content = EnglishStrings.VideosPost.Translate();
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            if (_video == null) return;
            if (!_subscribed)
            {
                _video.OnImageFetched += Video_OnImageFetched;
                _subscribed = true;
            }
            //A imagem pode ter chegado enquanto o controle estava fora da tela
            imgVideo.Source = _video.GetThumbnail();
        }

        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_video != null && _subscribed)
            {
                _video.OnImageFetched -= Video_OnImageFetched;
                _subscribed = false;
            }
        }

        private void Video_OnImageFetched()
        {
            imgVideo.Source = _video.GetThumbnail();
        }

        //Miniatura sempre em 16:9 com cantos arredondados, qualquer que seja a largura do cartao
        private void thumbGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double w = thumbGrid.ActualWidth;
            if (w <= 0) return;
            double h = Math.Round(w * 9 / 16);
            if (Math.Abs(thumbGrid.Height - h) > 0.5)
                thumbGrid.Height = h;
            thumbGrid.Clip = new RectangleGeometry(new Rect(0, 0, w, h), 8, 8);
        }

        private string GetNumberOfViews()
        {
            if (_video.Views > 301 && Video.Iterations == 1)
                return "301+";
            return _video.Views.ToShortNumber();
        }

        private void SuspendedAdorner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (LawyerClick != null && StoreItems.Current.Lawyer.Purchased && Video.IsSuspended)
                LawyerClick(this, EventArgs.Empty);
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            if (Video.IsSuspended) return;
            if (UploadClick != null)
                UploadClick(this, EventArgs.Empty);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.ShowDialog(EnglishStrings.Delete.Translate(), EnglishStrings.DeleteVideo.Translate(), MessagePicture.Question, (result) =>
                {
                    if (result == true)
                    {
                        if (DeleteClick != null)
                            DeleteClick(this, EventArgs.Empty);
                    }
                });
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            cardBorder.Background = (Brush)Application.Current.FindResource("YtDarkCardHoverBrush");
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            cardBorder.Background = (Brush)Application.Current.FindResource("YtDarkCardBrush");
        }

        //Clique no cartao (os botoes marcam o evento como tratado, entao nao chegam aqui)
        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.PlayAnimation(() =>
            {
                if (Video.IsSuspended)
                {
                    return;
                }

                if (_channel == Channel.UnreleasedVideos)
                {
                    if (UploadClick != null)
                        UploadClick(this, EventArgs.Empty);
                }
                else
                {
                    ViewVideoDialog dlg = new ViewVideoDialog(Video, _channel);
                    dlg.ShowDialog();
                }
            });
        }
    }
}
