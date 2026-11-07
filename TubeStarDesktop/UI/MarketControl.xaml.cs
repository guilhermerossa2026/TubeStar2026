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
    //Tela Mercado financeiro: abas Mercado e Carteira, e a tela do ativo (nesta etapa só para olhar)
    public partial class MarketControl : UserControl
    {
        private static readonly CultureInfo Culture = new CultureInfo("en-US");
        private static readonly string[] TickerStrip = { "BTC", "ETH", "PETR4", "AAPL", "SPY", "BOVA11" };
        //Mesma ordem do enum AssetClass: Ações BR, Ações EUA, Cripto, Fundos, Renda fixa
        private static readonly EnglishStrings[] AssetTabNames =
        {
            EnglishStrings.MarketTabStockBR, EnglishStrings.MarketTabStockUS, EnglishStrings.MarketTabCrypto,
            EnglishStrings.MarketTabFunds, EnglishStrings.MarketTabFixed
        };
        private const int SparkDays = 7;
        private const int MoverRows = 5;
        private const int SectorRows = 8;
        private const double SectorFullBar = 3; //variação (%) que enche a barra do setor

        private enum View { Market, Wallet, Asset }

        //Referências de uma linha/cartão para atualizar sem recriar
        private class RowRefs
        {
            public string Ticker;
            public TextBlock Star;
            public TextBlock Price;
            public TextBlock Change;
            public Canvas Spark;
            public TextBlock Spark7;
        }

        private class SectorStat
        {
            public EnglishStrings Name;
            public double Sum;
            public int Count;
            public double Average;
        }

        private class Mover
        {
            public AssetDef Def;
            public double Change;
        }

        private View _view = View.Market;
        private int _assetTab;
        private bool _showGainers = true;
        private bool _tableDirty = true;
        private string _current;
        private int _period = 1; //0 = 7d, 1 = 28d, 2 = 112d, 3 = tudo (semanal)
        private List<double> _chartValues = new List<double>();
        private bool _chartFixed;

        private readonly List<RowRefs> _tickerCards = new List<RowRefs>();
        private readonly List<RowRefs> _tableRows = new List<RowRefs>();
        private readonly List<Button> _assetTabButtons = new List<Button>();

        public MarketControl()
        {
            InitializeComponent();
            Translate();
            BuildTickerCards();
            BuildAssetTabs();
            UpdateTabStyles();
        }

        private void Translate()
        {
            txtTitle.Text = EnglishStrings.NavMarket.Translate();
            txtLead.Text = EnglishStrings.MarketLead.Translate();
            txtTabMarket.Text = EnglishStrings.FinanceTabMarket.Translate();
            txtTabWallet.Text = EnglishStrings.MarketTabPortfolio.Translate();
            txtWalletEmpty.Text = EnglishStrings.MarketPortfolioEmpty.Translate();

            txtMoversTitle.Text = EnglishStrings.MarketMovers.Translate();
            txtGainers.Text = EnglishStrings.MarketGainers.Translate();
            txtLosers.Text = EnglishStrings.MarketLosers.Translate();
            txtSectorsTitle.Text = EnglishStrings.MarketSectors.Translate();
            txtSectorsNote.Text = EnglishStrings.MarketTodayNote.Translate();
            txtNewsTitle.Text = EnglishStrings.MarketNews.Translate();
            txtNewsSoon.Text = EnglishStrings.MarketNewsSoon.Translate();
            txtTipTitle.Text = EnglishStrings.MarketTipTitle.Translate();
            txtAssetsTitle.Text = EnglishStrings.MarketAssets.Translate();

            txtBack.Text = "← " + EnglishStrings.MarketBack.Translate();
            txtFavTitle.Text = EnglishStrings.MarketFavorites.Translate();
            txtFavHint.Text = EnglishStrings.MarketFavoritesHint.Translate();
            txtPAll.Text = EnglishStrings.MarketPeriodAll.Translate();
            txtPillRisky.Text = EnglishStrings.MarketRisky.Translate();
            txtChartEmpty.Text = EnglishStrings.MarketChartNoData.Translate();
            txtBuy.Text = EnglishStrings.MarketBuy.Translate();
            txtSell.Text = EnglishStrings.MarketSell.Translate();
            txtGrossLabel.Text = EnglishStrings.MarketGross.Translate();
            txtFeeLabel.Text = EnglishStrings.FinanceInvestFee.Translate();
            txtTaxLabel.Text = EnglishStrings.FinanceInvestTax.Translate();
            txtPosTitle.Text = EnglishStrings.MarketYourPosition.Translate();
            txtPosQtyLabel.Text = EnglishStrings.MarketQuantity.Translate();
            txtPosAvgLabel.Text = EnglishStrings.MarketAvgPrice.Translate();
            txtPosInvestedLabel.Text = EnglishStrings.MarketInvested.Translate();
            txtPosChangeLabel.Text = EnglishStrings.MarketSinceBuy.Translate();
            btnBuy.Click += delegate { SetMode(false); };
            btnSell.Click += delegate { SetMode(true); };
            btnPct25.Click += Pct_Click;
            btnPct50.Click += Pct_Click;
            btnPct75.Click += Pct_Click;
            btnPct100.Click += Pct_Click;
            btnTrade.Click += Trade_Click;
            boxAmount.TextChanged += BoxAmount_TextChanged;
        }

        //---------- Utilidades ----------

        private static Brush FindBrush(string key)
        {
            return (Brush)Application.Current.FindResource(key);
        }

        private static Style FindStyle(string key)
        {
            return (Style)Application.Current.FindResource(key);
        }

        //Variação do dia em %; sem preço anterior ou preço igual = 0 (neutro)
        private static double DayChange(AssetState state)
        {
            if (state == null || state.PrevPrice <= 0 || state.Price == state.PrevPrice)
                return 0;
            return (state.Price - state.PrevPrice) / state.PrevPrice * 100;
        }

        private static string FormatPct(double value)
        {
            double rounded = Math.Round(value, 2);
            if (rounded == 0)
                return "0.00%";
            return (rounded > 0 ? "+" : "−") + Math.Abs(rounded).ToString("0.00", Culture) + "%";
        }

        private static Brush ChangeBrush(double value)
        {
            double rounded = Math.Round(value, 2);
            if (rounded > 0) return FindBrush("YtGainBrush");
            if (rounded < 0) return FindBrush("YtLossBrush");
            return FindBrush("YtTextSecondaryBrush");
        }

        //Cor pela direção do movimento, sem arredondar (serve para preços muito pequenos)
        private static Brush TrendBrush(double delta)
        {
            if (delta > 0) return FindBrush("YtGainBrush");
            if (delta < 0) return FindBrush("YtLossBrush");
            return FindBrush("YtTextSecondaryBrush");
        }

        private static string ChangeStyleKey(double value)
        {
            double rounded = Math.Round(value, 2);
            if (rounded > 0) return "PillGainStyle";
            if (rounded < 0) return "PillLossStyle";
            return "PillStyle";
        }

        //Preço em dólar: 2 casas (>= 1), 4 (< 1), 6 (< 0.01) ou 8 (< 0.0001). Renda fixa é um índice: 4 casas
        private static string FormatPrice(double price, bool index)
        {
            int decimals = 2;
            if (index) decimals = 4;
            else if (price < 0.0001) decimals = 8;
            else if (price < 0.01) decimals = 6;
            else if (price < 1) decimals = 4;
            return "$" + price.ToString("N" + decimals, Culture);
        }

        private static TextBlock Text(string text, double size, FontWeight weight, Brush brush)
        {
            var block = new TextBlock { Text = text, FontSize = size, FontWeight = weight, VerticalAlignment = VerticalAlignment.Center };
            if (brush != null)
                block.Foreground = brush;
            return block;
        }

        //Etiqueta do tipo (Ação, Cripto, Fundo, Renda fixa)
        private static Border MakeTag(string text)
        {
            var label = Text(text, 10, FontWeights.SemiBold, FindBrush("YtTextSecondaryBrush"));
            return new Border { Background = FindBrush("YtDarkCardHoverBrush"), CornerRadius = new CornerRadius(6), Padding = new Thickness(7, 1, 7, 1), VerticalAlignment = VerticalAlignment.Center, Child = label };
        }

        private static Border MakePill(string text, string styleKey)
        {
            var label = new TextBlock { Text = text, Style = FindStyle("InheritTextStyle") };
            return new Border { Style = FindStyle(styleKey), Child = label, Margin = new Thickness(6, 0, 0, 0) };
        }

        private static void SetSeg(Button button, bool active)
        {
            button.Style = FindStyle(active ? "SegButtonActiveStyle" : "SegButtonStyle");
        }

        //Mini-gráfico: um Canvas com uma Polyline dentro
        private static Canvas NewSpark(double width, double height)
        {
            var canvas = new Canvas { Width = width, Height = height, VerticalAlignment = VerticalAlignment.Center };
            canvas.Children.Add(new Polyline { StrokeThickness = 1.8, StrokeLineJoin = PenLineJoin.Round });
            return canvas;
        }

        private static readonly Dictionary<string, Brush> AvatarBrushes = new Dictionary<string, Brush>();

        //Bolinha colorida com a inicial do ativo
        private static UIElement MakeAvatar(AssetDef def, double size)
        {
            Brush fill;
            if (!AvatarBrushes.TryGetValue(def.Color, out fill))
            {
                fill = (Brush)new BrushConverter().ConvertFromString(def.Color);
                fill.Freeze();
                AvatarBrushes[def.Color] = fill;
            }
            var grid = new Grid { Width = size, Height = size, VerticalAlignment = VerticalAlignment.Center };
            grid.Children.Add(new Ellipse { Fill = fill });
            //Cor clara (ex.: cinza da Apple) pede letra escura para dar contraste
            var sc = fill as SolidColorBrush;
            bool light = sc != null && (0.299 * sc.Color.R + 0.587 * sc.Color.G + 0.114 * sc.Color.B) > 160;
            var initial = Text(def.Ticker.Substring(0, 1), size * 0.46, FontWeights.Bold, FindBrush(light ? "YtDarkBgBrush" : "YtOnAccentBrush"));
            initial.HorizontalAlignment = HorizontalAlignment.Center;
            grid.Children.Add(initial);
            return grid;
        }

        //Ticker (com selo Arriscada, se for o caso) e nome embaixo
        private static StackPanel MakeNames(AssetDef def, bool showRisky)
        {
            var names = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var top = new StackPanel { Orientation = Orientation.Horizontal };
            top.Children.Add(Text(def.Ticker, 13, FontWeights.Bold, null));
            if (showRisky && def.Risky)
            {
                var pill = MakePill(EnglishStrings.MarketRisky.Translate(), "PillLossStyle");
                pill.Padding = new Thickness(6, 0, 6, 0);
                top.Children.Add(pill);
            }
            names.Children.Add(top);
            var name = Text(def.DisplayName(), 11, FontWeights.Normal, FindBrush("YtTextSecondaryBrush"));
            name.TextTrimming = TextTrimming.CharacterEllipsis;
            names.Children.Add(name);
            return names;
        }

        //Rótulo "7d +X%"
        private static TextBlock NewSpark7()
        {
            return Text("", 11, FontWeights.SemiBold, null);
        }

        //Últimos 7 dias; menos de 2 pontos = sem gráfico. Cor e rótulo seguem a tendência dos 7 pontos
        private static void UpdateSpark(Canvas canvas, TextBlock label, List<double> daily)
        {
            var line = (Polyline)canvas.Children[0];
            line.Points.Clear();
            label.Text = "";
            if (daily == null || daily.Count < 2)
                return;

            int count = Math.Min(SparkDays, daily.Count);
            int start = daily.Count - count;
            double min = daily[start];
            double max = daily[start];
            for (int i = start; i < daily.Count; i++)
            {
                if (daily[i] < min) min = daily[i];
                if (daily[i] > max) max = daily[i];
            }
            double range = max - min;
            for (int i = 0; i < count; i++)
            {
                double x = 2 + (canvas.Width - 4) * i / (count - 1);
                double y = range <= 0 ? canvas.Height / 2 : (canvas.Height - 3) - (daily[start + i] - min) / range * (canvas.Height - 6);
                line.Points.Add(new Point(x, y));
            }
            double first = daily[start];
            double pct = first > 0 ? (daily[daily.Count - 1] - first) / first * 100 : 0;
            line.Stroke = ChangeBrush(pct);
            label.Text = "7d " + FormatPct(pct);
            label.Foreground = ChangeBrush(pct);
        }

        //---------- Abas e telas ----------

        private void SetView(View view)
        {
            _view = view;
            pageMarket.Visibility = view == View.Market ? Visibility.Visible : Visibility.Collapsed;
            pageWallet.Visibility = view == View.Wallet ? Visibility.Visible : Visibility.Collapsed;
            pageAsset.Visibility = view == View.Asset ? Visibility.Visible : Visibility.Collapsed;
            tabsBar.Visibility = view == View.Asset ? Visibility.Collapsed : Visibility.Visible;
            UpdateTabStyles();
            scroll.ScrollToTop();
        }

        private void UpdateTabStyles()
        {
            bool wallet = _view == View.Wallet;
            tabMarket.Style = FindStyle(wallet ? "TabBorderStyle" : "TabActiveBorderStyle");
            tabWallet.Style = FindStyle(wallet ? "TabActiveBorderStyle" : "TabBorderStyle");
            txtTabMarket.Foreground = FindBrush(wallet ? "YtTextSecondaryBrush" : "YtTextPrimaryBrush");
            txtTabWallet.Foreground = FindBrush(wallet ? "YtTextPrimaryBrush" : "YtTextSecondaryBrush");
        }

        private void TabMarket_Click(object sender, MouseButtonEventArgs e)
        {
            SetView(View.Market);
            Update();
        }

        private void TabWallet_Click(object sender, MouseButtonEventArgs e)
        {
            SetView(View.Wallet);
        }

        private void OpenAsset(string ticker)
        {
            _current = ticker;
            SetMode(false);
            txtTradeNote.Visibility = Visibility.Collapsed;
            SetView(View.Asset);
            Update();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            SetView(View.Market);
            Update();
        }

        //Chamado ao abrir a tela e depois de cada novo dia (só se estiver visível)
        public void Update()
        {
            var m = Market.Current;
            if (_view == View.Market)
            {
                RefreshTickerCards(m);
                RefreshMovers(m);
                RefreshSectors(m);
                RefreshTip();
                if (_tableDirty)
                    BuildTable();
                RefreshTable(m);
            }
            else if (_view == View.Asset)
            {
                RefreshAsset(m);
            }
        }

        //Dica da semana: uma das 8 dicas, trocando a cada 7 dias do jogo
        private void RefreshTip()
        {
            const int tips = 8;
            int week = Player.Current.Iterations / 7;
            var tip = (EnglishStrings)((int)EnglishStrings.MarketTip1 + week % tips);
            txtTipText.Text = tip.Translate();
        }

        //---------- Faixa de cotações ----------

        private void BuildTickerCards()
        {
            foreach (string ticker in TickerStrip)
            {
                var def = AssetDef.Find(ticker);
                if (def == null)
                    continue;

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var avatar = MakeAvatar(def, 28);
                ((FrameworkElement)avatar).Margin = new Thickness(0, 0, 8, 0);
                ((FrameworkElement)avatar).VerticalAlignment = VerticalAlignment.Top;
                grid.Children.Add(avatar);

                var names = MakeNames(def, false);
                Grid.SetColumn(names, 1);
                grid.Children.Add(names);

                var sparkBox = new StackPanel { Margin = new Thickness(8, 0, 0, 0) };
                var spark = NewSpark(56, 22);
                var spark7 = NewSpark7();
                spark7.HorizontalAlignment = HorizontalAlignment.Right;
                spark7.Margin = new Thickness(0, 4, 0, 0);
                sparkBox.Children.Add(spark);
                sparkBox.Children.Add(spark7);
                Grid.SetColumn(sparkBox, 2);
                grid.Children.Add(sparkBox);

                var price = Text("", 15, FontWeights.Bold, null);
                price.Margin = new Thickness(0, 6, 0, 0);
                Grid.SetRow(price, 1);
                Grid.SetColumnSpan(price, 2);
                grid.Children.Add(price);

                var change = Text("", 12, FontWeights.SemiBold, null);
                change.Margin = new Thickness(8, 6, 0, 0);
                change.HorizontalAlignment = HorizontalAlignment.Right;
                Grid.SetRow(change, 1);
                Grid.SetColumn(change, 2);
                grid.Children.Add(change);

                var card = new Border { Style = FindStyle("CardBorderStyle"), Padding = new Thickness(12, 10, 12, 10), Margin = new Thickness(5, 0, 5, 0), Child = grid };
                tickerGrid.Children.Add(card);
                _tickerCards.Add(new RowRefs { Ticker = ticker, Price = price, Change = change, Spark = spark, Spark7 = spark7 });
            }
        }

        private void RefreshTickerCards(Market m)
        {
            foreach (var card in _tickerCards)
            {
                var def = AssetDef.Find(card.Ticker);
                var state = m.Get(card.Ticker);
                if (def == null || state == null)
                    continue;
                double change = DayChange(state);
                card.Price.Text = FormatPrice(state.Price, def.Class == AssetClass.FixedIncome);
                card.Change.Text = FormatPct(change);
                card.Change.Foreground = ChangeBrush(change);
                UpdateSpark(card.Spark, card.Spark7, state.Daily);
            }
        }

        //---------- Maiores altas / baixas ----------

        private void Gainers_Click(object sender, RoutedEventArgs e)
        {
            _showGainers = true;
            SetSeg(btnGainers, true);
            SetSeg(btnLosers, false);
            RefreshMovers(Market.Current);
        }

        private void Losers_Click(object sender, RoutedEventArgs e)
        {
            _showGainers = false;
            SetSeg(btnGainers, false);
            SetSeg(btnLosers, true);
            RefreshMovers(Market.Current);
        }

        private void RefreshMovers(Market m)
        {
            var list = new List<Mover>();
            foreach (var def in AssetDef.All)
            {
                if (def.Class == AssetClass.FixedIncome)
                    continue;
                list.Add(new Mover { Def = def, Change = DayChange(m.Get(def.Ticker)) });
            }
            list.Sort((a, b) => b.Change.CompareTo(a.Change));

            moversPanel.Children.Clear();
            int rows = Math.Min(MoverRows, list.Count);
            for (int i = 0; i < rows; i++)
            {
                var mover = _showGainers ? list[i] : list[list.Count - 1 - i];
                var state = m.Get(mover.Def.Ticker);
                if (state == null)
                    continue;
                moversPanel.Children.Add(MakeMoverRow(mover, state));
            }
        }

        private UIElement MakeMoverRow(Mover mover, AssetState state)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(62) });

            var avatar = MakeAvatar(mover.Def, 28);
            ((FrameworkElement)avatar).Margin = new Thickness(0, 0, 10, 0);
            grid.Children.Add(avatar);

            var names = MakeNames(mover.Def, true);
            Grid.SetColumn(names, 1);
            grid.Children.Add(names);

            var tag = MakeTag(mover.Def.TypeLabel());
            tag.Margin = new Thickness(8, 0, 8, 0);
            Grid.SetColumn(tag, 2);
            grid.Children.Add(tag);

            var price = Text(FormatPrice(state.Price, false), 13, FontWeights.Normal, null);
            price.Margin = new Thickness(0, 0, 8, 0);
            Grid.SetColumn(price, 3);
            grid.Children.Add(price);

            var change = Text(FormatPct(mover.Change), 13, FontWeights.SemiBold, ChangeBrush(mover.Change));
            change.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetColumn(change, 4);
            grid.Children.Add(change);

            var row = new Border { Style = FindStyle("TableRowStyle"), Padding = new Thickness(12, 4, 12, 4), MinHeight = 46, Cursor = Cursors.Hand, Child = grid, Tag = mover.Def.Ticker };
            row.MouseLeftButtonUp += Row_Click;
            return row;
        }

        //---------- Desempenho por setor ----------

        private void RefreshSectors(Market m)
        {
            var stats = new List<SectorStat>();
            foreach (var def in AssetDef.All)
            {
                if (def.Class == AssetClass.FixedIncome)
                    continue;
                var state = m.Get(def.Ticker);
                if (state == null)
                    continue;
                SectorStat stat = null;
                foreach (var s in stats)
                {
                    if (s.Name == def.Sector)
                        stat = s;
                }
                if (stat == null)
                {
                    stat = new SectorStat { Name = def.Sector };
                    stats.Add(stat);
                }
                stat.Sum += DayChange(state);
                stat.Count++;
            }
            foreach (var s in stats)
                s.Average = s.Count > 0 ? s.Sum / s.Count : 0;
            stats.Sort((a, b) => b.Average.CompareTo(a.Average));

            //Muitos setores: as maiores altas e as maiores baixas
            if (stats.Count > SectorRows)
            {
                int half = SectorRows / 2;
                var shown = new List<SectorStat>();
                shown.AddRange(stats.GetRange(0, half));
                shown.AddRange(stats.GetRange(stats.Count - (SectorRows - half), SectorRows - half));
                stats = shown;
            }

            sectorsGrid.Children.Clear();
            sectorsGrid.RowDefinitions.Clear();
            for (int i = 0; i < stats.Count; i++)
            {
                sectorsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 28 });
                var row = MakeSectorRow(stats[i]);
                Grid.SetRow(row, i);
                sectorsGrid.Children.Add(row);
            }
        }

        private static UIElement MakeSectorRow(SectorStat stat)
        {
            var grid = new Grid { VerticalAlignment = VerticalAlignment.Center };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(124) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(58) });

            var name = Text(stat.Name.Translate(), 12, FontWeights.Normal, null);
            name.TextTrimming = TextTrimming.CharacterEllipsis;
            name.Margin = new Thickness(0, 0, 8, 0);
            grid.Children.Add(name);

            //Barra a partir do meio: ganho para a direita, perda para a esquerda
            var host = new Grid { Height = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };
            var track = new Border { Height = 8, CornerRadius = new CornerRadius(4), Background = FindBrush("YtDarkCardHoverBrush"), VerticalAlignment = VerticalAlignment.Center };
            var fill = new Grid();
            double rounded = Math.Round(stat.Average, 2);
            if (rounded != 0)
            {
                double w = Math.Min(1, Math.Abs(rounded) / SectorFullBar) * 50;
                bool gain = rounded > 0;
                fill.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(gain ? 50 : 50 - w, GridUnitType.Star) });
                fill.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(w, GridUnitType.Star) });
                fill.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(gain ? 50 - w : 50, GridUnitType.Star) });
                var bar = new Border
                {
                    Background = FindBrush(gain ? "YtGainBrush" : "YtLossBrush"),
                    CornerRadius = gain ? new CornerRadius(0, 4, 4, 0) : new CornerRadius(4, 0, 0, 4)
                };
                Grid.SetColumn(bar, 1);
                fill.Children.Add(bar);
            }
            track.Child = fill;
            host.Children.Add(track);
            host.Children.Add(new Border { Width = 1, Height = 12, HorizontalAlignment = HorizontalAlignment.Center, Background = FindBrush("YtDisabledTextBrush") });
            Grid.SetColumn(host, 1);
            grid.Children.Add(host);

            var pct = Text(FormatPct(stat.Average), 12, FontWeights.SemiBold, ChangeBrush(stat.Average));
            pct.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetColumn(pct, 2);
            grid.Children.Add(pct);
            return grid;
        }

        //---------- Tabela de ativos ----------

        private void BuildAssetTabs()
        {
            for (int i = 0; i < AssetTabNames.Length; i++)
            {
                var button = new Button { Tag = i, Style = FindStyle(i == _assetTab ? "SegButtonActiveStyle" : "SegButtonStyle") };
                button.Content = new TextBlock { Text = AssetTabNames[i].Translate(), Style = FindStyle("InheritTextStyle") };
                button.Click += AssetTab_Click;
                assetTabsPanel.Children.Add(button);
                _assetTabButtons.Add(button);
            }
        }

        private void AssetTab_Click(object sender, RoutedEventArgs e)
        {
            _assetTab = (int)((Button)sender).Tag;
            for (int i = 0; i < _assetTabButtons.Count; i++)
                SetSeg(_assetTabButtons[i], i == _assetTab);
            BuildTable();
            RefreshTable(Market.Current);
        }

        private static void AddTableColumns(Grid grid)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(96) });
        }

        private static void AddHeaderText(Grid grid, int column, string text, bool right)
        {
            var block = new TextBlock { Text = text, Style = FindStyle("TableHeaderTextStyle") };
            if (right)
            {
                block.HorizontalAlignment = HorizontalAlignment.Right;
                block.Margin = new Thickness(0, 0, 8, 0);
            }
            Grid.SetColumn(block, column);
            grid.Children.Add(block);
        }

        //Monta cabeçalho e linhas da aba escolhida (só quando a aba muda); os valores são atualizados em RefreshTable
        private void BuildTable()
        {
            _tableDirty = false;
            var cls = (AssetClass)_assetTab;
            bool fixedIncome = cls == AssetClass.FixedIncome;

            var header = new Grid();
            AddTableColumns(header);
            AddHeaderText(header, 1, EnglishStrings.MarketColAsset.Translate(), false);
            AddHeaderText(header, 2, EnglishStrings.MarketColSector.Translate(), false);
            AddHeaderText(header, 3, (fixedIncome ? EnglishStrings.MarketColRate : EnglishStrings.MarketColPrice).Translate(), true);
            if (!fixedIncome)
            {
                AddHeaderText(header, 4, EnglishStrings.MarketColDay.Translate(), true);
                AddHeaderText(header, 5, EnglishStrings.MarketCol7Days.Translate(), true);
            }
            tableHeader.Child = header;

            tablePanel.Children.Clear();
            _tableRows.Clear();
            foreach (var def in AssetDef.All)
            {
                if (def.Class == cls)
                    tablePanel.Children.Add(MakeTableRow(def, fixedIncome));
            }
        }

        private UIElement MakeTableRow(AssetDef def, bool fixedIncome)
        {
            var refs = new RowRefs { Ticker = def.Ticker };
            var grid = new Grid();
            AddTableColumns(grid);

            //Estrela: favorita sem abrir o ativo
            refs.Star = new TextBlock { FontFamily = new FontFamily("Segoe UI Symbol"), FontSize = 16, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            var starBox = new Border { Background = Brushes.Transparent, Cursor = Cursors.Hand, Tag = def.Ticker, Width = 28, Height = 28, Child = refs.Star };
            starBox.MouseLeftButtonUp += Star_Click;
            grid.Children.Add(starBox);

            var asset = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var avatar = MakeAvatar(def, 28);
            ((FrameworkElement)avatar).Margin = new Thickness(0, 0, 10, 0);
            asset.Children.Add(avatar);
            asset.Children.Add(MakeNames(def, true));
            Grid.SetColumn(asset, 1);
            grid.Children.Add(asset);

            var sector = Text(def.Sector.Translate(), 12, FontWeights.Normal, FindBrush("YtTextSecondaryBrush"));
            sector.TextTrimming = TextTrimming.CharacterEllipsis;
            Grid.SetColumn(sector, 2);
            grid.Children.Add(sector);

            refs.Price = Text("", 13, FontWeights.Normal, null);
            refs.Price.HorizontalAlignment = HorizontalAlignment.Right;
            refs.Price.Margin = new Thickness(0, 0, 8, 0);
            Grid.SetColumn(refs.Price, 3);
            grid.Children.Add(refs.Price);

            if (fixedIncome)
            {
                //Renda fixa: taxa ao ano (não tem variação nem gráfico)
                refs.Price.Text = (def.AnnualRate * 100).ToString("0.00", Culture) + "%";
                refs.Price.FontWeight = FontWeights.Bold;
                refs.Price.Foreground = FindBrush("YtGainBrush");
            }
            else
            {
                refs.Change = Text("", 13, FontWeights.SemiBold, null);
                refs.Change.HorizontalAlignment = HorizontalAlignment.Right;
                refs.Change.Margin = new Thickness(0, 0, 8, 0);
                Grid.SetColumn(refs.Change, 4);
                grid.Children.Add(refs.Change);

                refs.Spark = NewSpark(64, 24);
                refs.Spark7 = NewSpark7();
                refs.Spark7.Width = 58;
                refs.Spark7.TextAlignment = TextAlignment.Right;
                refs.Spark7.Margin = new Thickness(6, 0, 8, 0);
                var sparkBox = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
                sparkBox.Children.Add(refs.Spark);
                sparkBox.Children.Add(refs.Spark7);
                Grid.SetColumn(sparkBox, 5);
                grid.Children.Add(sparkBox);
            }

            //Botão discreto: abre o ativo (a linha toda também é clicável)
            var action = new Button { Style = FindStyle("SmallButtonStyle"), Tag = def.Ticker, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
            action.Content = new TextBlock { Text = (fixedIncome ? EnglishStrings.MarketApply : EnglishStrings.MarketNegotiate).Translate(), Style = FindStyle("InheritTextStyle") };
            action.Click += Negotiate_Click;
            Grid.SetColumn(action, 6);
            grid.Children.Add(action);

            var row = new Border { Style = FindStyle("TableRowStyle"), Padding = new Thickness(12, 0, 12, 0), MinHeight = 46, Cursor = Cursors.Hand, Child = grid, Tag = def.Ticker };
            row.MouseLeftButtonUp += Row_Click;
            _tableRows.Add(refs);
            return row;
        }

        private void RefreshTable(Market m)
        {
            foreach (var row in _tableRows)
            {
                var def = AssetDef.Find(row.Ticker);
                var state = m.Get(row.Ticker);
                if (def == null || state == null)
                    continue;

                bool favorite = m.IsFavorite(row.Ticker);
                row.Star.Text = favorite ? "★" : "☆";
                row.Star.Foreground = FindBrush(favorite ? "YtFavoriteBrush" : "YtDisabledTextBrush");

                if (row.Change == null)
                    continue;
                double change = DayChange(state);
                row.Price.Text = FormatPrice(state.Price, false);
                row.Change.Text = FormatPct(change);
                row.Change.Foreground = ChangeBrush(change);
                UpdateSpark(row.Spark, row.Spark7, state.Daily);
            }
        }

        private void Negotiate_Click(object sender, RoutedEventArgs e)
        {
            OpenAsset((string)((FrameworkElement)sender).Tag);
        }

        private void Star_Click(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Market.Current.ToggleFavorite((string)((FrameworkElement)sender).Tag);
            RefreshTable(Market.Current);
        }

        private void Row_Click(object sender, MouseButtonEventArgs e)
        {
            OpenAsset((string)((FrameworkElement)sender).Tag);
        }

        //---------- Tela do ativo ----------

        private void Period_Click(object sender, RoutedEventArgs e)
        {
            _period = Int32.Parse(((Button)sender).Tag.ToString(), Culture);
            RefreshAsset(Market.Current);
        }

        private void Fav_Click(object sender, RoutedEventArgs e)
        {
            var m = Market.Current;
            m.ToggleFavorite(_current);
            RefreshAsset(m);
        }

        private void FavItem_Click(object sender, MouseButtonEventArgs e)
        {
            _current = (string)((FrameworkElement)sender).Tag;
            SetMode(false);
            txtTradeNote.Visibility = Visibility.Collapsed;
            RefreshAsset(Market.Current);
        }

        private List<double> PeriodSeries(AssetState state)
        {
            if (_period == 3)
                return state.Weekly;
            int days = _period == 0 ? 7 : (_period == 1 ? 28 : 112);
            if (state.Daily.Count <= days)
                return state.Daily;
            return state.Daily.GetRange(state.Daily.Count - days, days);
        }

        private string PeriodName()
        {
            if (_period == 0) return "7d";
            if (_period == 1) return "28d";
            if (_period == 2) return "112d";
            return EnglishStrings.MarketPeriodAll.Translate();
        }

        private void RefreshAsset(Market m)
        {
            var def = AssetDef.Find(_current);
            var state = m.Get(_current);
            if (def == null || state == null)
            {
                SetView(View.Market);
                Update();
                return;
            }

            bool fixedIncome = def.Class == AssetClass.FixedIncome;
            bool favorite = m.IsFavorite(_current);

            avatarHost.Child = MakeAvatar(def, 40);
            txtAssetName.Text = def.DisplayName();
            txtAssetSub.Text = def.Ticker + " · " + def.TypeLabel();
            txtAssetPrice.Text = FormatPrice(state.Price, fixedIncome);

            double today = DayChange(state);
            txtPillToday.Text = String.Format(EnglishStrings.MarketChangeToday.Translate(), FormatPct(today));
            pillToday.Style = FindStyle(ChangeStyleKey(today));

            txtPillSector.Text = String.Format(EnglishStrings.MarketSectorLabel.Translate(), def.Sector.Translate());
            pillRisky.Visibility = def.Risky ? Visibility.Visible : Visibility.Collapsed;

            txtFavBtn.Text = (favorite ? "★ " : "☆ ") + (favorite ? EnglishStrings.MarketFavorited : EnglishStrings.MarketFavorite).Translate();
            btnFav.Foreground = FindBrush(favorite ? "YtFavoriteBrush" : "YtTextPrimaryBrush");
            btnFav.BorderBrush = FindBrush(favorite ? "YtFavoriteBrush" : "YtDarkBorderBrush");

            SetSeg(btnP7, _period == 0);
            SetSeg(btnP28, _period == 1);
            SetSeg(btnP112, _period == 2);
            SetSeg(btnPAll, _period == 3);

            //Gráfico do período e variação nele
            var series = PeriodSeries(state);
            _chartValues = new List<double>(series);
            _chartFixed = fixedIncome;
            if (series.Count >= 2 && series[0] > 0)
            {
                double change = (series[series.Count - 1] - series[0]) / series[0] * 100;
                txtPillPeriod.Text = String.Format(EnglishStrings.MarketChangePeriod.Translate(), FormatPct(change), PeriodName());
                pillPeriod.Style = FindStyle(ChangeStyleKey(change));
                pillPeriod.Visibility = Visibility.Visible;
            }
            else
            {
                pillPeriod.Visibility = Visibility.Collapsed;
            }
            DrawChart();

            txtTradeTitle.Text = String.Format(EnglishStrings.MarketTrade.Translate(), def.Ticker);
            ApplyModeTexts(def);
            RefreshPosition(def, state);
            Recalc();

            RefreshFavorites(m);
        }

        //---------- Comprar / Vender ----------

        private bool _sellMode;
        private bool _busy;
        private TradeQuote _quote;
        private string _sellAllText; //texto posto pelo botão 100% da venda (vende a posição inteira)

        private static string FormatQty(double qty)
        {
            return qty.ToString("0.########", CultureInfo.InvariantCulture);
        }

        //Textos que dependem do modo (comprar/vender) e do ativo
        private void ApplyModeTexts(AssetDef def)
        {
            SetSeg(btnBuy, !_sellMode);
            SetSeg(btnSell, _sellMode);
            txtAmountLabel.Text = (_sellMode ? EnglishStrings.MarketQuantity : EnglishStrings.MarketAmountUsd).Translate();
            txtTotalLabel.Text = (_sellMode ? EnglishStrings.MarketReceive : EnglishStrings.MarketPay).Translate();
            rowTax.Visibility = _sellMode ? Visibility.Visible : Visibility.Collapsed;
            txtTradeBtn.Text = String.Format((_sellMode ? EnglishStrings.MarketSellAsset : EnglishStrings.MarketBuyAsset).Translate(), def.Ticker);

            Holding h = Market.Current.GetHolding(def.Ticker);
            if (_sellMode)
                txtAvailable.Text = String.Format(EnglishStrings.MarketYouHave.Translate(), FormatQty(h == null ? 0 : h.Quantity) + " " + def.Ticker);
            else
                txtAvailable.Text = String.Format(EnglishStrings.MarketAvailableCash.Translate(), Player.Current.Money.ToCurrencyString());
        }

        private void SetMode(bool sell)
        {
            _sellMode = sell;
            _sellAllText = null;
            boxAmount.Text = "";
            txtTradeNote.Visibility = Visibility.Collapsed;
            var def = AssetDef.Find(_current);
            if (def != null)
                ApplyModeTexts(def);
            Recalc();
        }

        private void BoxAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtTradeNote.Visibility = Visibility.Collapsed;
            Recalc();
        }

        private void ClearSummary()
        {
            _quote = null;
            txtGross.Text = "—";
            txtFee.Text = "—";
            txtTax.Text = "—";
            txtTotal.Text = "—";
            txtQtyPreview.Text = "";
            btnTrade.IsEnabled = false;
        }

        private void ShowTradeError(string text)
        {
            txtTradeError.Text = text;
            txtTradeError.Visibility = Visibility.Visible;
        }

        private void Recalc()
        {
            var def = AssetDef.Find(_current);
            if (def == null || Market.Current.Get(_current) == null)
                return;

            Holding h = Market.Current.GetHolding(_current);
            bool locked = Trading.IsLocked(def);
            bool noPosition = _sellMode && h == null;
            bool pctOn = !locked && !noPosition;
            tradePanel.IsEnabled = !locked;
            btnPct25.IsEnabled = pctOn;
            btnPct50.IsEnabled = pctOn;
            btnPct75.IsEnabled = pctOn;
            btnPct100.IsEnabled = pctOn;
            txtTradeError.Visibility = Visibility.Collapsed;
            ClearSummary();

            if (locked)
            {
                ShowTradeError(EnglishStrings.MarketErrLocked.Translate());
                return;
            }
            if (noPosition)
            {
                ShowTradeError(String.Format(EnglishStrings.MarketNoHolding.Translate(), def.Ticker));
                return;
            }

            double value;
            string text = boxAmount.Text.Trim().Replace(',', '.');
            if (text.Length == 0 || !Double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return;
            if (Double.IsNaN(value) || Double.IsInfinity(value) || value <= 0)
                return;

            TradeQuote q;
            if (!_sellMode)
                q = Trading.QuoteBuyByValue(_current, value);
            else if (_sellAllText != null && boxAmount.Text == _sellAllText)
                q = Trading.QuoteSellAll(_current);
            else
                q = Trading.QuoteSell(_current, value);

            if (q.Quantity > 0)
            {
                txtGross.Text = q.Gross.ToCurrencyString();
                txtFee.Text = q.Fee.ToCurrencyString();
                txtTax.Text = q.Tax.ToCurrencyString();
                txtTotal.Text = q.Total.ToCurrencyString();
                if (!_sellMode)
                    txtQtyPreview.Text = String.Format(EnglishStrings.MarketEstQty.Translate(), FormatQty(q.Quantity), def.Ticker);
            }
            if (q.Error != TradeError.None)
            {
                ShowTradeError(q.ErrorText());
                return;
            }
            _quote = q;
            btnTrade.IsEnabled = true;
        }

        private void Pct_Click(object sender, RoutedEventArgs e)
        {
            int pct = Int32.Parse(((Button)sender).Tag.ToString(), CultureInfo.InvariantCulture);
            string text;
            _sellAllText = null;
            if (_sellMode)
            {
                Holding h = Market.Current.GetHolding(_current);
                if (h == null)
                    return;
                double qty = pct == 100 ? h.Quantity : Math.Floor(h.Quantity * pct / 100 * 1e8 + 1e-6) / 1e8;
                if (qty <= 0)
                    return;
                text = FormatQty(qty);
                if (pct == 100)
                    _sellAllText = text;
            }
            else
            {
                double money = Player.Current.Money;
                if (money <= 0)
                    return;
                //Arredonda para baixo em centavos para nunca passar do caixa
                double value = Math.Floor(money * pct / 100 * 100 + 1e-6) / 100;
                text = value.ToString("0.00", CultureInfo.InvariantCulture);
            }
            boxAmount.Text = text;
            Recalc();
        }

        private void Trade_Click(object sender, RoutedEventArgs e)
        {
            if (_busy)
                return;
            _busy = true;
            btnTrade.IsEnabled = false;
            try
            {
                TradeQuote quote = _quote;
                if (quote == null)
                    return;
                var def = AssetDef.Find(quote.Ticker);
                TradeQuote r = quote.IsBuy ? Trading.Buy(quote) : Trading.Sell(quote);
                if (r.Error == TradeError.None)
                {
                    //O dinheiro do topo muda sozinho: Player.Money dispara MoneyChanged, que o MainPage escuta
                    _sellAllText = null;
                    boxAmount.Text = "";
                    RefreshAsset(Market.Current);
                    string tickerName = def == null ? r.Ticker : def.Ticker;
                    txtTradeNote.Text = String.Format((r.IsBuy ? EnglishStrings.MarketBuyDone : EnglishStrings.MarketSellDone).Translate(),
                        FormatQty(r.Quantity), tickerName, r.Total.ToCurrencyString());
                    txtTradeNote.Visibility = Visibility.Visible;
                }
                else
                {
                    RefreshAsset(Market.Current);
                    ShowTradeError(r.ErrorText());
                }
            }
            finally
            {
                _busy = false;
            }
        }

        //---------- Sua posição ----------

        private void RefreshPosition(AssetDef def, AssetState state)
        {
            Holding h = Market.Current.GetHolding(def.Ticker);
            if (h == null)
            {
                posDetails.Visibility = Visibility.Collapsed;
                txtNoPosition.Visibility = Visibility.Visible;
                txtNoPosition.Text = String.Format(EnglishStrings.MarketNoHolding.Translate(), def.Ticker);
                return;
            }
            posDetails.Visibility = Visibility.Visible;
            txtNoPosition.Visibility = Visibility.Collapsed;
            txtPosQty.Text = FormatQty(h.Quantity) + " " + def.Ticker;
            txtPosAvg.Text = FormatPrice(h.AvgPrice, def.Class == AssetClass.FixedIncome);
            txtPosInvested.Text = h.TotalInvested.ToCurrencyString();
            double change = h.AvgPrice > 0 ? (state.Price / h.AvgPrice - 1) * 100 : 0;
            txtPosChange.Text = FormatPct(change);
            posChangePill.Style = FindStyle(ChangeStyleKey(change));
        }

        private void RefreshFavorites(Market m)
        {
            favList.Children.Clear();
            txtFavCount.Text = String.Format(EnglishStrings.MarketFavoritesCount.Translate(), m.Favorites.Count);
            txtFavHint.Visibility = m.Favorites.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            foreach (string ticker in m.Favorites)
            {
                var def = AssetDef.Find(ticker);
                var state = m.Get(ticker);
                if (def == null || state == null)
                    continue;

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                bool selected = ticker == _current;
                var accent = new Border { Background = selected ? FindBrush("YtRedBrush") : Brushes.Transparent };
                grid.Children.Add(accent);

                var avatar = MakeAvatar(def, 28);
                ((FrameworkElement)avatar).Margin = new Thickness(9, 0, 0, 0);
                Grid.SetColumn(avatar, 1);
                grid.Children.Add(avatar);

                var names = MakeNames(def, false);
                names.Margin = new Thickness(8, 0, 8, 0);
                Grid.SetColumn(names, 2);
                grid.Children.Add(names);

                double change = DayChange(state);
                var prices = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0) };
                var price = Text(FormatPrice(state.Price, def.Class == AssetClass.FixedIncome), 12, FontWeights.SemiBold, null);
                price.HorizontalAlignment = HorizontalAlignment.Right;
                prices.Children.Add(price);
                var pct = Text(FormatPct(change), 11, FontWeights.Normal, ChangeBrush(change));
                pct.HorizontalAlignment = HorizontalAlignment.Right;
                prices.Children.Add(pct);
                Grid.SetColumn(prices, 3);
                grid.Children.Add(prices);

                var item = new Border { Style = FindStyle("TableRowStyle"), Padding = new Thickness(0, 8, 0, 8), Cursor = Cursors.Hand, Child = grid, Tag = ticker };
                if (selected)
                    item.Background = FindBrush("YtDarkCardHoverBrush");
                item.MouseLeftButtonUp += FavItem_Click;
                favList.Children.Add(item);
            }
        }

        //---------- Gráfico do ativo ----------

        private void AssetChart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawChart();
        }

        private void DrawChart()
        {
            assetChart.Children.Clear();
            bool enough = _chartValues.Count >= 2;
            txtChartEmpty.Visibility = enough ? Visibility.Collapsed : Visibility.Visible;

            double width = assetChart.ActualWidth;
            double height = assetChart.ActualHeight;
            if (!enough || width < 150 || height < 60)
                return;

            double min = _chartValues[0];
            double max = _chartValues[0];
            foreach (double value in _chartValues)
            {
                if (value < min) min = value;
                if (value > max) max = value;
            }
            //Série constante não pode dividir por zero: abre uma faixa de ±5% em volta do valor
            if (max - min <= 0)
            {
                double pad = Math.Abs(max) * 0.05;
                if (pad <= 0) pad = 1;
                min -= pad;
                max += pad;
            }
            else
            {
                double pad = (max - min) * 0.12;
                min -= pad;
                max += pad;
            }
            double range = max - min;

            double left = 10, right = 88, top = 12, bottom = 12;
            double plotWidth = width - left - right;
            double plotHeight = height - top - bottom;

            //Linhas de grade com o preço ao lado
            for (int i = 0; i < 5; i++)
            {
                double y = top + plotHeight * i / 4;
                assetChart.Children.Add(new Line { X1 = left, X2 = left + plotWidth, Y1 = y, Y2 = y, Stroke = FindBrush("YtDarkBorderBrush"), StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 3, 4 } });

                var label = new TextBlock { Text = FormatPrice(max - range * i / 4, _chartFixed), Foreground = FindBrush("YtTextSecondaryBrush"), FontSize = 11 };
                Canvas.SetLeft(label, left + plotWidth + 8);
                Canvas.SetTop(label, y - 8);
                assetChart.Children.Add(label);
            }

            Brush color = TrendBrush(_chartValues[_chartValues.Count - 1] - _chartValues[0]);

            var polyline = new Polyline { Stroke = color, StrokeThickness = 2.5, StrokeLineJoin = PenLineJoin.Round };
            double lastX = 0, lastY = 0;
            for (int i = 0; i < _chartValues.Count; i++)
            {
                double x = left + plotWidth * i / (_chartValues.Count - 1);
                double y = top + plotHeight * (max - _chartValues[i]) / range;
                polyline.Points.Add(new Point(x, y));
                lastX = x;
                lastY = y;
            }
            assetChart.Children.Add(polyline);

            var dot = new Ellipse { Width = 9, Height = 9, Fill = color };
            Canvas.SetLeft(dot, lastX - 4.5);
            Canvas.SetTop(dot, lastY - 4.5);
            assetChart.Children.Add(dot);
        }
    }
}
