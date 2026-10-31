using Nicenis.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TubeStar
{
    /// <summary>
    /// Interaction logic for VideoBlockGrid.xaml
    /// </summary>
    public partial class VideoBlockGrid : UserControl
    {
        public event EventHandler UploadClick;
        public event EventHandler LawyerClick;

        private Channel _channel;

        private Video _video;
        public Video Video
        {
            get { return _video; }
            private set
            {
                _video = value;
                DataContext = new VideoDataContext(_video);
                if (_video != null)
                {
                    if (_channel == null)
                        throw new NotSupportedException("Channel cannot be null");

                    bool isUnreleasedChannel = (_channel == Channel.UnreleasedVideos);

                    txtName.Text = _video.Name;
                    txtViews.Text = String.Format("{0} {1}", GetNumberOfViews(), EnglishStrings.Views.Translate());
                    txtLikes.Text = _video.Likes.ToNumberString();
                    txtDislikes.Text = _video.Dislikes.ToNumberString();

                    panelStats.Visibility = isUnreleasedChannel ? Visibility.Collapsed : Visibility.Visible;

                    txtQuality.Text = GetQualityText(isUnreleasedChannel);

                    SuspendedAdorner.Visibility = _video.IsSuspended ? Visibility.Visible : Visibility.Collapsed;
                    txtSuspendedSub.Visibility = Visibility.Collapsed;
                    if (_video.IsSuspended && StoreItems.Current.Lawyer.Purchased)
                    {
                        txtSuspendedSub.Visibility = Visibility.Visible;
                    }

                    imgVideo.Source = _video.GetThumbnail();
                }
            }
        }

        private bool _subscribed;

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

        private string GetNumberOfViews()
        {
            if (_video.Views > 301 && Video.Iterations == 1)
                return "301+";
            return _video.Views.ToNumberString();
        }

        public void HideInformation()
        {
            bottomGrid.Visibility = topGrid.Visibility = Visibility.Collapsed;
        }

        private void SuspendedAdorner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (LawyerClick != null && StoreItems.Current.Lawyer.Purchased && Video.IsSuspended)
                LawyerClick(this, EventArgs.Empty);
        }

        private int GetCount(Video video)
        {
            return video.GetCommentsCount();
        }

        private string GetQualityText(bool isUnreleasedChannel)
        {
            bool canView = !isUnreleasedChannel || Player.Current.CanViewQualityBeforeUpload;
            qualityGrid.Visibility = canView ? Visibility.Visible : Visibility.Collapsed;
            return String.Format("{0}", canView ? String.Format("{0}", _video.GenerateQuality()) : "-");
        }

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
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Background = (Brush)System.Windows.Application.Current.FindResource("YtDarkCardHoverBrush");
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Background = Brushes.Transparent;
        }

        private void UserControl_PreviewMouseUp(object sender, MouseButtonEventArgs e)
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
                    ViewVideoDialog dlg = new ViewVideoDialog(Video);
                    dlg.ShowDialog();
                }
            });
        }
    }

    public class VideoDataContext : IDataObjectProvider
    {
        private Video _video;

        public VideoDataContext(Video video)
        {
            _video = video;
        }

        public object GetDataObject()
        {
            return _video;
        }
    }
}