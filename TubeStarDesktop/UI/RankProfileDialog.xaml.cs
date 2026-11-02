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
    /// Perfil de um canal do Top TubeStars (só números; rivais não têm vídeos)
    /// </summary>
    public partial class RankProfileDialog : ChildWindow
    {
        public RankProfileDialog(RankChannel channel)
        {
            InitializeComponent();
            Caption = channel.Name;
            btnOk.Content = EnglishStrings.Ok.Translate();

            //Banner na cor do avatar, escurecendo para a direita
            var color = RivalControl.GetAvatarColor(channel.Name);
            banner.Background = new LinearGradientBrush(Color.FromRgb((byte)(color.R / 3), (byte)(color.G / 3), (byte)(color.B / 3)), color, 0);
            avatarHost.Content = RivalControl.CreateAvatar(channel.Name, 56);

            txtName.Text = channel.Name;
            string position = String.Format(EnglishStrings.ProfilePosition.Translate(), channel.Position);
            txtSubtitle.Text = String.IsNullOrEmpty(channel.Theme) ? position : String.Format("{0} · {1}", channel.Theme, position);

            AddKpi(kpisTop, EnglishStrings.Subscribers.Translate(), channel.Subscribers.ToShortNumber(), null);
            AddKpi(kpisTop, EnglishStrings.Views.Translate(), channel.Views.ToShortNumber(), null);
            AddKpi(kpisTop, EnglishStrings.ProfileVideos.Translate(), channel.VideoCount.ToNumberString(), null);

            long growth = channel.WeekGrowth;
            AddKpi(kpisBottom, EnglishStrings.RankSubscribersWeek.Translate(), (growth > 0 ? "+" : "") + growth.ToShortNumber(), ChangeBrush(growth));
            int change = channel.Change;
            AddKpi(kpisBottom, EnglishStrings.ProfileChange.Translate(), change > 0 ? "▲ " + change : (change < 0 ? "▼ " + (-change) : EnglishStrings.ProfileNoChange.Translate()), ChangeBrush(change));
            AddKpi(kpisBottom, EnglishStrings.ProfileDaysAtTop.Translate(), channel.DaysAtTop.ToNumberString(), null);

            txtBestAndDistance.Text = String.Format(EnglishStrings.ProfileBest.Translate(), channel.BestPosition) + GetDistanceText(channel);
        }

        //Distância para o maior canal do jogador (não aparece no perfil do próprio jogador)
        private static string GetDistanceText(RankChannel channel)
        {
            if (channel.IsPlayer)
                return "";

            var me = Ranking.Current.Channels.FirstOrDefault(c => c.IsPlayer);
            if (me == null)
                return "";

            long diff = channel.Subscribers - me.Subscribers;
            if (diff > 0)
                return " · " + String.Format(EnglishStrings.ProfileDistanceBehind.Translate(), diff.ToShortNumber());
            if (diff == 0)
                return " · " + EnglishStrings.ProfileDistanceTie.Translate();
            return " · " + String.Format(EnglishStrings.ProfileDistanceAhead.Translate(), (-diff).ToShortNumber());
        }

        private static Brush ChangeBrush(long value)
        {
            if (value == 0)
                return null;
            return (Brush)Application.Current.FindResource(value > 0 ? "YtGainBrush" : "YtLossBrush");
        }

        private void AddKpi(UniformGrid target, string label, string value, Brush valueBrush)
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = label, Style = (Style)FindResource("KpiLabelStyle") });
            var txtValue = new TextBlock { Text = value, Style = (Style)FindResource("KpiValueStyle"), FontSize = 18 };
            if (valueBrush != null)
                txtValue.Foreground = valueBrush;
            panel.Children.Add(txtValue);
            target.Children.Add(panel);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
