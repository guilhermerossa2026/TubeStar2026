using System;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace TubeStar
{
    /// <summary>
    /// Ficha do vídeo: miniatura, nota e números
    /// </summary>
    public partial class ViewVideoDialog : ChildWindow
    {
        public ViewVideoDialog(Video video, Channel channel)
        {
            InitializeComponent();
            Caption = EnglishStrings.ViewVideo.Translate();
            btnOk.Content = EnglishStrings.Ok.Translate();

            imgVideo.Source = video.GetThumbnail();
            txtName.Text = video.Name;
            txtChannel.Text = channel != null ? channel.Name : "";
            txtScore.Text = String.Format(EnglishStrings.VideosScore.Translate(), (video.GenerateQuality() / 10.0).ToString("0.0"));

            AddKpi(EnglishStrings.Views.Translate(), video.Views.ToNumberString());
            AddKpi(EnglishStrings.Likes.Translate(), video.Likes.ToNumberString());
            AddKpi(EnglishStrings.Dislikes.Translate(), video.Dislikes.ToNumberString());
            AddKpi(EnglishStrings.Comments.Translate(), video.GetCommentsCount().ToNumberString());
            AddKpi(EnglishStrings.DaysOnline.Translate(), video.Iterations.ToNumberString());
        }

        private void AddKpi(string label, string value)
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = label, Style = (Style)FindResource("KpiLabelStyle") });
            panel.Children.Add(new TextBlock { Text = value, Style = (Style)FindResource("KpiValueStyle"), FontSize = 18 });
            kpiGrid.Children.Add(panel);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
