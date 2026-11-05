using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace TubeStar
{
    /// <summary>
    /// Análise do vídeo: abas Desempenho (views, posição, nota) e Dinheiro (custo, quanto rendeu, lucro)
    /// </summary>
    public partial class ViewVideoDialog : ChildWindow
    {
        public ViewVideoDialog(Video video, Channel channel)
        {
            InitializeComponent();
            Caption = EnglishStrings.ViewVideo.Translate();
            btnOk.Content = EnglishStrings.Ok.Translate();
            btnTabPerformance.Content = EnglishStrings.VideoTabPerformance.Translate();
            btnTabMoney.Content = EnglishStrings.VideoTabMoney.Translate();
            txtWhyScore.Text = EnglishStrings.VideoWhyScore.Translate();
            txtMoneyNote.Text = EnglishStrings.VideoMoneyNote.Translate();

            imgVideo.Source = video.GetThumbnail();
            txtName.Text = video.Name;
            txtChannel.Text = channel != null ? channel.Name : "";
            if (video.Type.HasValue)
                txtChannel.Text += (txtChannel.Text.Length > 0 ? " · " : "") + video.GetTypeString();
            txtScore.Text = String.Format(EnglishStrings.VideosScore.Translate(), (video.GenerateQuality() / 10.0).ToString("0.0"));

            double cost = video.Cost + video.OnceOffCost + video.Expenses;
            double profit = video.Income - cost;
            txtProfitHeader.Text = String.Format(EnglishStrings.VideoProfitHeader.Translate(), profit.ToShortCurrencyString());
            txtProfitHeader.ToolTip = String.Format(EnglishStrings.VideoProfitHeader.Translate(), profit.ToCurrencyString());
            txtProfitHeader.Foreground = FindBrush(profit < 0 ? "YtLossBrush" : "YtGainBrush");

            PopulatePerformance(video, channel);

            AddKpi(moneyKpis, EnglishStrings.VideoCost.Translate(), cost.ToShortCurrencyString(), null, cost.ToCurrencyString());
            AddKpi(moneyKpis, EnglishStrings.VideoIncome.Translate(), video.Income.ToShortCurrencyString(), null, video.Income.ToCurrencyString());
            AddKpi(moneyKpis, EnglishStrings.VideoProfit.Translate(), profit.ToShortCurrencyString(), FindBrush(profit < 0 ? "YtLossBrush" : "YtGainBrush"), profit.ToCurrencyString());

            //RPM do vídeo: ganho por mil views até agora; a tooltip mostra o RPM de hoje
            string rpmTip = null;
            if (channel != null && video.Quality.HasValue)
                rpmTip = String.Format(EnglishStrings.VideoRpmTip.Translate(), RpmCalculator.Rpm(channel, video).ToCurrencyString());
            AddKpi(moneyKpis, EnglishStrings.ChannelRpm.Translate(), video.Views > 0 ? (video.Income * 1000.0 / video.Views).ToCurrencyString() : "-", null, rpmTip);

            ShowTab(true);
        }

        private static Brush FindBrush(string key)
        {
            return (Brush)Application.Current.FindResource(key);
        }

        private void PopulatePerformance(Video video, Channel channel)
        {
            AddKpi(performanceKpis, EnglishStrings.Views.Translate(), video.Views.ToShortNumber(), null);
            AddKpi(performanceKpis, EnglishStrings.VideoSubscribersGained.Translate(), (video.SubscribersGained > 0 ? "+" : "") + video.SubscribersGained.ToShortNumber(), null);

            //Posição do vídeo no canal, pelo número de views
            string rank = "-";
            if (channel != null && channel.Videos.Count > 0)
            {
                int position = channel.Videos.OrderByDescending(v => v.Views).ToList().IndexOf(video) + 1;
                if (position > 0)
                    rank = String.Format(EnglishStrings.VideoRankValue.Translate(), position, channel.Videos.Count);
            }
            AddKpi(performanceKpis, EnglishStrings.VideoRankInChannel.Translate(), rank, null);

            int votes = video.Likes + video.Dislikes;
            AddKpi(performanceKpis, EnglishStrings.VideoApproval.Translate(), votes > 0 ? (video.Likes * 100 / votes) + "%" : "-", null);

            //De onde veio a nota: gravação + edição (+ bônus de atributos), escala 0-100
            AddQualityRow(EnglishStrings.VideoShooting.Translate(), video.ShootQuality);
            AddQualityRow(EnglishStrings.VideoEditing.Translate(), video.HasBeenEdited ? video.EditQuality : null);

            if (video.Attributes != null && video.Attributes.Count > 0)
                txtAttributes.Text = String.Format(EnglishStrings.VideoAttributesList.Translate(), String.Join(", ", video.Attributes.Select(a => a.Name)));
            else
                txtAttributes.Visibility = Visibility.Collapsed;
        }

        private void AddQualityRow(string label, int? value)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });

            grid.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center });

            int points = Math.Max(0, Math.Min(100, value ?? 0));
            var track = new Grid { Height = 8, VerticalAlignment = VerticalAlignment.Center };
            track.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Max(points, 0.01), GridUnitType.Star) });
            track.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Max(100 - points, 0.01), GridUnitType.Star) });
            var back = new Border { CornerRadius = new CornerRadius(4), Background = FindBrush("YtDarkBorderBrush") };
            Grid.SetColumnSpan(back, 2);
            track.Children.Add(back);
            if (points > 0)
                track.Children.Add(new Border { CornerRadius = new CornerRadius(4), Background = FindBrush("YtRedBrush") });
            Grid.SetColumn(track, 1);
            grid.Children.Add(track);

            var txtValue = new TextBlock
            {
                Text = value.HasValue ? String.Format(EnglishStrings.VideoPoints.Translate(), value.Value) : EnglishStrings.VideoNotEdited.Translate(),
                Style = (Style)FindResource("MutedTextStyle"),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtValue, 2);
            grid.Children.Add(txtValue);

            qualityRows.Children.Add(grid);
        }

        private void AddKpi(UniformGrid target, string label, string value, Brush valueBrush)
        {
            AddKpi(target, label, value, valueBrush, null);
        }

        private void AddKpi(UniformGrid target, string label, string value, Brush valueBrush, string tooltip)
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = label, Style = (Style)FindResource("KpiLabelStyle") });
            var txtValue = new TextBlock { Text = value, Style = (Style)FindResource("KpiValueStyle"), FontSize = 18 };
            if (valueBrush != null)
                txtValue.Foreground = valueBrush;
            if (tooltip != null)
                txtValue.ToolTip = tooltip;
            panel.Children.Add(new Viewbox { Child = txtValue, StretchDirection = StretchDirection.DownOnly, HorizontalAlignment = HorizontalAlignment.Left });
            target.Children.Add(panel);
        }

        private void ShowTab(bool performance)
        {
            panelPerformance.Visibility = performance ? Visibility.Visible : Visibility.Collapsed;
            panelMoney.Visibility = performance ? Visibility.Collapsed : Visibility.Visible;
            btnTabPerformance.Style = (Style)FindResource(performance ? "ChipActiveButtonStyle" : "ChipButtonStyle");
            btnTabMoney.Style = (Style)FindResource(performance ? "ChipButtonStyle" : "ChipActiveButtonStyle");
        }

        private void btnTabPerformance_Click(object sender, RoutedEventArgs e)
        {
            ShowTab(true);
        }

        private void btnTabMoney_Click(object sender, RoutedEventArgs e)
        {
            ShowTab(false);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
