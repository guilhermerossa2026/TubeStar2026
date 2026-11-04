using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Win32;

namespace TubeStar
{
    /// <summary>
    /// Interaction logic for TitlePage.xaml
    /// </summary>
    public partial class TitlePage : UserControl
    {
        public event Action ContinueGameClicked;
        public event Action<bool, bool> NewGameClicked;

        public TitlePage()
        {
            InitializeComponent();

            Refresh();

            Loaded += (s, e) =>
            {
                ThumbLibrary.ProgressChanged -= OnLibraryProgress;
                ThumbLibrary.ProgressChanged += OnLibraryProgress;
                _wasUpdating = ThumbLibrary.IsUpdating;
                UpdateLibraryStatus();
            };
            Unloaded += (s, e) =>
            {
                ThumbLibrary.ProgressChanged -= OnLibraryProgress;
                _hideTimer.Stop();
            };
            _hideTimer.Interval = TimeSpan.FromSeconds(4);
            _hideTimer.Tick += (s, e) =>
            {
                _hideTimer.Stop();
                barThumbLib.Visibility = Visibility.Collapsed;
            };
        }

        private bool _wasUpdating;
        private readonly System.Windows.Threading.DispatcherTimer _hideTimer = new System.Windows.Threading.DispatcherTimer();

        //Vem da thread de fundo: passa para a thread da tela
        private void OnLibraryProgress()
        {
            Dispatcher.BeginInvoke(new Action(UpdateLibraryStatus));
        }

        private void UpdateLibraryStatus()
        {
            if (ThumbLibrary.IsUpdating)
            {
                _wasUpdating = true;
                _hideTimer.Stop();
                int total = ThumbLibrary.ChannelsTotal;
                int done = ThumbLibrary.ChannelsDone;
                int percent = total > 0 ? Math.Min(100, done * 100 / total) : 0;
                txtThumbLib.Text = String.Format(EnglishStrings.ThumbLibUpdating.Translate(), percent, Math.Min(done + 1, total), total);
                barThumbLib.Value = percent;
                barThumbLib.Visibility = Visibility.Visible;
                return;
            }

            int videos = ThumbLibrary.VideoCount;
            if (videos == 0)
            {
                //Sem biblioteca (sem chave/internet): não mostra nada
                txtThumbLib.Text = "";
                barThumbLib.Visibility = Visibility.Collapsed;
                return;
            }

            txtThumbLib.Text = String.Format((ThumbLibrary.LastUpdateFailed ? EnglishStrings.ThumbLibFailed : EnglishStrings.ThumbLibReady).Translate(), videos);
            if (_wasUpdating)
            {
                //Acabou de terminar: a barra fica cheia por uns 4 s
                _wasUpdating = false;
                _hideTimer.Stop();
                if (ThumbLibrary.LastUpdateFailed)
                {
                    barThumbLib.Visibility = Visibility.Collapsed;
                }
                else
                {
                    barThumbLib.Value = 100;
                    _hideTimer.Start();
                }
            }
            else
            {
                barThumbLib.Visibility = Visibility.Collapsed;
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            NewGameDialog dialog = new NewGameDialog();
            dialog.CasualClicked += () =>
            {
                if (NewGameClicked != null)
                    NewGameClicked(false, false);
            };
            dialog.CareerClicked += () =>
            {
                if (NewGameClicked != null)
                    NewGameClicked(true, false);
            };
            dialog.UltraClicked += () =>
            {
                if (NewGameClicked != null)
                    NewGameClicked(true, true);
            };
            dialog.ShowDialog();
        }

        private void ContinueGame_Click(object sender, RoutedEventArgs e)
        {
            if (ContinueGameClicked != null)
                ContinueGameClicked();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void Refresh()
        {
            btnContinue.IsEnabled = System.IO.File.Exists(SaveLoadHelper.SaveFile);
            Translate();
        }

        private void Translate()
        {
            btnContinue.Content = EnglishStrings.ContinueGame.Translate();
            btnNewGame.Content = EnglishStrings.NewGame.Translate();
            btnExit.Content = EnglishStrings.Exit.Translate();
            txtTagline.Text = EnglishStrings.TitleTagline.Translate();
            UpdateLibraryStatus();
        }

        private void imgGeorgia_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Georgia");
            Refresh();
        }

        private void imgPoland_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Poland");
            Refresh();
        }

        private void imgRussia_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Russia");
            Refresh();
        }

        private void imgUK_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("English (default)");
            Refresh();
        }

        private void imgFrance_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("France");
            Refresh();
        }

        private void imgGermany_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Germany");
            Refresh();
        }

        private void imgSweden_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Sweden");
            Refresh();
        }

        private void imgThailand_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Thailand");
            Refresh();
        }

        private void imgTurkey_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Turkey");
            Refresh();
        }

        private void imgChina_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("China");
            Refresh();
        }

        private void imgNetherlands_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Netherlands");
            Refresh();
        }

        private void imgSpain_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Spain");
            Refresh();
        }

        private void imgAlbania_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Albania");
            Refresh();
        }

        private void imgRomania_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Romania");
            Refresh();
        }

        private void imgIndonesia_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Indonesia");
            Refresh();
        }

        private void imgHungary_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Hungary");
            Refresh();
        }

        private void imgItaly_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Italy");
            Refresh();
        }

        private void imgCzech_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Czech");
            Refresh();
        }

        private void imgBrazil_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Brazil");
            Refresh();
        }

        private void imgPortugal_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Languages.SetLanguage("Portugal");
            Refresh();
        }

    }
}