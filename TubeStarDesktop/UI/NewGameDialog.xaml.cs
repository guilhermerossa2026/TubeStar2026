using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Xceed.Wpf.Toolkit;

namespace TubeStar
{
    /// <summary>
    /// Interaction logic for NewGameDialog.xaml
    /// </summary>
    public partial class NewGameDialog : ChildWindow
    {
        public event Action CasualClicked;
        public event Action CareerClicked;
        public event Action UltraClicked;

        public NewGameDialog()
        {
            InitializeComponent();
            Translate();
        }

        private void Translate()
        {
            Caption = EnglishStrings.NewGame.Translate();
            txtCasual.Text = EnglishStrings.Casual.Translate();
            txtChallenge.Text = EnglishStrings.Challenge.Translate();
            txtUltra.Text = EnglishStrings.Ultra.Translate();
            btnCancel.Content = EnglishStrings.Cancel.Translate();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Casual_Click(object sender, RoutedEventArgs e)
        {
            if (CasualClicked != null)
            {
                CasualClicked();
            }
        }

        private void Career_Click(object sender, RoutedEventArgs e)
        {
            if (CareerClicked != null)
            {
                CareerClicked();
            }
        }

        private void Ultra_Click(object sender, RoutedEventArgs e)
        {
            if (UltraClicked != null)
            {
                UltraClicked();
            }
        }
    }
}