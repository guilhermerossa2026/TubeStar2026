using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace TubeStar
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : UserControl
    {
        public event Action GameLose;
        public event Action GameExit;

        public MainPage()
        {
            InitializeComponent();
            Translate();
            txtMoney.Text = Player.Current.Money.ToCurrencyString();
            UpdateHeader();
            //Remove ao sair da tela: cada jogo novo cria outra MainPage
            Player.Current.MoneyChanged += Player_MoneyChanged;
            Unloaded += (s, e) => Player.Current.MoneyChanged -= Player_MoneyChanged;

            dailyPlanner.NewDayCompleted += () =>
            {
                this.IsEnabled = true;
                progress.Visibility = System.Windows.Visibility.Collapsed;

                //A cada 3 dias roda um evento; depois dele, tudo é atualizado (uma vez só)
                bool eventDay = Player.Current.Iterations > 1 && Player.Current.Iterations % 3 == 0;
                if (eventDay)
                    RandomEvents.RunEvent();

                //Depois do evento: ele pode mudar inscritos sem mexer no dinheiro
                UpdateHeader();

                if (eventDay || videoManager.Visibility == System.Windows.Visibility.Visible)
                    videoManager.Update();

                if (postVideo.Visibility == System.Windows.Visibility.Visible)
                    postVideo.Update();

                if (eventDay || rivalViewer.Visibility == System.Windows.Visibility.Visible)
                    rivalViewer.Update();

                if (eventDay)
                    dailyPlanner.Update();

                CheckTrophies();
            };

            //"Gravar vídeo" da Agenda abre a tela Criar vídeo; ao criar, a tarefa vai para a Agenda
            dailyPlanner.CreateVideoRequested += () => btnCreateVideo_Click(null, null);
            createVideo.VideoCreated += (task) =>
            {
                dailyPlanner.AddTask(task);
                btnDailyPlanner_Click(null, null);
            };

            //Postar vídeo: vem do menu ou do botão "Postar" de Meus vídeos
            videoManager.PostRequested += (video) => ShowPostVideo(video);
            postVideo.CreateVideoRequested += () => btnCreateVideo_Click(null, null);
            postVideo.VideoPosted += () =>
            {
                UpdateHeader();
                CheckTrophies();
            };

            dailyPlanner.GameExit += () =>
            {
                if (GameLose != null)
                    GameLose();
            };
        }

        private void Translate()
        {
            gridHelp.ToolTip = EnglishStrings.Help.Translate();
            gridExit.ToolTip = EnglishStrings.Exit.Translate();
            imgSave.ToolTip = EnglishStrings.SaveGame.Translate();
            txtDailyPlanner.Text = EnglishStrings.NavSchedule.Translate();
            txtVideoPlanner.Text = EnglishStrings.NavMyVideos.Translate();
            txtOnlineStore.Text = EnglishStrings.NavStore.Translate();
            txtRivals.Text = EnglishStrings.TopTubeStars.Translate();
            txtNavHome.Text = EnglishStrings.NavHome.Translate();
            txtNavCreate.Text = EnglishStrings.NavCreateVideo.Translate();
            txtNavPost.Text = EnglishStrings.NavPostVideo.Translate();
            txtNavUniversity.Text = EnglishStrings.NavUniversity.Translate();
            txtNavEmployees.Text = EnglishStrings.NavEmployees.Translate();
            string soon = EnglishStrings.Soon.Translate();
            txtSoon1.Text = soon;
            txtSoon4.Text = soon;
            txtSoon5.Text = soon;
            txtMoneyLabel.Text = EnglishStrings.MoneyLabel.Translate();
            txtSubscribersLabel.Text = EnglishStrings.Subscribers.Translate();
            txtNewDay.Text = EnglishStrings.NextDay.Translate();
            txtSectionTitle.Text = txtDailyPlanner.Text;
        }

        private void Player_MoneyChanged()
        {
            Dispatcher.BeginInvoke(new Action(delegate()
            {
                txtMoney.Text = Player.Current.Money.ToCurrencyString();
                UpdateHeader();
            }));
        }

        //Data do jogo (ano de 112 dias, semana de 7) e total de inscritos no cabeçalho
        private void UpdateHeader()
        {
            //Iterations vale 0 no 1º dia (o DailyPlanner já roda o primeiro NewDay ao abrir)
            int day = Math.Max(0, Player.Current.Iterations);
            int dayOfYear = day % 112 + 1;
            int week = (dayOfYear - 1) / 7 + 1;
            int year = day / 112 + 1;
            //Igual à prévia: "Dia" é o dia do ano (1 a 112)
            txtDate.Text = String.Format(EnglishStrings.GameDate.Translate(), year, week, dayOfYear);

            long subscribers = 0;
            foreach (var channel in Player.Current.Channels)
            {
                if (channel != Channel.UnreleasedVideos)
                    subscribers += channel.Subscribers;
            }
            txtSubscribers.Text = subscribers.ToString("N0");
        }

        private void CheckTrophies()
        {
            foreach (var channel in Player.Current.Channels)
            {
                int heelCount = 0;
                int rantCount = 0;
                int nerdCount = 0;
                foreach (var video in channel.Videos)
                {
                    if (!TrophyManager.HasTrophy(Trophy.InternetFamous) && video.Views >= 100000)
                        TrophyManager.UnlockTrophy(Trophy.InternetFamous);

                    if (!TrophyManager.HasTrophy(Trophy.PropDepartment) && video.Cost == 400)
                        TrophyManager.UnlockTrophy(Trophy.PropDepartment);

                    if (!TrophyManager.HasTrophy(Trophy.WellHeeld) && video.Category == VideoCategory.Hauls && video.Name.ToLower().Contains("heels"))
                        heelCount++;

                    if (!TrophyManager.HasTrophy(Trophy.RantMaster) && video.Category == VideoCategory.Vlog && video.Name.ToLower().Contains("rant"))
                        rantCount++;

                    if (!TrophyManager.HasTrophy(Trophy.Procrastinator) && video.Category == VideoCategory.Gaming && video.Name.ToLower().Contains("nerd"))
                        nerdCount++;
                }

                if (heelCount >= 5)
                    TrophyManager.UnlockTrophy(Trophy.WellHeeld);

                if (rantCount >= 5)
                    TrophyManager.UnlockTrophy(Trophy.RantMaster);

                if (nerdCount >= 3)
                    TrophyManager.UnlockTrophy(Trophy.Procrastinator);
            }

            if (!TrophyManager.HasTrophy(Trophy.AptPupil))
            {
                bool completed = true;
                foreach (var study in Studies.Current.All)
                {
                    if (!study.IsCompleted)
                    {
                        completed = false;
                        break;
                    }
                }

                if (completed)
                    TrophyManager.UnlockTrophy(Trophy.AptPupil);
            }
        }

        //Marca o item do menu, mostra só a tela dele e troca o título da seção
        private void ShowSection(ToggleButton navButton, UIElement screen, string title)
        {
            ToggleButton[] buttons = { btnDailyPlanner, btnVideoPlanner, btnCreateVideo, btnPostVideo, btnOnlineStore, btnRivals };
            foreach (var button in buttons)
                button.IsChecked = button == navButton;

            UIElement[] screens = { dailyPlanner, videoManager, createVideo, postVideo, onlineStore, rivalViewer };
            foreach (var s in screens)
                s.Visibility = s == screen ? Visibility.Visible : Visibility.Collapsed;

            txtSectionTitle.Text = title;
        }

        private void btnDailyPlanner_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(btnDailyPlanner, dailyPlanner, txtDailyPlanner.Text);
        }

        private void btnVideoPlanner_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(btnVideoPlanner, videoManager, txtVideoPlanner.Text);
            videoManager.Update();
        }

        private void btnCreateVideo_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(btnCreateVideo, createVideo, txtNavCreate.Text);
            createVideo.Update();
        }

        private void btnPostVideo_Click(object sender, RoutedEventArgs e)
        {
            ShowPostVideo(null);
        }

        private void ShowPostVideo(Video video)
        {
            ShowSection(btnPostVideo, postVideo, txtNavPost.Text);
            postVideo.Update(video);
        }

        private void btnOnlineStore_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(btnOnlineStore, onlineStore, txtOnlineStore.Text);
            onlineStore.Update();
        }

        private void btnRivals_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(btnRivals, rivalViewer, txtRivals.Text);
            rivalViewer.Update();
        }

        private void btnNewDay_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            progress.Visibility = System.Windows.Visibility.Visible;

            //Rivais primeiro: a simulação do dia já considera os vídeos novos deles
            rivalViewer.NewDay();
            dailyPlanner.NewDay();
        }

        private void Tutorial_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!TrophyManager.HasTrophy(Trophy.Pupil))
                TrophyManager.UnlockTrophy(Trophy.Pupil);

            System.Diagnostics.Process.Start("http://www.youtube.com/watch?v=oKfNSm1SLSQ");
        }

        private void Save_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Directory.Exists(SaveLoadHelper.SaveDirectory))
            {
                Directory.CreateDirectory(SaveLoadHelper.SaveDirectory);
            }

            if (File.Exists(SaveLoadHelper.SaveFile))
            {
                CustomMessageBox.ShowDialog(EnglishStrings.OverwriteSave.Translate(), EnglishStrings.SaveExists.Translate(), MessagePicture.Question, (result) =>
                    {
                        if (result == true)
                            DoSave();
                    });
            }
            else
            {
                DoSave();
            }
        }

        //A agenda do dia vai junto no save e é restaurada no Continuar,
        //então as horas já marcadas ficam como estão (antes eram tiradas aqui)
        private void DoSave()
        {
            SaveLoadHelper.Save(SaveLoadHelper.SaveFile);
        }

        private void Exit_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CustomMessageBox.ShowDialog(EnglishStrings.Confirm.Translate(), EnglishStrings.LeaveGame.Translate(), MessagePicture.Question, (result) =>
            {
                if (result == true)
                {
                    if (GameExit != null)
                        GameExit();
                }
            });
        }
    }
}