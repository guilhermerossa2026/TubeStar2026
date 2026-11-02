using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace TubeStar
{
    /// <summary>
    /// Resumo da semana ou do ano (Calendário)
    /// </summary>
    public partial class PeriodSummaryDialog : ChildWindow
    {
        public PeriodSummaryDialog(string title, PeriodStats stats)
        {
            InitializeComponent();
            Translate();
            Caption = title;
            Populate(stats);
        }

        private void Translate()
        {
            txtByChannel.Text = EnglishStrings.SummaryByChannel.Translate();
            hdrChannel.Text = EnglishStrings.Channel.Translate();
            hdrViews.Text = EnglishStrings.Views.Translate();
            hdrSubscribers.Text = EnglishStrings.SummaryNewSubscribers.Translate();
            hdrBestVideo.Text = EnglishStrings.SummaryBestVideo.Translate();
            txtNoChannels.Text = EnglishStrings.SummaryNoChannels.Translate();
            btnOk.Content = EnglishStrings.Ok.Translate();
        }

        private void Populate(PeriodStats stats)
        {
            foreach (var kpi in DailyPlanner.CreateSummaryKpis(stats))
                kpiGrid.Children.Add(kpi);

            //Só canais com movimento, do que teve mais views para o que teve menos
            var channels = stats.Channels.Where(c => c.Views > 0 || c.NewSubscribers != 0).OrderByDescending(c => c.Views).ToList();
            txtNoChannels.Visibility = channels.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            foreach (var channel in channels)
            {
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

                AddCell(grid, 0, channel.Name, HorizontalAlignment.Left);
                AddCell(grid, 1, channel.Views.ToShortNumber(), HorizontalAlignment.Right);
                AddCell(grid, 2, channel.NewSubscribers.ToShortNumber(), HorizontalAlignment.Right);

                var best = channel.BestVideo;
                var bestCell = AddCell(grid, 3, best == null ? "-" : String.Format("{0} ({1} {2})", best.Name, best.Views.ToShortNumber(), EnglishStrings.Views.Translate().ToLower()), HorizontalAlignment.Left);
                bestCell.Margin = new Thickness(16, 0, 0, 0);

                channelRows.Children.Add(new Border { Style = (Style)FindResource("TableRowStyle"), Child = grid });
            }
        }

        private static TextBlock AddCell(Grid grid, int column, string text, HorizontalAlignment alignment)
        {
            var cell = new TextBlock { Text = text, HorizontalAlignment = alignment, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
            Grid.SetColumn(cell, column);
            grid.Children.Add(cell);
            return cell;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
