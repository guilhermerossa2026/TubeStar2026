using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Xceed.Wpf.Toolkit;

namespace TubeStar
{
    /// <summary>
    /// Interaction logic for VideoStatsDialog.xaml
    /// </summary>
    public partial class ChannelStatsDialog : ChildWindow
    {
        public ChannelStatsDialog(Channel channel)
        {
            InitializeComponent();
            Translate();
            Populate(channel);
        }

        private void Translate()
        {
            Caption = EnglishStrings.Stats.Translate();
            btnOk.Content = EnglishStrings.Ok.Translate();
        }

        private void Populate(Channel channel)
        {
            PopulateSubscribers(channel);
            PopulateIncome(channel);
        }

        private void PopulateSubscribers(Channel channel)
        {
            TextBlock nameBlock = new TextBlock();
            nameBlock.Text = EnglishStrings.SubscriberHistory.Translate();
            nameBlock.FontSize = 16;
            nameBlock.Margin = new Thickness(5, 0, 5, 0);
            plotParent.Children.Add(nameBlock);

            OxyPlot.Wpf.Plot plot = new OxyPlot.Wpf.Plot();
            plot.Height = 200;

            var plotModel1 = new PlotModel();

            ApplyDarkTheme(plotModel1);
            var viewSeries = new LineSeries(OxyColor.FromRgb(0xFF, 0x00, 0x00));
            int count = 1;
            foreach (var subscriber in channel.SubscribersOverTime)
            {
                viewSeries.Points.Add(new DataPoint(count++, subscriber));
            }
            plotModel1.Series.Add(viewSeries);
            plot.Model = plotModel1;
            plotParent.Children.Add(plot);
        }

        private void PopulateIncome(Channel channel)
        {
            TextBlock nameBlock = new TextBlock();
            nameBlock.Text = EnglishStrings.DailyIncome.Translate();
            nameBlock.FontSize = 16;
            nameBlock.Margin = new Thickness(5, 0, 5, 0);
            plotParent.Children.Add(nameBlock);

            OxyPlot.Wpf.Plot plot = new OxyPlot.Wpf.Plot();
            plot.Height = 200;

            var plotModel1 = new PlotModel();

            ApplyDarkTheme(plotModel1);
            var incomeSeries = new LineSeries(OxyColor.FromRgb(0x3E, 0xA6, 0x6B));
            int count = 1;
            double baseIncome = 0;
            foreach (var income in channel.IncomeOverTime)
            {
                baseIncome += income;
                incomeSeries.Points.Add(new DataPoint(count++, baseIncome));
            }
            plotModel1.Series.Add(incomeSeries);

            var expenseSeries = new LineSeries(OxyColor.FromRgb(0xE5, 0x53, 0x4B));
            count = 1;
            double baseExpenses = 0;
            foreach (var expense in channel.ExpensesOverTime)
            {
                baseExpenses += expense;
                expenseSeries.Points.Add(new DataPoint(count++, baseExpenses));
            }
            plotModel1.Series.Add(expenseSeries);
            plot.Model = plotModel1;
            plotParent.Children.Add(plot);
        }

        // Cores do tema escuro (Custom.xaml) para o gráfico: texto, eixos e grade

        private static void ApplyDarkTheme(PlotModel model)

        {

            OxyColor text = OxyColor.FromRgb(0xAA, 0xAA, 0xAA);

            OxyColor line = OxyColor.FromRgb(0x2D, 0x2D, 0x2D);

            model.Background = OxyColor.FromRgb(0x14, 0x14, 0x14);

            model.TextColor = text;

            model.PlotAreaBorderColor = line;

            model.LegendTextColor = OxyColor.FromRgb(0xF1, 0xF1, 0xF1);

            foreach (AxisPosition pos in new[] { AxisPosition.Bottom, AxisPosition.Left })

            {

                LinearAxis axis = new LinearAxis();

                axis.Position = pos;

                axis.TextColor = text;

                axis.TitleColor = text;

                axis.AxislineColor = line;

                axis.TicklineColor = line;

                axis.MajorGridlineColor = OxyColor.FromRgb(0x26, 0x26, 0x26);

                axis.MinorGridlineColor = OxyColor.FromRgb(0x1F, 0x1F, 0x1F);

                model.Axes.Add(axis);

            }

        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}