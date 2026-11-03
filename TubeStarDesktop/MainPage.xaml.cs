using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

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
            SetMoneyText();
            UpdateHeader();
            //Remove ao sair da tela: cada jogo novo cria outra MainPage
            Player.Current.MoneyChanged += Player_MoneyChanged;
            Unloaded += (s, e) =>
            {
                Player.Current.MoneyChanged -= Player_MoneyChanged;
                ActiveChannel.Changed -= ActiveChannel_Changed;
                if (hostWindow != null)
                    hostWindow.PreviewKeyDown -= Window_PreviewKeyDown;
            };
            //Barra de espaço: passa o dia ou fecha avisos só com OK (escuta na janela, vale com qualquer foco)
            Loaded += (s, e) =>
            {
                hostWindow = Window.GetWindow(this);
                if (hostWindow != null)
                {
                    hostWindow.PreviewKeyDown -= Window_PreviewKeyDown;
                    hostWindow.PreviewKeyDown += Window_PreviewKeyDown;
                }
            };
            ActiveChannel.Changed += ActiveChannel_Changed;

            dailyPlanner.NewDayCompleted += () =>
            {
                this.IsEnabled = true;
                progress.Visibility = System.Windows.Visibility.Collapsed;

                //Top TubeStars: rivais crescem e as posições do dia são calculadas
                Ranking.Current.NewDay(Player.Current.Iterations);

                //A cada 3 dias roda um evento; depois dele, tudo é atualizado (uma vez só)
                bool eventDay = Player.Current.Iterations > 1 && Player.Current.Iterations % 3 == 0;
                if (eventDay)
                {
                    double moneyBefore = Player.Current.Money;
                    long subscribersBefore = TotalSubscribers();
                    RandomEvents.RunEvent();
                    Player.Current.RecordEvent(Player.Current.Money - moneyBefore, TotalSubscribers() - subscribersBefore);
                    dailyPlanner.Update();
                }

                //Depois do evento: ele pode mudar inscritos sem mexer no dinheiro
                UpdateHeader();

                if (eventDay || videoManager.Visibility == System.Windows.Visibility.Visible)
                    videoManager.Update();

                if (postVideo.Visibility == System.Windows.Visibility.Visible)
                    postVideo.Update();

                if (eventDay || rivalViewer.Visibility == System.Windows.Visibility.Visible)
                    rivalViewer.Update();

                CheckTrophies();

                //Fim de semana/ano: resumo do período
                if (dailyPlanner.PendingSummary != null)
                {
                    new PeriodSummaryDialog(dailyPlanner.PendingSummaryTitle, dailyPlanner.PendingSummary).ShowDialog();
                    dailyPlanner.PendingSummary = null;
                }
            };

            //"Gravar vídeo" da Agenda abre a tela Criar vídeo; ao criar, a tarefa vai para a Agenda
            dailyPlanner.CreateVideoRequested += () => btnCreateVideo_Click(null, null);
            createVideo.VideoCreated += (task) =>
            {
                dailyPlanner.AddTask(task);
                btnDailyPlanner_Click(null, null);
            };

            //Postar vídeo: só pelo botão "Postar" de Meus vídeos; ao postar ou voltar, volta para Meus vídeos
            videoManager.PostRequested += (video) => ShowPostVideo(video);
            postVideo.CreateVideoRequested += () => btnCreateVideo_Click(null, null);
            postVideo.BackRequested += () => btnVideoPlanner_Click(null, null);
            postVideo.VideoPosted += () =>
            {
                UpdateHeader();
                CheckTrophies();
                btnVideoPlanner_Click(null, null);
            };

            dailyPlanner.GameExit += () =>
            {
                if (GameLose != null)
                    GameLose();
            };
        }

        private void Translate()
        {
            gridExit.ToolTip = EnglishStrings.Exit.Translate();
            imgSave.ToolTip = EnglishStrings.SaveGame.Translate();
            txtDailyPlanner.Text = EnglishStrings.NavSchedule.Translate();
            txtVideoPlanner.Text = EnglishStrings.NavMyVideos.Translate();
            txtOnlineStore.Text = EnglishStrings.NavStore.Translate();
            txtRivals.Text = EnglishStrings.TopTubeStars.Translate();
            txtNavHome.Text = EnglishStrings.NavHome.Translate();
            txtNavCreate.Text = EnglishStrings.NavCreateVideo.Translate();
            txtNavUniversity.Text = EnglishStrings.NavUniversity.Translate();
            txtNavEmployees.Text = EnglishStrings.NavEmployees.Translate();
            string soon = EnglishStrings.Soon.Translate();
            txtSoon1.Text = soon;
            txtSoon4.Text = soon;
            txtSoon5.Text = soon;
            txtMoneyLabel.Text = EnglishStrings.MoneyLabel.Translate();
            txtSubscribersLabel.Text = EnglishStrings.Subscribers.Translate();
            txtNewDay.Text = EnglishStrings.NextDay.Translate();
            btnCalendar.ToolTip = EnglishStrings.CalendarOpenHint.Translate();
            txtSectionTitle.Text = txtDailyPlanner.Text;
        }

        //Dinheiro do cabeçalho: curto a partir de 1 milhão (valor exato no ToolTip), para não empurrar o layout
        private void SetMoneyText()
        {
            double money = Player.Current.Money;
            txtMoney.Text = Math.Abs(money) >= 1000000 ? money.ToShortCurrencyString() : money.ToCurrencyString();
            txtMoney.ToolTip = money.ToCurrencyString();
        }

        private void Player_MoneyChanged()
        {
            Dispatcher.BeginInvoke(new Action(delegate()
            {
                SetMoneyText();
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

            //Inscritos do canal escolhido no seletor (ou de todos, na Visão geral)
            txtSubscribers.Text = ActiveChannel.Subscribers.ToShortNumber();
            UpdateChannelSelector();
        }

        #region Seletor de canal

        private void ActiveChannel_Changed()
        {
            UpdateHeader();
            videoManager.ShowActiveChannel();
        }

        private void UpdateChannelSelector()
        {
            var channel = ActiveChannel.Current;
            if (channel == null)
            {
                txtSelectorName.Text = EnglishStrings.ChannelOverview.Translate();
                int count = Player.Current.Channels.Count(c => c != Channel.UnreleasedVideos);
                txtSelectorInfo.Text = count == 1 ? EnglishStrings.ChannelOverviewInfoOne.Translate() : String.Format(EnglishStrings.ChannelOverviewInfo.Translate(), count);
                selectorAvatar.Content = RivalControl.CreateAvatar(txtSelectorName.Text, 28);
            }
            else
            {
                txtSelectorName.Text = channel.Name;
                txtSelectorInfo.Text = channel.Niche.HasValue ? channel.Niche.Value.GetString() : "";
                selectorAvatar.Content = RivalControl.CreateAvatar(channel.Name, 28);
            }
        }

        private void btnChannelSelector_Click(object sender, RoutedEventArgs e)
        {
            channelOptions.Children.Clear();
            channelOptions.Children.Add(CreateChannelOption(EnglishStrings.ChannelOverview.Translate(), null, null));
            foreach (var channel in Player.Current.Channels.Where(c => c != Channel.UnreleasedVideos))
                channelOptions.Children.Add(CreateChannelOption(channel.Name, channel.Niche.HasValue ? channel.Niche.Value.GetString() : null, channel));

            var add = new Button { Content = EnglishStrings.VideosNewChannel.Translate(), Style = (Style)FindResource("GhostButtonStyle"), HorizontalContentAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 4, 0, 0) };
            add.Click += (s, args) =>
            {
                popupChannels.IsOpen = false;
                var dialog = new AddChannelDialog();
                dialog.ShowDialog(() =>
                {
                    if (dialog.Channel != null)
                    {
                        Player.Current.Channels.Add(dialog.Channel);
                        ActiveChannel.Select(dialog.Channel);
                        videoManager.Update();
                    }
                });
            };
            channelOptions.Children.Add(add);
            popupChannels.IsOpen = true;
        }

        private Button CreateChannelOption(string name, string info, Channel channel)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            panel.Children.Add(RivalControl.CreateAvatar(name, 24));
            var texts = new StackPanel { Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            texts.Children.Add(new TextBlock { Text = name, FontWeight = FontWeights.SemiBold });
            if (!String.IsNullOrEmpty(info))
                texts.Children.Add(new TextBlock { Text = info, FontSize = 11, Foreground = (Brush)FindResource("YtTextSecondaryBrush") });
            panel.Children.Add(texts);

            var option = new Button { Content = panel, Style = (Style)FindResource("GhostButtonStyle"), HorizontalContentAlignment = HorizontalAlignment.Left, Padding = new Thickness(8, 5, 8, 5) };
            if (channel == ActiveChannel.Current)
                option.Background = (Brush)FindResource("YtDarkCardHoverBrush");
            option.Click += (s, e) =>
            {
                popupChannels.IsOpen = false;
                ActiveChannel.Select(channel);
                //Mesmo canal já ativo: a grade pode estar em "Não lançados", então mostra de novo
                videoManager.ShowActiveChannel();
            };
            return option;
        }

        #endregion

        private static long TotalSubscribers()
        {
            long subscribers = 0;
            foreach (var channel in Player.Current.Channels)
            {
                if (channel != Channel.UnreleasedVideos)
                    subscribers += channel.Subscribers;
            }
            return subscribers;
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

                    if (!TrophyManager.HasTrophy(Trophy.WellHeeld) && video.Category == VideoCategory.Beauty && video.Name.ToLower().Contains("heels"))
                        heelCount++;

                    if (!TrophyManager.HasTrophy(Trophy.RantMaster) && video.Category == VideoCategory.Vlog && video.Name.ToLower().Contains("rant"))
                        rantCount++;

                    if (!TrophyManager.HasTrophy(Trophy.Procrastinator) && video.Category == VideoCategory.Games && video.Name.ToLower().Contains("nerd"))
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
            ToggleButton[] buttons = { btnDailyPlanner, btnVideoPlanner, btnCreateVideo, btnOnlineStore, btnRivals };
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

        //Tela de postar faz parte de Meus vídeos (o menu continua marcando Meus vídeos)
        private void ShowPostVideo(Video video)
        {
            ShowSection(btnVideoPlanner, postVideo, EnglishStrings.NavPostVideo.Translate());
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

        private void btnCalendar_Click(object sender, RoutedEventArgs e)
        {
            new CalendarDialog().ShowDialog();
        }

        private Window hostWindow;

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Space || !IsVisible)
                return;
            //Tecla segurada: continua bloqueando onde o espaço é bloqueado, mas só age no primeiro toque
            bool firstPress = !e.IsRepeat;

            //Digitando: o espaço continua sendo espaço
            if (Keyboard.FocusedElement is TextBoxBase)
                return;

            //Janela aberta: só fecha avisos com OK; confirmações (OK/Cancelar) ignoram o espaço;
            //nas demais janelas o espaço segue normal (ativa o botão focado)
            var root = LogicalTreeHelper.FindLogicalNode(Application.Current.MainWindow, "LayoutRoot") as Grid;
            if (root != null)
            {
                var dialog = root.Children.OfType<Xceed.Wpf.Toolkit.ChildWindow>().LastOrDefault();
                if (dialog != null)
                {
                    var summary = dialog as PeriodSummaryDialog;
                    if (summary != null)
                    {
                        e.Handled = true;
                        if (firstPress)
                            summary.Confirm();
                        return;
                    }

                    var messageBox = dialog as CustomMessageBox;
                    if (messageBox != null)
                    {
                        e.Handled = true;
                        if (messageBox.IsOkOnly && firstPress)
                            messageBox.Confirm();
                    }
                    return;
                }
            }

            //Telas de criação e loja: o espaço não passa o dia (evita pular dia sem querer)
            if (createVideo.IsVisible || postVideo.IsVisible || onlineStore.IsVisible)
                return;

            e.Handled = true;
            if (firstPress && IsEnabled && btnNewDay.IsEnabled && btnNewDay.IsVisible)
                btnNewDay_Click(btnNewDay, new RoutedEventArgs());
        }

        private void btnNewDay_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            progress.Visibility = System.Windows.Visibility.Visible;

            dailyPlanner.NewDay();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
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
            if (SaveLoadHelper.Save(SaveLoadHelper.SaveFile))
                ShowSavedNotice();
        }

        private System.Windows.Threading.DispatcherTimer savedTimer;

        //Aviso curto "Jogo salvo" no cabeçalho; clicar de novo reinicia os 2 segundos
        private void ShowSavedNotice()
        {
            txtSaved.Text = EnglishStrings.GameSaved.Translate() + " ✓";
            txtSaved.Visibility = System.Windows.Visibility.Visible;

            if (savedTimer == null)
            {
                savedTimer = new System.Windows.Threading.DispatcherTimer();
                savedTimer.Interval = TimeSpan.FromSeconds(2);
                savedTimer.Tick += (s, args) =>
                {
                    savedTimer.Stop();
                    txtSaved.Visibility = System.Windows.Visibility.Collapsed;
                };
            }
            savedTimer.Stop();
            savedTimer.Start();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
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