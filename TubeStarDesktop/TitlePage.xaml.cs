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