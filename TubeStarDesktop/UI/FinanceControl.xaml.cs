using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TubeStar
{
    //Tela Finanças (Visão geral): números, gráfico de patrimônio, entradas/saídas da semana e extrato
    public partial class FinanceControl : UserControl
    {
        private static readonly CultureInfo Culture = new CultureInfo("en-US");
        private const int StatementWeeks = 8;
        private const int ChartMaxPoints = 24;
        private const double Epsilon = 0.005;

        private readonly List<double> _chartValues = new List<double>();
        private bool _hasHistory;

        public FinanceControl()
        {
            InitializeComponent();
            Translate();
        }

        private void Translate()
        {
            txtTitle.Text = EnglishStrings.NavFinance.Translate();
            txtLead.Text = EnglishStrings.FinanceLead.Translate();
            txtTabOverview.Text = EnglishStrings.FinanceTabOverview.Translate();
            txtTabInvestments.Text = EnglishStrings.FinanceTabInvestments.Translate();
            txtTabMarket.Text = EnglishStrings.FinanceTabMarket.Translate();
            txtTabCollectibles.Text = EnglishStrings.FinanceTabCollectibles.Translate();
            string soon = EnglishStrings.Soon.Translate();
            txtSoon1.Text = soon;
            txtSoon2.Text = soon;
            txtSoon3.Text = soon;
            txtCashLabel.Text = EnglishStrings.FinanceCash.Translate();
            txtInvestedLabel.Text = EnglishStrings.FinanceInvested.Translate();
            txtInvestedNote.Text = EnglishStrings.FinanceInvestedNote.Translate();
            txtWeekLabel.Text = EnglishStrings.FinanceWeekResult.Translate();
            txtWeekNote.Text = EnglishStrings.FinanceWeekResultNote.Translate();
            txtYearLabel.Text = EnglishStrings.FinanceProfitYear.Translate();
            txtYearNote.Text = EnglishStrings.FinanceProfitYearNote.Translate();
            txtChartTitle.Text = EnglishStrings.FinanceNetWorth.Translate();
            txtChartEmpty.Text = EnglishStrings.FinanceChartEmpty.Translate();
            txtCompTitle.Text = EnglishStrings.FinanceComposition.Translate();
            txtIncomeTitle.Text = EnglishStrings.FinanceIncomeWeek.Translate();
            txtSpendingTitle.Text = EnglishStrings.FinanceSpendingWeek.Translate();
            txtStatementTitle.Text = EnglishStrings.FinanceStatement.Translate();
            txtNoWeeks.Text = EnglishStrings.FinanceNoWeeks.Translate();
        }

        private static Brush FindBrush(string key)
        {
            return (Brush)Application.Current.FindResource(key);
        }

        private static string Money(double value)
        {
            return Math.Round(value, 2).ToString("C", Culture);
        }

        private static Brush ResultBrush(double value)
        {
            if (value > Epsilon)
                return FindBrush("YtGainBrush");
            if (value < -Epsilon)
                return FindBrush("YtLossBrush");
            return FindBrush("YtTextPrimaryBrush");
        }

        //Semana ou ano? Saves antigos não têm IsYear: o título do ano não tem o "·" do título da semana
        private static bool IsYearSummary(PeriodStats summary)
        {
            if (summary.IsYear)
                return true;
            return !String.IsNullOrEmpty(summary.Title) && !summary.Title.Contains("·");
        }

        //Semanas fechadas, da mais antiga para a mais nova
        private static List<PeriodStats> ClosedWeeks()
        {
            var result = new List<PeriodStats>();
            var history = Player.Current.PastSummaries;
            if (history == null)
                return result;
            foreach (var summary in history)
            {
                if (summary != null && !IsYearSummary(summary))
                    result.Add(summary);
            }
            return result;
        }

        public void Update()
        {
            var player = Player.Current;
            var closed = ClosedWeeks();

            //Semana em andamento; logo depois do fechamento (ainda vazia), mostra a última fechada
            bool currentIsOpen = player.CurrentWeek != null;
            PeriodStats week = player.CurrentWeek;
            if (week == null)
                week = closed.Count > 0 ? closed[closed.Count - 1] : new PeriodStats();

            txtCash.Text = Money(player.Money);
            txtInvested.Text = Money(0);

            double result = week.Balance;
            txtWeek.Text = Money(result);
            txtWeek.Foreground = ResultBrush(result);

            double yearProfit = player.CurrentYear != null ? player.CurrentYear.Balance : 0;
            txtYear.Text = Money(yearProfit);
            txtYear.Foreground = ResultBrush(yearProfit);

            //Pontos do gráfico: patrimônio no fechamento das semanas (as antigas, sem valor, ficam de fora) + agora
            _chartValues.Clear();
            foreach (var closedWeek in closed)
            {
                if (closedWeek.NetWorthEnd.HasValue)
                    _chartValues.Add(closedWeek.NetWorthEnd.Value);
            }
            _hasHistory = _chartValues.Count > 0;
            _chartValues.Add(player.GetNetWorth());
            if (_chartValues.Count > ChartMaxPoints)
                _chartValues.RemoveRange(0, _chartValues.Count - ChartMaxPoints);
            DrawChart();
            UpdateComposition();

            incomePanel.Children.Clear();
            FillIncome(incomePanel, week, currentIsOpen);
            spendingPanel.Children.Clear();
            FillSpending(spendingPanel, week);

            UpdateStatement(closed);
        }

        //---------- Gráfico ----------

        private void ChartCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawChart();
        }

        private void DrawChart()
        {
            chartCanvas.Children.Clear();
            txtChartEmpty.Visibility = _hasHistory ? Visibility.Collapsed : Visibility.Visible;

            double width = chartCanvas.ActualWidth;
            double height = chartCanvas.ActualHeight;
            //Sem semana fechada: o texto "mais pontos aparecem..." fica embaixo e o gráfico mostra só o ponto de agora
            txtChartEmpty.Visibility = _hasHistory ? Visibility.Collapsed : Visibility.Visible;
            if (_chartValues.Count < 1 || width < 120 || height < 60)
                return;

            double min = _chartValues[0];
            double max = _chartValues[0];
            foreach (double value in _chartValues)
            {
                if (value < min) min = value;
                if (value > max) max = value;
            }
            //Faixa mínima (±5% do valor ou ±$100) e limites "bonitos" para os rótulos não se repetirem
            double range = max - min;
            double minRange = Math.Max(Math.Abs(max) * 0.1, 200);
            if (range < minRange)
            {
                double middle = (min + max) / 2;
                min = middle - minRange / 2;
                max = middle + minRange / 2;
            }
            else
            {
                double pad = range * 0.1;
                min -= pad;
                max += pad;
            }
            double step = NiceStep((max - min) / 3);
            while (Math.Floor(min / step) * step + step * 3 < max)
                step = NiceStep(step * 1.01);
            min = Math.Floor(min / step) * step;
            max = min + step * 3;
            range = max - min;

            double left = 70, right = 12, top = 10, bottom = 10;
            double plotWidth = width - left - right;
            double plotHeight = height - top - bottom;

            //Linhas de grade com o valor ao lado
            for (int i = 0; i < 4; i++)
            {
                double y = top + plotHeight * i / 3;
                var line = new Line { X1 = left, X2 = left + plotWidth, Y1 = y, Y2 = y, Stroke = FindBrush("YtDarkBorderBrush"), StrokeThickness = 1 };
                chartCanvas.Children.Add(line);

                double labelValue = max - range * i / 3;
                var label = new TextBlock { Text = Math.Round(labelValue).ToString("C0", Culture), Foreground = FindBrush("YtTextSecondaryBrush"), FontSize = 10, Width = left - 8, TextAlignment = TextAlignment.Right };
                Canvas.SetLeft(label, 0);
                Canvas.SetTop(label, y - 7);
                chartCanvas.Children.Add(label);
            }

            var polyline = new Polyline { Stroke = FindBrush("YtRedBrush"), StrokeThickness = 2, StrokeLineJoin = PenLineJoin.Round };
            double lastX = 0, lastY = 0;
            for (int i = 0; i < _chartValues.Count; i++)
            {
                double x = _chartValues.Count == 1 ? left + plotWidth / 2 : left + plotWidth * i / (_chartValues.Count - 1);
                double y = top + plotHeight * (max - _chartValues[i]) / range;
                polyline.Points.Add(new Point(x, y));
                lastX = x;
                lastY = y;
            }
            chartCanvas.Children.Add(polyline);

            //Ponto do patrimônio de agora
            var dot = new Ellipse { Width = 8, Height = 8, Fill = FindBrush("YtRedBrush") };
            Canvas.SetLeft(dot, lastX - 4);
            Canvas.SetTop(dot, lastY - 4);
            chartCanvas.Children.Add(dot);
        }

        //Menor passo "bonito" (1, 2 ou 5 x 10^n) que seja maior ou igual ao valor
        private static double NiceStep(double raw)
        {
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(raw)));
            double[] bases = { 1, 2, 5, 10 };
            foreach (double b in bases)
            {
                if (b * magnitude >= raw)
                    return b * magnitude;
            }
            return 10 * magnitude;
        }

        //Barra de composição: dinheiro, empréstimo e dívida fiscal (os dois últimos só se existirem)
        private void UpdateComposition()
        {
            compBar.Children.Clear();
            compBar.ColumnDefinitions.Clear();
            compLegend.Children.Clear();

            var player = Player.Current;
            var names = new List<string>();
            var values = new List<double>();
            var brushes = new List<Brush>();

            names.Add(EnglishStrings.FinanceCompCash.Translate());
            //Dinheiro negativo: a barra usa o valor absoluto em vermelho e a legenda mostra o valor real
            bool negativeCash = player.Money < 0;
            values.Add(Math.Abs(player.Money));
            brushes.Add(FindBrush(negativeCash ? "YtLossBrush" : "YtGainBrush"));

            double loan = player.GetLoanBalance();
            if (loan > Epsilon)
            {
                names.Add(EnglishStrings.FinanceCompLoan.Translate());
                values.Add(loan);
                brushes.Add(FindBrush("YtLossBrush"));
            }
            if (player.TaxDebt > Epsilon)
            {
                names.Add(EnglishStrings.FinanceCompTax.Translate());
                values.Add(player.TaxDebt);
                brushes.Add(new SolidColorBrush(Color.FromRgb(0xF2, 0xA3, 0x3A)));
            }

            double total = 0;
            foreach (double value in values)
                total += value;

            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] > 0)
                {
                    int column = compBar.ColumnDefinitions.Count;
                    compBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(values[i], GridUnitType.Star) });
                    var segment = new Border { Background = brushes[i] };
                    Grid.SetColumn(segment, column);
                    compBar.Children.Add(segment);
                }

                double percent = total > 0 ? values[i] * 100 / total : 0;
                var legend = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 18, 0) };
                legend.Children.Add(new Border { Width = 10, Height = 10, CornerRadius = new CornerRadius(2), Background = brushes[i], VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) });
                double shown = (i == 0 && negativeCash) ? player.Money : values[i];
                var legendText = new TextBlock { Text = names[i] + " " + Money(shown) + " (" + percent.ToString("0", Culture) + "%)", Style = (Style)FindResource("MutedTextStyle") };
                if (i == 0 && negativeCash)
                    legendText.Foreground = FindBrush("YtLossBrush");
                legend.Children.Add(legendText);
                compLegend.Children.Add(legend);
            }
        }

        //---------- Entradas e saídas ----------

        private static UIElement MakeRow(string label, double value, Brush valueBrush, bool bold, bool indent, bool total)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var text = new TextBlock { Text = label, Margin = new Thickness(indent ? 14 : 0, 0, 8, 0), TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center };
            if (indent)
            {
                text.Foreground = FindBrush("YtTextSecondaryBrush");
                text.FontSize = 12;
            }
            if (bold)
                text.FontWeight = FontWeights.Bold;
            grid.Children.Add(text);

            var amount = new TextBlock { Text = Money(value), Foreground = valueBrush, VerticalAlignment = VerticalAlignment.Center };
            if (indent)
                amount.FontSize = 12;
            if (bold)
                amount.FontWeight = FontWeights.Bold;
            Grid.SetColumn(amount, 1);
            grid.Children.Add(amount);

            var border = new Border { Child = grid, MinHeight = 30, Padding = new Thickness(0, 4, 0, 4), BorderBrush = FindBrush("YtDarkBorderBrush"), BorderThickness = new Thickness(0, total ? 1 : 0, 0, 0) };
            return border;
        }

        private static void AddPlain(Panel panel, string label, double value, bool indent)
        {
            panel.Children.Add(MakeRow(label, value, indent ? FindBrush("YtTextSecondaryBrush") : FindBrush("YtTextPrimaryBrush"), false, indent, false));
        }

        private static void AddEmpty(Panel panel)
        {
            panel.Children.Add(new TextBlock { Text = EnglishStrings.FinanceNoMovement.Translate(), Style = (Style)Application.Current.FindResource("MutedTextStyle"), Margin = new Thickness(0, 4, 0, 4) });
        }

        //Uma linha por canal, emprego e eventos; semanas antigas (sem renda por canal) mostram "Canais" no total
        private static void FillIncome(Panel panel, PeriodStats week, bool useCurrentJob)
        {
            int rows = 0;
            double channelSum = 0;
            foreach (var channel in week.Channels)
            {
                if (channel.Earned > Epsilon)
                {
                    AddPlain(panel, channel.Name, channel.Earned, false);
                    channelSum += channel.Earned;
                    rows++;
                }
            }
            double channelRest = week.ChannelEarned - channelSum;
            if (channelRest > Epsilon)
            {
                AddPlain(panel, EnglishStrings.FinanceChannelsTotal.Translate(), channelRest, false);
                rows++;
            }
            if (week.Salary > Epsilon)
            {
                bool partTime = useCurrentJob && Player.Current.PartTime;
                AddPlain(panel, (partTime ? EnglishStrings.FinancePartTime : EnglishStrings.FinanceJob).Translate(), week.Salary, false);
                rows++;
            }
            if (week.OtherEarned > Epsilon)
            {
                AddPlain(panel, EnglishStrings.FinanceEvents.Translate(), week.OtherEarned, false);
                rows++;
            }
            if (rows == 0)
                AddEmpty(panel);

            panel.Children.Add(MakeRow(EnglishStrings.FinanceIncome.Translate(), week.Earned, FindBrush("YtGainBrush"), true, false, true));
        }

        private static void AddGroup(Panel panel, string name, double total)
        {
            panel.Children.Add(MakeRow(name, total, FindBrush("YtTextPrimaryBrush"), true, false, false));
        }

        //Saídas agrupadas; o que não tem categoria (eventos, semanas antigas) cai em "Outros"
        private static void FillSpending(Panel panel, PeriodStats week)
        {
            double fixedCosts = week.LivingCost + week.TaxCost + week.LoanCost;
            double channelCosts = week.VideoCost + week.VideoPromotionCost;
            double other = week.Spent - fixedCosts - channelCosts - week.StoreCost - week.StudyCost;

            int groups = 0;
            if (fixedCosts > Epsilon)
            {
                AddGroup(panel, EnglishStrings.FinanceFixed.Translate(), fixedCosts);
                if (week.LivingCost > Epsilon) AddPlain(panel, EnglishStrings.FinanceFixedLiving.Translate(), week.LivingCost, true);
                if (week.TaxCost > Epsilon) AddPlain(panel, EnglishStrings.FinanceFixedTax.Translate(), week.TaxCost, true);
                if (week.LoanCost > Epsilon) AddPlain(panel, EnglishStrings.FinanceFixedLoan.Translate(), week.LoanCost, true);
                groups++;
            }
            if (channelCosts > Epsilon)
            {
                AddGroup(panel, EnglishStrings.FinanceChannelGroup.Translate(), channelCosts);
                if (week.VideoCost > Epsilon) AddPlain(panel, EnglishStrings.FinanceVideoCost.Translate(), week.VideoCost, true);
                if (week.VideoPromotionCost > Epsilon) AddPlain(panel, EnglishStrings.FinancePromotion.Translate(), week.VideoPromotionCost, true);
                groups++;
            }
            if (week.StoreCost > Epsilon)
            {
                AddGroup(panel, EnglishStrings.FinanceStoreGroup.Translate(), week.StoreCost);
                groups++;
            }
            if (week.StudyCost > Epsilon)
            {
                AddGroup(panel, EnglishStrings.FinanceStudiesGroup.Translate(), week.StudyCost);
                groups++;
            }
            if (other > Epsilon)
            {
                AddGroup(panel, EnglishStrings.FinanceOther.Translate(), other);
                groups++;
            }
            if (groups == 0)
                AddEmpty(panel);

            panel.Children.Add(MakeRow(EnglishStrings.FinanceSpending.Translate(), week.Spent, FindBrush("YtTextPrimaryBrush"), true, false, true));
        }

        //---------- Extrato ----------

        private void UpdateStatement(List<PeriodStats> closed)
        {
            statementPanel.Children.Clear();
            txtNoWeeks.Visibility = closed.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            if (closed.Count > 0)
                statementPanel.Children.Add(CreateStatementColumns());

            int shown = 0;
            for (int i = closed.Count - 1; i >= 0 && shown < StatementWeeks; i--, shown++)
                statementPanel.Children.Add(CreateStatementItem(closed[i]));
        }

        private static Grid CreateStatementGrid()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
            return grid;
        }

        //Cabeçalho das colunas do extrato (mesmas colunas e margens das linhas)
        private static UIElement CreateStatementColumns()
        {
            var header = CreateStatementGrid();
            var week = new TextBlock { Text = EnglishStrings.FinanceColWeek.Translate(), VerticalAlignment = VerticalAlignment.Center };
            header.Children.Add(week);
            AddHeaderCell(header, 1, EnglishStrings.FinanceColIn.Translate(), FindBrush("YtTextSecondaryBrush"), false);
            AddHeaderCell(header, 2, EnglishStrings.FinanceColOut.Translate(), FindBrush("YtTextSecondaryBrush"), false);
            AddHeaderCell(header, 3, EnglishStrings.FinanceResult.Translate(), FindBrush("YtTextSecondaryBrush"), false);
            week.Foreground = FindBrush("YtTextSecondaryBrush");
            foreach (UIElement child in header.Children)
            {
                var text = child as TextBlock;
                if (text != null)
                    text.FontSize = 12;
            }
            return new Border { Child = header, Height = 28, Padding = new Thickness(10, 0, 10, 0), BorderBrush = FindBrush("YtDarkBorderBrush"), BorderThickness = new Thickness(0, 0, 0, 1) };
        }

        //"Semana N" (o ano só aparece se for de um ano anterior); se não achar os números, usa o título salvo
        private static string StatementWeekTitle(PeriodStats week)
        {
            if (String.IsNullOrEmpty(week.Title))
                return "-";
            var match = System.Text.RegularExpressions.Regex.Match(week.Title, @"(\d+)\D+(\d+)");
            if (!match.Success)
                return week.Title;
            int weekNumber = Int32.Parse(match.Groups[1].Value, Culture);
            int yearNumber = Int32.Parse(match.Groups[2].Value, Culture);
            int day = Player.Current.Iterations;
            int currentYear = day > 0 ? (day - 1) / 112 + 1 : 1;
            if (yearNumber < currentYear)
                return String.Format(EnglishStrings.FinanceWeekYearNumber.Translate(), weekNumber, yearNumber);
            return String.Format(EnglishStrings.FinanceWeekNumber.Translate(), weekNumber);
        }

        private UIElement CreateStatementItem(PeriodStats week)
        {
            var item = new StackPanel();

            var header = CreateStatementGrid();

            var title = new TextBlock { Text = StatementWeekTitle(week), FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center };
            header.Children.Add(title);
            AddHeaderCell(header, 1, Money(week.Earned), FindBrush("YtGainBrush"), false);
            AddHeaderCell(header, 2, Money(week.Spent), FindBrush("YtTextPrimaryBrush"), false);
            AddHeaderCell(header, 3, Money(week.Balance), ResultBrush(week.Balance), true);
            var arrow = new TextBlock { Text = "▾", Foreground = FindBrush("YtTextSecondaryBrush"), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(arrow, 4);
            header.Children.Add(arrow);

            var row = new Border { Style = (Style)FindResource("TableRowStyle"), Height = 40, Cursor = Cursors.Hand, Child = header };
            item.Children.Add(row);

            var detail = new Border { Visibility = Visibility.Collapsed, Background = FindBrush("YtDarkSurfaceBrush"), Padding = new Thickness(16, 8, 16, 12) };
            item.Children.Add(detail);

            row.MouseLeftButtonUp += (s, e) =>
            {
                if (detail.Visibility == Visibility.Visible)
                {
                    detail.Visibility = Visibility.Collapsed;
                    arrow.Text = "▾";
                    return;
                }
                if (detail.Child == null)
                    detail.Child = CreateDetail(week);
                detail.Visibility = Visibility.Visible;
                arrow.Text = "▴";
            };

            return item;
        }

        private static void AddHeaderCell(Grid header, int column, string text, Brush brush, bool bold)
        {
            var cell = new TextBlock { Text = text, Foreground = brush, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };
            if (bold)
                cell.FontWeight = FontWeights.Bold;
            Grid.SetColumn(cell, column);
            header.Children.Add(cell);
        }

        //Detalhe da semana: mesmo agrupamento das saídas, empréstimo recebido como linha neutra
        private static UIElement CreateDetail(PeriodStats week)
        {
            var root = new StackPanel();

            var columns = new Grid();
            columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var income = new StackPanel { Margin = new Thickness(0, 0, 14, 0) };
            income.Children.Add(new TextBlock { Text = EnglishStrings.FinanceIncome.Translate(), FontWeight = FontWeights.Bold, FontSize = 12, Foreground = FindBrush("YtTextSecondaryBrush"), Margin = new Thickness(0, 0, 0, 2) });
            FillIncome(income, week, false);
            columns.Children.Add(income);

            var spending = new StackPanel { Margin = new Thickness(14, 0, 0, 0) };
            spending.Children.Add(new TextBlock { Text = EnglishStrings.FinanceSpending.Translate(), FontWeight = FontWeights.Bold, FontSize = 12, Foreground = FindBrush("YtTextSecondaryBrush"), Margin = new Thickness(0, 0, 0, 2) });
            FillSpending(spending, week);
            Grid.SetColumn(spending, 1);
            columns.Children.Add(spending);
            root.Children.Add(columns);

            if (week.LoanReceived > Epsilon)
            {
                root.Children.Add(new Border { Height = 1, Background = FindBrush("YtDarkBorderBrush"), Margin = new Thickness(0, 10, 0, 6) });
                var loanRow = MakeRow(EnglishStrings.FinanceLoanReceived.Translate(), week.LoanReceived, FindBrush("YtTextSecondaryBrush"), false, false, false);
                System.Windows.Documents.TextElement.SetForeground((DependencyObject)loanRow, FindBrush("YtTextSecondaryBrush"));
                root.Children.Add(loanRow);
            }
            return root;
        }
    }
}
