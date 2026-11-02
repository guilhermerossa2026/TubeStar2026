using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace TubeStar
{
    /// <summary>
    /// Calendário: dias da semana atual, totais da semana até agora e histórico de resumos
    /// </summary>
    public partial class CalendarDialog : ChildWindow
    {
        public CalendarDialog()
        {
            InitializeComponent();
            Caption = EnglishStrings.CalendarTitle.Translate();
            txtThisWeek.Text = EnglishStrings.CalendarThisWeek.Translate();
            txtHistory.Text = EnglishStrings.CalendarPastSummaries.Translate();
            txtNoHistory.Text = EnglishStrings.CalendarNoHistory.Translate();
            txtWeekEmpty.Text = EnglishStrings.SummaryNoDayYet.Translate();
            btnOk.Content = EnglishStrings.Ok.Translate();

            //Mesma conta do cabeçalho (ano de 112 dias, semana de 7)
            int day = Math.Max(0, Player.Current.Iterations);
            int dayOfYear = day % 112 + 1;
            int week = (dayOfYear - 1) / 7 + 1;
            int year = day / 112 + 1;
            txtDate.Text = String.Format(EnglishStrings.GameDate.Translate(), year, week, dayOfYear);

            PopulateWeekDays(dayOfYear);
            PopulateThisWeek();
            PopulateHistory();
        }

        private static Brush FindBrush(string key)
        {
            return (Brush)Application.Current.FindResource(key);
        }

        private void PopulateWeekDays(int dayOfYear)
        {
            int firstDay = (dayOfYear - 1) / 7 * 7 + 1;
            for (int d = firstDay; d < firstDay + 7; d++)
            {
                bool isToday = d == dayOfYear;
                bool isPast = d < dayOfYear;

                var box = new Border
                {
                    Margin = new Thickness(4),
                    Padding = new Thickness(6, 10, 6, 10),
                    CornerRadius = new CornerRadius(8),
                    BorderThickness = new Thickness(1),
                    Background = isToday ? FindBrush("YtRedBrush") : FindBrush("YtDarkCardBrush"),
                    BorderBrush = isToday ? FindBrush("YtRedBrush") : FindBrush("YtDarkBorderBrush"),
                    Opacity = isPast ? 0.55 : 1.0
                };

                var panel = new StackPanel();
                panel.Children.Add(new TextBlock
                {
                    Text = String.Format(EnglishStrings.CalendarDay.Translate(), d),
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = isToday ? FindBrush("YtOnAccentBrush") : FindBrush("YtTextPrimaryBrush")
                });
                panel.Children.Add(new TextBlock
                {
                    Text = isToday ? EnglishStrings.CalendarToday.Translate() : (isPast ? "✓" : " "),
                    FontSize = 11,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = isToday ? FindBrush("YtOnAccentBrush") : FindBrush("YtTextSecondaryBrush")
                });
                box.Child = panel;
                weekDays.Children.Add(box);
            }
        }

        private void PopulateThisWeek()
        {
            var week = Player.Current.CurrentWeek;
            txtWeekEmpty.Visibility = week == null ? Visibility.Visible : Visibility.Collapsed;
            if (week == null)
                return;

            foreach (var kpi in DailyPlanner.CreateSummaryKpis(week))
                weekKpis.Children.Add(kpi);
        }

        //Mais recentes primeiro; clicar abre o resumo completo
        private void PopulateHistory()
        {
            var history = Player.Current.PastSummaries;
            bool empty = history == null || history.Count == 0;
            txtNoHistory.Visibility = empty ? Visibility.Visible : Visibility.Collapsed;
            if (empty)
                return;

            for (int i = history.Count - 1; i >= 0; i--)
            {
                var summary = history[i];

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                grid.Children.Add(new TextBlock { Text = summary.Title, VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.SemiBold });

                var views = new TextBlock
                {
                    Text = String.Format("{0} {1}", summary.Views.ToString("N0"), EnglishStrings.Views.Translate().ToLower()),
                    Foreground = FindBrush("YtTextSecondaryBrush"),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(16, 0, 16, 0)
                };
                Grid.SetColumn(views, 1);
                grid.Children.Add(views);

                var balance = new TextBlock
                {
                    Text = summary.Balance.ToCurrencyString(),
                    FontWeight = FontWeights.SemiBold,
                    Foreground = FindBrush(summary.Balance < 0 ? "YtLossBrush" : "YtGainBrush"),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(balance, 2);
                grid.Children.Add(balance);

                var row = new Border { Style = (Style)FindResource("TableRowStyle"), Child = grid, Cursor = Cursors.Hand };
                row.MouseLeftButtonUp += (s, e) => new PeriodSummaryDialog(summary.Title, summary).ShowDialog();
                historyRows.Children.Add(row);
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
