using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Xceed.Wpf.Toolkit;

namespace TubeStar
{
    /// <summary>
    /// Interaction logic for EditVideoDialog.xaml
    /// </summary>
    public partial class EditVideoDialog : ChildWindow
    {
        public Video Video { get; private set; }

        public EditVideoDialog(List<Video> videos)
        {
            InitializeComponent();
            Translate();
            FocusedElement = cmbVideo;

            cmbVideo.ItemsSource = GetData(videos);
            cmbVideo.DisplayMemberPath = "Value";
            cmbVideo.SelectedValuePath = "Key";

            if (videos.Count == 1)
            {
                cmbVideo.SelectedValue = videos[0];
                cmbVideo.IsEnabled = false;
            }
        }

        private void Translate()
        {
            Caption = EnglishStrings.EditVideo.Translate();
            lblVideo.Text = EnglishStrings.Video.Translate() + ":";
            lblHoursSelect.Text = EnglishStrings.Hours.Translate() + ":";
            btnOk.Content = EnglishStrings.Ok.Translate();
            btnCancel.Content = EnglishStrings.Cancel.Translate();
            //Horas fixas pela habilidade de edição
            lblHours.Text = EditVideo.HoursForSkill(Player.Current.PostProductionSkill) + " " + EnglishStrings.Hours.Translate().ToLower();
            lblHoursHint.Text = EnglishStrings.HoursFixedHint.Translate();
        }

        private Dictionary<Video, string> GetData(List<Video> videos)
        {
            Dictionary<Video, string> data = new Dictionary<Video, string>();
            foreach (var video in videos)
            {
                data[video] = video.Name;
            }
            return data;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (cmbVideo.SelectedValue == null)
            {
                CustomMessageBox.ShowDialog(EnglishStrings.MissingValueHeader.Translate(), EnglishStrings.MissingVideo.Translate(), MessagePicture.Puzzle);
                return;
            }

            Video = (Video)cmbVideo.SelectedValue;
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}