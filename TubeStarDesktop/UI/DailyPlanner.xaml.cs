using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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
            txtAddTask.Text = EnglishStrings.AddTask.Translate();
        }

        private void AppointmentBlock_Click(object sender, EventArgs e)
        {
            var appointmentBlock = sender as AppointmentBlock;
            if (appointmentBlock != null && appointmentBlock.Task != null)
            {
                //Special: quit job
                if (appointmentBlock.Task.TaskType == TaskType.Job)
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
                else if (appointmentBlock.Task != null)
                {
                    RemoveAppointment(appointmentBlock.Task, false);
                }
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            AddTaskDialog dialog = new AddTaskDialog();
            dialog.TaskClick += (t) =>
            {
                AddTask(t);
            };
            dialog.ShowDialog();
        }

        private void AddTask(Task task)
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
            //Todo Tasks
            tasksPanel.Children.Clear();
            foreach (var task in Player.Current.TasksInProgress)
            {
                TodoTask appointmentTask = new TodoTask(task);
                appointmentTask.TaskClick += (s, ev) =>
                {
                    if (!task.IsCompleted)
                    {
                        AddAppointment(task);
                    }
                };
                appointmentTask.CancelTaskClick += (s, ev) =>
                {
                    bool isStudy = task is Study;
                    CustomMessageBox.ShowDialog(EnglishStrings.RemoveTask.Translate(), String.Format("{0} {1}", EnglishStrings.AreYouSure.Translate(), isStudy ? String.Format("\n{0}", EnglishStrings.NoRefunds.Translate()) : ""), MessagePicture.Question, (result) =>
                    {
                        if (result == true)
                        {
                            RemoveAppointment(task, true);
                        }
                    });
                };
                tasksPanel.Children.Add(appointmentTask);
                appointmentTask.UpdateText();
            }

            //Appointments
            foreach (AppointmentBlock appointment in appointmentPanel.Children)
            {
                appointment.Task = null;
            }

            for (int i = 0; i < Appointments.Count; i++)
            {
                (appointmentPanel.Children[i] as AppointmentBlock).Task = Appointments[i];
            }
        }

        private void CleanTodoList()
        {
            foreach (Task task in Player.Current.TasksInProgress)
            {
                if (task.IsCompleted)
                {
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
        }

        public void NewDay()
        {
            _moneyAtStartOfDay = Player.Current.Money;

            Player.Current.Money -= 50; //Living Expenses
            Player.Current.Money -= Math.Max(0, Player.Current.CostOfLivingExtra);

            if(StoreItems.Current.Loan.Purchased)
            {
                Player.Current.LoanPayOff -= StoreItems.Current.Loan.AdditionalCost;
                Player.Current.Money -= StoreItems.Current.Loan.AdditionalCost;

                if (Player.Current.LoanPayOff <= 0)
                {
                    StoreItems.Current.Loan.Purchased = false;
                }
            }

            Appointments = new List<Task>();
            if (!Player.Current.QuitJob)
            {
                Player.Current.Money += 100;
                var job = new Job();
                for (int i = 0; i <= job.HoursToComplete; i++)
                {
                    AddAppointment(job);
                }
                if (Player.Current.HasPromotion)
                {
                    Player.Current.Money += 50;
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
            CleanTodoList();
            Update();

            RunIterations();
        }

        //Roda na thread da tela: a simulação ficou rápida (lotes) e assim não disputa
        //Player/Rivais/RandomHelpers com a tela, e erros vão para o Crash Report
        private void RunIterations()
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
                        foreach (var video in channel.Videos)
                        {
                            VideoViewer.Iteration(channel, video, ref dailyIncome, ref dailyExpenses);
                            if (video.Iterations == 1)
                            {
                                dailyExpenses += video.Cost;
                                dailyExpenses += video.OnceOffCost;
                            }
                        }

                        //Channel stats
                        channel.Income += dailyIncome;
                        channel.SubscribersOverTime.Add(channel.Subscribers);
                        channel.IncomeOverTime.Add(dailyIncome);
                        channel.ExpensesOverTime.Add(dailyExpenses);

                        //Player costs
                        Player.Current.Money += (dailyIncome - dailyExpenses);
                    }
                }

                double rivalDailyIncome = 0;
                double refDailyExpenses = 0;
                foreach (var rival in Rivals.Current.All)
                {
                    if (rival.Channel != null && rival.Channel.Videos != null)
                    {
                        foreach (var video in rival.Channel.Videos.ToList())
                        {
                            VideoViewer.Iteration(rival.Channel, video, ref rivalDailyIncome, ref refDailyExpenses);
                        }
                    }
                }

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

                Player.Current.Iterations++;
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
