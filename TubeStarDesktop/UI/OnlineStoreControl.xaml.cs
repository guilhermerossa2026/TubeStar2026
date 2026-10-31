using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace TubeStar
{
    /// <summary>
    /// Interaction logic for OnlineStoreControl.xaml
    /// </summary>
    public partial class OnlineStoreControl : UserControl
    {
        public OnlineStoreControl()
        {
            InitializeComponent();
            Translate();
        }

        private void Translate()
        {
            txtTitle.Text = EnglishStrings.OnlineStore.Translate();
        }

        public void Update()
        {
            txtMoney.Inlines.Clear();
            string[] parts = EnglishStrings.YouHave.Translate().Split(new string[] { "{0}" }, StringSplitOptions.None);
            txtMoney.Inlines.Add(new Run(parts[0]));
            txtMoney.Inlines.Add(new Bold(new Run(Player.Current.Money.ToCurrencyString())) { Foreground = (Brush)FindResource("YtTextPrimaryBrush") });
            if (parts.Length > 1)
                txtMoney.Inlines.Add(new Run(parts[1]));

            //Itens a venda primeiro; os ja comprados ficam no fim, com a pill "Comprado"
            itemPanel.Children.Clear();
            var available = StoreItems.Current.All.Where(i => !i.Purchased).Reverse();
            var purchased = StoreItems.Current.All.Where(i => i.Purchased).Reverse();
            foreach (var item in available.Concat(purchased))
            {
                var storeBlock = new StoreItemBlock(item);
                storeBlock.PurchaseMade += StoreBlock_PurchaseMade;
                itemPanel.Children.Add(storeBlock);
            }
        }

        private void StoreBlock_PurchaseMade()
        {
            Update();
        }
    }
}