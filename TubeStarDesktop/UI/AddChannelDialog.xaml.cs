using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace TubeStar
{
    /// <summary>
    /// Interaction logic for AddChannelDialog.xaml
    /// </summary>
    public partial class AddChannelDialog : ChildWindow
    {
        public Channel Channel { get; private set; }

        public AddChannelDialog()
        {
            InitializeComponent();
            Translate();
            FocusedElement = txtName;

            cmbAdvertising.ItemsSource = GetData();
            cmbAdvertising.DisplayMemberPath = "Value";
            cmbAdvertising.SelectedValuePath = "Key";

            cmbAdvertising.SelectedValue = AdvertisingStrategy.Normal;

            BuildNicheChips();
        }

        private VideoCategory? _niche;
        private bool _editing;

        private void BuildNicheChips()
        {
            foreach (VideoCategory niche in Enum.GetValues(typeof(VideoCategory)))
            {
                var chip = new Button { Content = niche.GetString(), Tag = niche, Style = (Style)FindResource("ChipButtonStyle") };
                chip.Click += (s, e) => SelectNiche((VideoCategory)((Button)s).Tag);
                nicheChips.Children.Add(chip);
            }
            txtNicheInfo.Text = EnglishStrings.NicheChooseHint.Translate();
        }

        private void SelectNiche(VideoCategory niche)
        {
            if (_editing)
                return;

            _niche = niche;
            foreach (Button chip in nicheChips.Children)
                chip.Style = (Style)FindResource((VideoCategory)chip.Tag == niche ? "ChipActiveButtonStyle" : "ChipButtonStyle");
            txtNicheInfo.Text = GetNicheInfo(niche);
        }

        //"RPM: $25 / mil views · Concorrência: Baixa"
        public static string GetNicheInfo(VideoCategory niche)
        {
            string pay = RpmCalculator.BaseRpm(niche).ToCurrencyString();
            double competition = niche.GetCompetitionMultiplier();
            string crowd = (competition <= 0.85 ? EnglishStrings.NicheCrowdHigh : (competition <= 0.95 ? EnglishStrings.NicheCrowdMedium : EnglishStrings.NicheCrowdLow)).Translate();
            return String.Format(EnglishStrings.NicheInfo.Translate(), pay, crowd);
        }

        private void Translate()
        {
            Caption = EnglishStrings.AddChannel.Translate();
            lblName.Text = EnglishStrings.Name.Translate() + ":";
            lblNiche.Text = EnglishStrings.NicheLabel.Translate() + ":";
            lblStrategy.Text = EnglishStrings.AdvertisingStrategy.Translate() + ":";
            lblStrategy.ToolTip = EnglishStrings.StrategyTable.Translate();
            lblStrategyHelp.ToolTip = EnglishStrings.StrategyTable.Translate();
            btnOk.Content = EnglishStrings.Ok.Translate();
            btnCancel.Content = EnglishStrings.Cancel.Translate();
        }

        public AddChannelDialog(Channel channel)
            : this()
        {
            Caption = EnglishStrings.EditChannel.Translate();
            Channel = channel;
            txtName.Text = channel.Name;
            cmbAdvertising.SelectedValue = channel.Advertising;

            //Nicho é fixo depois de criado: só mostra
            if (channel.Niche.HasValue)
                SelectNiche(channel.Niche.Value);
            _editing = true;
            foreach (Button chip in nicheChips.Children)
                chip.IsEnabled = channel.Niche.HasValue && (VideoCategory)chip.Tag == channel.Niche.Value;
            txtNicheInfo.Text = EnglishStrings.NicheFixed.Translate();
        }

        private Dictionary<AdvertisingStrategy, string> GetData()
        {
            Dictionary<AdvertisingStrategy, string> data = new Dictionary<AdvertisingStrategy, string>();
            data[AdvertisingStrategy.Low] = GetEnumString(AdvertisingStrategy.Low);
            data[AdvertisingStrategy.Normal] = GetEnumString(AdvertisingStrategy.Normal);
            data[AdvertisingStrategy.High] = GetEnumString(AdvertisingStrategy.High);
            data[AdvertisingStrategy.Aggressive] = GetEnumString(AdvertisingStrategy.Aggressive);
            return data;
        }

        private string GetEnumString(AdvertisingStrategy strategy)
        {
            return String.Format("{0} {1}", strategy.GetString(),
                Player.Current.UltraMode ? "(CTR - 10%)" : (Player.Current.ChallengeMode ? "(CTR - 50%)" : "")).Trim();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtName.Text))
            {
                CustomMessageBox.ShowDialog(EnglishStrings.MissingValueHeader.Translate(), EnglishStrings.MissingName.Translate(), MessagePicture.Puzzle);
                return;
            }

            if (!_editing && !_niche.HasValue)
            {
                CustomMessageBox.ShowDialog(EnglishStrings.MissingValueHeader.Translate(), EnglishStrings.NicheMissing.Translate(), MessagePicture.Puzzle);
                return;
            }

            if (cmbAdvertising.SelectedValue == null)
            {
                CustomMessageBox.ShowDialog(EnglishStrings.MissingValueHeader.Translate(), EnglishStrings.MissingStrategy.Translate(), MessagePicture.Puzzle);
                return;
            }

            Channel = new Channel()
            {
                Name = txtName.Text,
                Advertising = (AdvertisingStrategy)cmbAdvertising.SelectedValue,
                Niche = _niche,
            };
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}