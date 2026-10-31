using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TubeStar
{
    /// <summary>
    /// Interaction logic for StoreItemBlock.xaml
    /// </summary>
    public partial class StoreItemBlock : UserControl
    {
        public event Action PurchaseMade;

        private StoreItem _storeItem;
        public StoreItem StoreItem
        {
            get { return _storeItem; }
            private set
            {
                _storeItem = value;
                if (_storeItem != null)
                {
                    txtName.Text = _storeItem.Name;
                    txtDescription.Text = _storeItem.Description;
                    txtPrice.Text = _storeItem.Cost.ToCurrencyString();
                    btnBuy.Visibility = _storeItem.Purchased ? Visibility.Collapsed : Visibility.Visible;
                    pillPurchased.Visibility = _storeItem.Purchased ? Visibility.Visible : Visibility.Collapsed;
                    LayoutRoot.Cursor = _storeItem.Purchased ? Cursors.Arrow : Cursors.Hand;
                    imgItem.Source = new BitmapImage(new Uri(String.Format("../Resources/StoreItems/{0}.jpg", _storeItem.ImageName), UriKind.Relative));
                }
            }
        }

        public StoreItemBlock(StoreItem item)
        {
            InitializeComponent();
            btnBuy.Content = EnglishStrings.Buy.Translate();
            txtPurchased.Text = EnglishStrings.Purchased.Translate();
            StoreItem = item;
        }

        private void btnBuy_Click(object sender, RoutedEventArgs e)
        {
            TryBuy();
        }

        private void LayoutRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TryBuy();
        }

        private void TryBuy()
        {
            if (_storeItem == null || _storeItem.Purchased)
                return;

            this.PlayAnimation(() =>
            {
                if (Player.Current.Money - _storeItem.Cost < 0)
                {
                    CustomMessageBox.ShowDialog(EnglishStrings.LowCashHeader.Translate(), EnglishStrings.LowCashMessage.Translate(), MessagePicture.Money);
                    return;
                }

                CustomMessageBox.ShowDialog(EnglishStrings.Buy.Translate(), String.Format(EnglishStrings.BuyItem.Translate(), _storeItem.Cost.ToCurrencyString()), MessagePicture.Question, result =>
                {
                    if (result == true)
                    {
                        Player.Current.Money -= _storeItem.Cost;
                        _storeItem.Purchased = true;

                        if (_storeItem == StoreItems.Current.Loan)
                        {
                            Player.Current.LoanPayOff = (StoreItems.Current.Loan.Payout * (100 + StoreItems.Current.Loan.Interest) / 100);
                            Player.Current.Money += StoreItems.Current.Loan.Payout;
                        }

                        if (PurchaseMade != null)
                            PurchaseMade();
                    }
                });
            });
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_storeItem != null && !_storeItem.Purchased)
                cardBorder.Background = (Brush)System.Windows.Application.Current.FindResource("YtDarkCardHoverBrush");
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            cardBorder.Background = (Brush)System.Windows.Application.Current.FindResource("YtDarkCardBrush");
        }
    }
}