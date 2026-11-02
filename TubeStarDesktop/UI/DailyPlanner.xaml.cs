using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TubeStar
{
    /// <summary>
    /// Interaction logic for DailyPlanner.xaml
    /// </summary>
    public partial class DailyPlanner : UserControl
    {
        public event Action GameExit;
        public event Action NewDayCompleted;

        public List<Task> Appointments { get; private set; }
        private double _moneyAtStartOfDay;

        private const string JobMarker = "Job";

        //Agenda que está na tela (a MainPage atual tem um só DailyPlanner): usada pelo Save
        private static DailyPlanner _active;
        //Agenda vinda do Load, consumida pelo próximo DailyPlanner criado (Continuar)
        private static List<string> _agendaToRestore;
        private static double? _moneyAtStartToRestore;

        public DailyPlanner()
        {
            InitializeComponent();
            Translate();
            Appointments = new List<Task>();
            _active = this;

            var agenda = _agendaToRestore;
            var moneyAtStart = _moneyAtStartToRestore;
            SetAgendaToRestore(null, null);

            if (agenda != null)
                RestoreDay(agenda, moneyAtStart);
            else
                NewDay();
        }

        public static void SetAgendaToRestore(List<string> agenda, double? moneyAtStartOfDay)
        {
            _agendaToRestore = agenda;
            _moneyAtStartToRestore = moneyAtStartOfDay;
        }

        public static List<string> GetCurrentAgendaForSave()
        {
            if (_active == null || _active.Appointments == null)
                return null;

            var result = new List<string>();
            foreach (var task in _active.Appointments)
            {
                if (task.TaskType == TaskType.Job)
                {
                    result.Add(JobMarker);
                }
                else
                {
                    int index = Player.Current.TasksInProgress.IndexOf(task);
                    if (index >= 0)
                        result.Add(index.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
            }
            return result;
        }

        public static double? GetMoneyAtStartOfDayForSave()
        {
            if (_active == null)
                return null;
            return _active._moneyAtStartOfDay;
        }

        //Volta ao mesmo dia do save: sem cobrar, pagar ou simular.
        //As horas das tarefas (HoursPutIn) já foram salvas com a agenda somada.
        private void RestoreDay(List<string> agenda, double? moneyAtStart)
        {
            _moneyAtStartOfDay = moneyAtStart.HasValue ? moneyAtStart.Value : Player.Current.Money;

            Job job = null;
            foreach (var marker in agenda)
            {
                if (Appointments.Count >= 15)
                    break;

                if (marker == JobMarker)
                {
                    if (job == null)
                        job = new Job();
                    job.HoursPutIn++;
                    Appointments.Add(job);
                }
                else
                {
                    int index;
                    if (int.TryParse(marker, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out index)
                        && index >= 0 && index < Player.Current.TasksInProgress.Count)
                    {
                        Appointments.Add(Player.Current.TasksInProgress[index]);
                    }
                }
            }
            Update();
        }

        private void Translate()
        {
            txtTitle.Text = EnglishStrings.NavSchedule.Translate();
            txtLead.Text = EnglishStrings.ScheduleLead.Translate();
            txtDayHours.Text = EnglishStrings.ScheduleDayHours.Translate();
            txtAddTask.Text = "+ " + EnglishStrings.AddTask.Translate();
            txtAddTaskHint.Text = EnglishStrings.ScheduleAddTaskHint.Translate();
            txtSummaryTitle.Text = EnglishStrings.SummaryYesterday.Translate();
            txtNoSummary.Text = EnglishStrings.SummaryNoDayYet.Translate();
        }

        //Resumo da semana ou do ano que acabou de fechar: a MainPage mostra depois do dia
        public PeriodStats PendingSummary { get; set; }
        public string PendingSummaryTitle { get; set; }

        //Fecha a semana a cada 7 dias (Dia 8, 15...) e o ano a cada 112 (aí só o resumo do ano)
        private void CheckPeriodEnd()
        {
            int day = Player.Current.Iterations;
            if (day <= 0 || day % 7 != 0)
                return;

            if (Player.Current.PastSummaries == null)
                Player.Current.PastSummaries = new List<PeriodStats>();

            //A semana sempre vai para o histórico (a última do ano também)
            var week = Player.Current.CurrentWeek ?? new PeriodStats();
            week.Title = String.Format(EnglishStrings.SummaryWeekTitle.Translate(), (day - 1) % 112 / 7 + 1, (day - 1) / 112 + 1);
            Player.Current.PastSummaries.Add(week);
            Player.Current.CurrentWeek = null;

            if (day % 112 == 0)
            {
                var year = Player.Current.CurrentYear ?? new PeriodStats();
                year.Title = String.Format(EnglishStrings.SummaryYearTitle.Translate(), day / 112);
                Player.Current.PastSummaries.Add(year);
                Player.Current.CurrentYear = null;
                PendingSummary = year;
            }
            else
            {
                PendingSummary = week;
            }
            PendingSummaryTitle = PendingSummary.Title;
        }

        private void UpdateSummary()
        {
            summaryGrid.Children.Clear();
            var day = Player.Current.LastDay;
            txtNoSummary.Visibility = day == null ? Visibility.Visible : Visibility.Collapsed;
            summaryGrid.Visibility = day == null ? Visibility.Collapsed : Visibility.Visible;
            if (day == null)
                return;

            foreach (var kpi in CreateSummaryKpis(day))
                summaryGrid.Children.Add(kpi);
        }

        //KPIs comuns ao painel do dia e às janelas de semana/ano
        public static List<UIElement> CreateSummaryKpis(PeriodStats stats)
        {
            var result = new List<UIElement>();
            result.Add(CreateKpi(EnglishStrings.SummaryEarned.Translate(), stats.Earned.ToCurrencyString(), null));
            result.Add(CreateKpi(EnglishStrings.SummarySpent.Translate(), stats.Spent.ToCurrencyString(), null));
            result.Add(CreateKpi(EnglishStrings.SummaryBalance.Translate(), stats.Balance.ToCurrencyString(),
                FindBrush(stats.Balance < 0 ? "YtLossBrush" : "YtGainBrush")));
            result.Add(CreateKpi(EnglishStrings.Views.Translate(), stats.Views.ToShortNumber(), null));
            result.Add(CreateKpi(EnglishStrings.SummaryNewSubscribers.Translate(), stats.NewSubscribers.ToShortNumber(), null));
            result.Add(CreateKpi(EnglishStrings.SummaryVideosPosted.Translate(), stats.VideosPosted.ToString("N0"), null));
            result.Add(CreateKpi(EnglishStrings.SummaryTasksDone.Translate(), stats.TasksCompleted.ToString("N0"), null));
            return result;
        }

        private static UIElement CreateKpi(string label, string value, Brush valueBrush)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 0, 12, 0) };
            panel.Children.Add(new TextBlock { Text = label, Style = (Style)Application.Current.FindResource("KpiLabelStyle"), TextWrapping = TextWrapping.Wrap });
            var valueBlock = new TextBlock { Text = value, Style = (Style)Application.Current.FindResource("KpiValueStyle"), FontSize = 17 };
            if (valueBrush != null)
                valueBlock.Foreground = valueBrush;
            panel.Children.Add(valueBlock);
            return panel;
        }

        //Mesmo comportamento do antigo clique num horário da agenda
        private void HourSlot_Click(Task task)
        {
            if (task == null)
                return;

            //Special: quit job
            if (task.TaskType == TaskType.Job)
            {
                AskQuitJob();
            }
            else
            {
                RemoveAppointment(task, false);
            }
        }

        private void AskQuitJob()
        {
            CustomMessageBox.ShowDialog(EnglishStrings.QuitJobHeader.Translate(), EnglishStrings.QuitJobText.Translate(), MessagePicture.Work, (result) =>
            {
                if (result == true)
                {
                    Player.Current.QuitJob = true;
                    Appointments.RemoveAll(a => a.TaskType == TaskType.Job);
                    Update();
                }
            });
        }

        private void AskRemoveTask(Task task)
        {
            bool isStudy = task is Study;
            CustomMessageBox.ShowDialog(EnglishStrings.RemoveTask.Translate(), String.Format("{0} {1}", EnglishStrings.AreYouSure.Translate(), isStudy ? String.Format("\n{0}", EnglishStrings.NoRefunds.Translate()) : ""), MessagePicture.Question, (result) =>
            {
                if (result == true)
                {
                    RemoveAppointment(task, true);
                }
            });
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            AddTaskDialog dialog = new AddTaskDialog();
            dialog.TaskClick += (t) =>
            {
                AddTask(t);
            };
            dialog.CreateVideoRequested += () =>
            {
                if (CreateVideoRequested != null)
                    CreateVideoRequested();
            };
            dialog.ShowDialog();
        }

        //Pedido para abrir a tela "Criar vídeo" (a MainPage troca a seção)
        public event Action CreateVideoRequested;

        public void AddTask(Task task)
        {
            foreach (var checkTask in Player.Current.TasksInProgress)
            {
                if (checkTask == task)
                    return;
            }

            Player.Current.TasksInProgress.Add(task);
            Update();
        }

        private void AddAppointment(Task task)
        {
            if (Appointments.Count < 15)
            {
                task.HoursPutIn++;
                Appointments.Add(task);
                Update();
            }
        }

        private void RemoveAppointment(Task task, bool removeTodoTask)
        {
            //Special case
            var shootVideoTask = task as ShootVideo;
            if (shootVideoTask != null)
            {
                foreach (var currentTask in Player.Current.TasksInProgress)
                {
                    var editVideoTask = currentTask as EditVideo;
                    if (editVideoTask != null && editVideoTask.Video == shootVideoTask.Video)
                    {
                        RemoveAppointment(currentTask, true);
                        break;
                    }
                }
            }

            if (removeTodoTask)
            {
                Appointments.RemoveAll(a => a == task);
                Player.Current.TasksInProgress.Remove(task);
            }
            else
            {
                Appointments.Remove(task);
                task.HoursPutIn--;
            }

            Update();
        }

        public void Update()
        {
            UpdateSummary();
            UpdateHoursBar();

            //Cartões de tarefa
            tasksPanel.Children.Clear();
            Task jobTask = Appointments.FirstOrDefault(a => a.TaskType == TaskType.Job);
            if (jobTask != null)
                tasksPanel.Children.Add(CreateTaskCard(jobTask, true));
            foreach (var task in Player.Current.TasksInProgress)
            {
                tasksPanel.Children.Add(CreateTaskCard(task, false));
            }
        }

        private static Brush FindBrush(string key)
        {
            return (Brush)Application.Current.FindResource(key);
        }

        private static string TypeLabel(Task task)
        {
            switch (task.TaskType)
            {
                case TaskType.Job: return EnglishStrings.Job.Translate();
                case TaskType.ShootVideo: return EnglishStrings.ShootVideo.Translate();
                case TaskType.EditVideo: return EnglishStrings.EditVideo.Translate();
                default: return EnglishStrings.Study.Translate();
            }
        }

        private static string IconKey(Task task)
        {
            switch (task.TaskType)
            {
                case TaskType.Job: return "IconBag";
                case TaskType.ShootVideo: return "IconFilm";
                case TaskType.EditVideo: return "IconPlay";
                default: return "IconCap";
            }
        }

        private void UpdateHoursBar()
        {
            hoursBar.Children.Clear();
            for (int i = 0; i < 15; i++)
            {
                Task task = i < Appointments.Count ? Appointments[i] : null;
                bool startsBlock = task != null && (i == 0 || Appointments[i - 1] != task);
                bool endsBlock = task != null && (i + 1 >= Appointments.Count || Appointments[i + 1] != task);

                var segment = new Border();
                segment.Margin = new Thickness(1, 0, 1, 0);
                segment.CornerRadius = new CornerRadius(startsBlock || task == null ? 6 : 0, endsBlock || task == null ? 6 : 0, endsBlock || task == null ? 6 : 0, startsBlock || task == null ? 6 : 0);
                segment.Background = task == null ? FindBrush("YtDarkCardHoverBrush") : new SolidColorBrush(task.Color);
                segment.ClipToBounds = false;

                if (task != null)
                {
                    segment.Cursor = Cursors.Hand;
                    segment.ToolTip = task.Name;
                    if (startsBlock)
                    {
                        var label = new TextBlock();
                        label.Text = TypeLabel(task);
                        label.Foreground = FindBrush("YtOnAccentBrush");
                        label.FontSize = 11;
                        label.FontWeight = FontWeights.SemiBold;
                        label.VerticalAlignment = VerticalAlignment.Center;
                        label.Margin = new Thickness(6, 0, 0, 0);
                        label.TextWrapping = TextWrapping.NoWrap;
                        label.IsHitTestVisible = false;
                        //O texto pode passar por cima dos próximos segmentos do mesmo bloco
                        var canvas = new Canvas();
                        canvas.IsHitTestVisible = false;
                        canvas.VerticalAlignment = VerticalAlignment.Center;
                        Canvas.SetTop(label, -8);
                        canvas.Children.Add(label);
                        Panel.SetZIndex(segment, 1);
                        segment.Child = canvas;
                    }
                    Task clicked = task;
                    segment.MouseLeftButtonUp += (s, ev) => HourSlot_Click(clicked);
                }
                hoursBar.Children.Add(segment);
            }
            txtHoursUsed.Text = String.Format(EnglishStrings.ScheduleHoursUsed.Translate(), Appointments.Count);
        }

        private Border CreateTaskCard(Task task, bool isJob)
        {
            int hoursToday = Appointments.Count(a => a == task);

            var card = new Border();
            card.Style = (Style)FindResource("CardBorderStyle");
            card.Margin = new Thickness(6, 0, 6, 12);

            var root = new StackPanel();
            card.Child = root;

            //Cabeçalho: ícone, tipo, nome e pill de horas
            var header = new DockPanel();
            header.LastChildFill = true;

            var iconBox = new Border();
            iconBox.Width = 36;
            iconBox.Height = 36;
            iconBox.CornerRadius = new CornerRadius(8);
            iconBox.Background = new SolidColorBrush(task.Color);
            iconBox.Margin = new Thickness(0, 0, 10, 0);
            var icon = new Path();
            icon.Data = (Geometry)FindResource(IconKey(task));
            icon.Stroke = FindBrush("YtOnAccentBrush");
            icon.StrokeThickness = 2;
            icon.StrokeLineJoin = PenLineJoin.Round;
            icon.StrokeStartLineCap = PenLineCap.Round;
            icon.StrokeEndLineCap = PenLineCap.Round;
            var iconCanvas = new Canvas();
            iconCanvas.Width = 24;
            iconCanvas.Height = 24;
            iconCanvas.Children.Add(icon);
            var viewbox = new Viewbox();
            viewbox.Width = 18;
            viewbox.Height = 18;
            viewbox.Child = iconCanvas;
            iconBox.Child = viewbox;
            DockPanel.SetDock(iconBox, Dock.Left);
            header.Children.Add(iconBox);

            var pill = new Border();
            pill.Style = (Style)FindResource(hoursToday > 0 ? "PillSoftStyle" : "PillStyle");
            var pillText = new TextBlock();
            pillText.Style = (Style)FindResource("InheritTextStyle");
            pillText.Text = String.Format(EnglishStrings.ScheduleHoursToday.Translate(), hoursToday);
            pill.Child = pillText;
            pill.Margin = new Thickness(8, 0, 0, 0);
            DockPanel.SetDock(pill, Dock.Right);
            header.Children.Add(pill);

            var titles = new StackPanel();
            titles.VerticalAlignment = VerticalAlignment.Center;
            var typeText = new TextBlock();
            typeText.Text = TypeLabel(task);
            typeText.FontWeight = FontWeights.Bold;
            typeText.FontSize = 14;
            titles.Children.Add(typeText);
            var nameText = new TextBlock();
            nameText.Style = (Style)FindResource("MutedTextStyle");
            nameText.Text = task.Name;
            nameText.TextTrimming = TextTrimming.CharacterEllipsis;
            nameText.ToolTip = task.Name;
            if (!isJob) //No Trabalhar o nome repetiria o tipo
                titles.Children.Add(nameText);
            header.Children.Add(titles);
            root.Children.Add(header);

            //Barra de progresso
            if (!isJob)
            {
                int total = task.HoursToComplete + task.ExtraHours;
                double ratio = total > 0 ? Math.Min(1.0, Math.Max(0.0, (double)task.HoursPutIn / total)) : 1.0;
                var track = new Grid();
                track.Height = 6;
                track.Margin = new Thickness(0, 12, 0, 0);
                track.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(ratio, GridUnitType.Star) });
                track.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0 - ratio, GridUnitType.Star) });
                var back = new Border();
                back.CornerRadius = new CornerRadius(3);
                back.Background = FindBrush("YtDarkCardHoverBrush");
                Grid.SetColumnSpan(back, 2);
                track.Children.Add(back);
                var fill = new Border();
                fill.CornerRadius = new CornerRadius(3);
                fill.Background = FindBrush("YtGainBrush");
                track.Children.Add(fill);
                root.Children.Add(track);
            }

            //Status
            var status = new TextBlock();
            status.Style = (Style)FindResource("MutedTextStyle");
            status.Margin = new Thickness(0, 8, 0, 0);
            if (isJob)
                status.Text = EnglishStrings.ScheduleJobStatus.Translate();
            else if (task.IsCompleted)
            {
                status.Text = EnglishStrings.ScheduleTaskDone.Translate();
                status.Foreground = FindBrush("YtGainBrush");
            }
            else
                status.Text = String.Format(EnglishStrings.ScheduleHoursRemaining.Translate(), task.HoursLeft);
            root.Children.Add(status);

            //Botões
            var buttons = new WrapPanel();
            buttons.Margin = new Thickness(0, 10, 0, 0);
            if (isJob)
            {
                var quit = NewSmallButton(EnglishStrings.QuitJobHeader.Translate());
                quit.Click += (s, ev) => AskQuitJob();
                buttons.Children.Add(quit);
            }
            else
            {
                var plus = NewSmallButton("+1h");
                plus.IsEnabled = !task.IsCompleted && Appointments.Count < 15;
                plus.Click += (s, ev) =>
                {
                    if (!task.IsCompleted)
                        AddAppointment(task);
                };
                buttons.Children.Add(plus);

                var minus = NewSmallButton("−1h");
                minus.IsEnabled = hoursToday > 0;
                minus.Click += (s, ev) =>
                {
                    if (Appointments.Contains(task))
                        RemoveLastAppointment(task);
                };
                buttons.Children.Add(minus);

                var remove = NewSmallButton(EnglishStrings.ScheduleRemove.Translate());
                remove.Style = (Style)FindResource("GhostButtonStyle");
                remove.MinHeight = 0;
                remove.FontSize = 12;
                remove.Padding = new Thickness(9, 3, 9, 3);
                remove.Click += (s, ev) => AskRemoveTask(task);
                buttons.Children.Add(remove);
            }
            root.Children.Add(buttons);

            return card;
        }

        private Button NewSmallButton(string text)
        {
            var button = new Button();
            button.Style = (Style)FindResource("SmallButtonStyle");
            button.Content = text;
            button.Margin = new Thickness(0, 0, 6, 0);
            return button;
        }

        //Remove a última hora desta tarefa na agenda de hoje (mesma regra do RemoveAppointment(task, false))
        private void RemoveLastAppointment(Task task)
        {
            int index = Appointments.LastIndexOf(task);
            if (index < 0)
                return;
            //RemoveAppointment tira a primeira ocorrência; para tirar a última, move-a antes
            Appointments.RemoveAt(index);
            Appointments.Insert(Appointments.IndexOf(task) < 0 ? index : Appointments.IndexOf(task), task);
            RemoveAppointment(task, false);
        }

        //Devolve quantas tarefas foram concluídas (para o resumo do dia)
        private int CleanTodoList()
        {
            int completed = 0;
            foreach (Task task in Player.Current.TasksInProgress)
            {
                if (task.IsCompleted)
                {
                    completed++;
                    var studyTask = task as Study;
                    if (studyTask != null)
                    {
                        switch (studyTask.SkillModifierType)
                        {
                            case (SkillModifierType.Shooting):
                                Player.Current.ShootingSkill += studyTask.SkillModifier;
                                break;

                            case (SkillModifierType.PostProduction):
                                Player.Current.PostProductionSkill += studyTask.SkillModifier;
                                break;

                            case (SkillModifierType.VideoAttribute):
                                Player.Current.VideoAttributePoints += studyTask.SkillModifier;
                                break;

                            case (SkillModifierType.ViewQuality):
                                Player.Current.CanViewQualityBeforeUpload = true;
                                break;
                        }
                    }

                    var shootTask = task as ShootVideo;
                    if (shootTask != null)
                    {
                        Player.Current.Videos.Add(shootTask.Video);
                    }

                    var editTask = task as EditVideo;
                    if (editTask != null)
                    {
                        editTask.Video.HasBeenEdited = true;
                    }
                }
            }

            Player.Current.TasksInProgress.RemoveAll(t => t.IsCompleted);
            return completed;
        }

        public void NewDay()
        {
            _moneyAtStartOfDay = Player.Current.Money;
            var today = new PeriodStats();

            Player.Current.Money -= 50; //Living Expenses
            Player.Current.Money -= Math.Max(0, Player.Current.CostOfLivingExtra);
            today.Spent += 50 + Math.Max(0, Player.Current.CostOfLivingExtra);

            if(StoreItems.Current.Loan.Purchased)
            {
                Player.Current.LoanPayOff -= StoreItems.Current.Loan.AdditionalCost;
                Player.Current.Money -= StoreItems.Current.Loan.AdditionalCost;
                today.Spent += StoreItems.Current.Loan.AdditionalCost;

                if (Player.Current.LoanPayOff <= 0)
                {
                    StoreItems.Current.Loan.Purchased = false;
                }
            }

            Appointments = new List<Task>();
            if (!Player.Current.QuitJob)
            {
                Player.Current.Money += 100;
                today.Earned += 100;
                var job = new Job();
                for (int i = 0; i <= job.HoursToComplete; i++)
                {
                    AddAppointment(job);
                }
                if (Player.Current.HasPromotion)
                {
                    Player.Current.Money += 50;
                    today.Earned += 50;
                    AddAppointment(job);
                    AddAppointment(job);
                }
                if (!Player.Current.QuitJob && Player.Current.Overtime)
                {
                    AddAppointment(job);
                    AddAppointment(job);
                    AddAppointment(job);
                    AddAppointment(job);
                    Player.Current.Overtime = false;
                }
            }
            today.TasksCompleted = CleanTodoList();
            Update();

            RunIterations(today);
        }

        //Roda na thread da tela: a simulação ficou rápida (lotes) e assim não disputa
        //Player/Rivais/RandomHelpers com a tela, e erros vão para o Crash Report
        private void RunIterations(PeriodStats today)
        {
            bool gameOver = false;
            try
            {
                foreach (var channel in Player.Current.Channels)
                {
                    if (channel != Channel.UnreleasedVideos)
                    {
                        double dailyIncome = 0;
                        double dailyExpenses = 0;
                        int subscribersBefore = channel.Subscribers;
                        var channelToday = today.GetChannel(channel.Name);
                        foreach (var video in channel.Videos)
                        {
                            int viewsBefore = video.Views;
                            double moneyBefore = Player.Current.Money;
                            double incomeBefore = dailyIncome;
                            double expensesBefore = dailyExpenses;
                            VideoViewer.Iteration(channel, video, ref dailyIncome, ref dailyExpenses);
                            //Crowdfunding entra direto no dinheiro, fora do dailyIncome
                            double crowdfunding = Math.Max(0, Player.Current.Money - moneyBefore);
                            today.Earned += crowdfunding;
                            video.Income += (dailyIncome - incomeBefore) + crowdfunding;
                            video.Expenses += dailyExpenses - expensesBefore;
                            if (video.Iterations == 1)
                            {
                                dailyExpenses += video.Cost;
                                dailyExpenses += video.OnceOffCost;
                                today.VideosPosted++;
                            }

                            long newViews = Math.Max(0, video.Views - viewsBefore);
                            channelToday.Views += newViews;
                            channelToday.AddVideoViews(video.Name, newViews);
                        }

                        //Channel stats
                        channel.Income += dailyIncome;
                        channel.SubscribersOverTime.Add(channel.Subscribers);
                        channel.IncomeOverTime.Add(dailyIncome);
                        channel.ExpensesOverTime.Add(dailyExpenses);

                        //Player costs
                        Player.Current.Money += (dailyIncome - dailyExpenses);

                        channelToday.NewSubscribers = channel.Subscribers - subscribersBefore;
                        today.Views += channelToday.Views;
                        today.NewSubscribers += channelToday.NewSubscribers;
                        today.Earned += dailyIncome;
                        today.Spent += dailyExpenses;
                    }
                }

                //Rivais não têm mais vídeos: crescem pelo Top TubeStars (Ranking.NewDay)

                //Game over: o dia para aqui (sem troféu, eventos ou atualização de tela)
                if (Player.Current.Money < 0)
                {
                    gameOver = true;
                    if (GameExit != null)
                        GameExit();
                    return;
                }

                if (Player.Current.Money - _moneyAtStartOfDay >= 1000)
                {
                    TrophyManager.UnlockTrophy(Trophy.Bunsen);
                }

                //O primeiro NewDay do jogo novo (Iterations -1) só prepara o Dia 1: não entra no resumo
                if (Player.Current.Iterations >= 0)
                    Player.Current.RecordDay(today);

                Player.Current.Iterations++;
                CheckPeriodEnd();
                UpdateSummary();
            }
            finally
            {
                //Mesmo com erro (vai para o Crash Report), a tela precisa ser liberada
                if (!gameOver && NewDayCompleted != null)
                    NewDayCompleted();
            }
        }
    }
}
