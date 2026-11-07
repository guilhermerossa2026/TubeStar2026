using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace TubeStar
{
    public enum TradeError { None, UnknownAsset, Locked, InvalidAmount, BelowMinimum, NotEnoughMoney, NotEnoughQuantity, NoPosition, AlreadySent }

    /// <summary>
    /// Simulação de uma compra ou venda (só números, não altera nada)
    /// </summary>
    public class TradeQuote
    {
        public string Ticker { get; set; }
        public bool IsBuy { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        //Valor das unidades, sem taxas
        public double Gross { get; set; }
        //Corretagem
        public double Fee { get; set; }
        //Imposto sobre o lucro (só venda)
        public double Tax { get; set; }
        //Lucro da venda (pode ser negativo)
        public double Profit { get; set; }
        //Compra: sai do caixa. Venda: entra no caixa
        public double Total { get; set; }
        public TradeError Error { get; set; }
        //Já foi executada (evita clique duplo)
        public bool Used { get; set; }

        public string ErrorText()
        {
            switch (Error)
            {
                case TradeError.UnknownAsset: return EnglishStrings.MarketErrUnknown.Translate();
                case TradeError.Locked: return EnglishStrings.MarketErrLocked.Translate();
                case TradeError.InvalidAmount: return EnglishStrings.MarketErrInvalid.Translate();
                case TradeError.BelowMinimum: return EnglishStrings.MarketErrMinimum.Translate();
                case TradeError.NotEnoughMoney: return EnglishStrings.MarketErrNoMoney.Translate();
                case TradeError.NotEnoughQuantity: return EnglishStrings.MarketErrNoQuantity.Translate();
                case TradeError.NoPosition: return EnglishStrings.MarketErrNoPosition.Translate();
                case TradeError.AlreadySent: return EnglishStrings.MarketErrSent.Translate();
                default: return "";
            }
        }
    }

    /// <summary>
    /// Regras de compra e venda de ativos
    /// </summary>
    public static class Trading
    {
        public const double MinOrder = 10;
        public const int QtyDecimals = 8;
        private const double QtyScale = 100000000.0;
        private const double StockFee = 2;
        private const double CryptoFeeRate = 0.005;
        private const double TaxRate = 0.15;

        private static double Round2(double x)
        {
            return Math.Round(x, 2, MidpointRounding.AwayFromZero);
        }

        private static double Floor8(double x)
        {
            return Math.Floor(x * QtyScale + 1e-6) / QtyScale;
        }

        public static double FeeFor(AssetDef def, double gross)
        {
            switch (def.Class)
            {
                case AssetClass.Crypto: return Round2(gross * CryptoFeeRate);
                case AssetClass.FixedIncome: return 0;
                default: return StockFee; //ações BR/EUA e fundos
            }
        }

        //CDB Banco Médio fica bloqueado até a M4
        public static bool IsLocked(AssetDef def)
        {
            return def.Ticker == "CDB-M";
        }

        private static bool IsBad(double x)
        {
            return double.IsNaN(x) || double.IsInfinity(x);
        }

        private static TradeQuote Fail(string ticker, bool isBuy, TradeError error)
        {
            return new TradeQuote { Ticker = ticker, IsBuy = isBuy, Error = error };
        }

        //Confere ativo e bloqueio; devolve o erro ou None
        private static TradeError CheckAsset(string ticker, out AssetDef def)
        {
            def = ticker == null ? null : AssetDef.Find(ticker);
            if (def == null) return TradeError.UnknownAsset;
            if (IsLocked(def)) return TradeError.Locked;
            return TradeError.None;
        }

        public static TradeQuote QuoteBuyByValue(string ticker, double total)
        {
            AssetDef def;
            TradeError err = CheckAsset(ticker, out def);
            if (err != TradeError.None) return Fail(ticker, true, err);
            if (IsBad(total) || total <= 0) return Fail(ticker, true, TradeError.InvalidAmount);

            double price = Market.Current.Get(ticker).Price;
            double gross;
            if (def.Class == AssetClass.Crypto) gross = total / (1 + CryptoFeeRate);
            else if (def.Class == AssetClass.FixedIncome) gross = total;
            else gross = total - StockFee;
            if (gross <= 0) return Fail(ticker, true, TradeError.BelowMinimum);

            double qty = Floor8(gross / price);
            TradeQuote bq = BuildBuy(def, qty, price);

            //Arredondamentos separados podem passar o Total em 1 centavo; reduz a quantidade até caber
            double step = Math.Max(1e-8, 0.01 / price);
            //(só roda se o único problema é o excesso de Total; outros erros ficam como estão)
            for (int i = 0; i < 100 && qty > 0 && bq.Quantity > 0 && bq.Total > total + 1e-9
                && (bq.Error == TradeError.None || bq.Error == TradeError.NotEnoughMoney); i++)
            {
                qty = Floor8(qty - step);
                if (qty <= 0) break;
                bq = BuildBuy(def, qty, price);
            }
            return bq;
        }

        public static TradeQuote QuoteBuyByQuantity(string ticker, double qty)
        {
            AssetDef def;
            TradeError err = CheckAsset(ticker, out def);
            if (err != TradeError.None) return Fail(ticker, true, err);
            if (IsBad(qty) || qty <= 0) return Fail(ticker, true, TradeError.InvalidAmount);

            return BuildBuy(def, Floor8(qty), Market.Current.Get(ticker).Price);
        }

        private static TradeQuote BuildBuy(AssetDef def, double qty, double price)
        {
            if (qty <= 0 || IsBad(price) || price <= 0) return Fail(def.Ticker, true, TradeError.InvalidAmount);
            var q = new TradeQuote { Ticker = def.Ticker, IsBuy = true, Quantity = qty, Price = price };
            q.Gross = Round2(qty * price);
            q.Fee = FeeFor(def, q.Gross);
            q.Total = Round2(q.Gross + q.Fee);
            if (IsBad(q.Total)) return Fail(def.Ticker, true, TradeError.InvalidAmount);
            //O mínimo vale sobre o valor total digitado (com corretagem)
            if (q.Total < MinOrder - 0.005) q.Error = TradeError.BelowMinimum;
            else if (!(q.Total <= Player.Current.Money + 0.005)) q.Error = TradeError.NotEnoughMoney;
            return q;
        }

        public static TradeQuote QuoteSellAll(string ticker)
        {
            AssetDef def;
            TradeError err = CheckAsset(ticker, out def);
            if (err != TradeError.None) return Fail(ticker, false, err);
            Holding h = Market.Current.GetHolding(ticker);
            if (h == null) return Fail(ticker, false, TradeError.NoPosition);
            return QuoteSell(ticker, h.Quantity);
        }

        public static TradeQuote QuoteSell(string ticker, double qty)
        {
            AssetDef def;
            TradeError err = CheckAsset(ticker, out def);
            if (err != TradeError.None) return Fail(ticker, false, err);
            if (IsBad(qty) || qty <= 0) return Fail(ticker, false, TradeError.InvalidAmount);

            Market market = Market.Current;
            Holding h = market.GetHolding(ticker);
            //Venda de 100% da posição: decide antes do Floor8, usa a quantidade exata; o mínimo de $10 não vale
            bool all = h != null && Math.Abs(qty - h.Quantity) <= 1e-9;
            if (all) qty = h.Quantity;
            else qty = Floor8(qty);
            if (qty <= 0) return Fail(ticker, false, TradeError.InvalidAmount);

            double price = market.Get(ticker).Price;
            if (IsBad(price) || price <= 0) return Fail(ticker, false, TradeError.InvalidAmount);
            var q = new TradeQuote { Ticker = ticker, IsBuy = false, Quantity = qty, Price = price };
            q.Gross = Round2(qty * price);
            q.Fee = FeeFor(def, q.Gross);
            if (IsBad(q.Gross)) return Fail(ticker, false, TradeError.InvalidAmount);

            if (h == null) q.Error = TradeError.NoPosition;
            else if (q.Gross < MinOrder && !all) q.Error = TradeError.BelowMinimum;
            else if (qty > h.Quantity + 1e-9) q.Error = TradeError.NotEnoughQuantity;

            if (h != null)
            {
                double cost = qty * h.AvgPrice;
                q.Profit = Round2(q.Gross - q.Fee - cost);
                q.Tax = q.Profit > 0 ? Round2(q.Profit * TaxRate) : 0;
                q.Total = Math.Max(0, Round2(q.Gross - q.Fee - q.Tax));
                if (IsBad(q.Total) || IsBad(q.Profit)) return Fail(ticker, false, TradeError.InvalidAmount);
            }
            return q;
        }

        //Executa a compra. Recalcula tudo de novo (preço e caixa); devolve o quote final
        public static TradeQuote Buy(TradeQuote quote)
        {
            if (quote == null) return Fail(null, true, TradeError.InvalidAmount);
            if (quote.Used) return Fail(quote.Ticker, true, TradeError.AlreadySent);
            TradeQuote q = QuoteBuyByQuantity(quote.Ticker, quote.Quantity);
            if (q.Error != TradeError.None) return q;
            quote.Used = true;
            q.Used = true;

            Player player = Player.Current;
            Market market = Market.Current;
            player.Money -= q.Total;

            Holding h = market.GetHolding(q.Ticker);
            if (h == null)
            {
                h = new Holding { Ticker = q.Ticker, FirstBuyDay = player.Iterations };
                market.Holdings.Add(h);
            }
            //A corretagem entra no preço médio
            h.TotalInvested += Round2(q.Gross + q.Fee);
            h.Quantity += q.Quantity;
            h.AvgPrice = h.TotalInvested / h.Quantity;

            Record(market, TradeKind.Buy, q);
            return q;
        }

        //Executa a venda. Recalcula tudo de novo; devolve o quote final
        public static TradeQuote Sell(TradeQuote quote)
        {
            if (quote == null) return Fail(null, false, TradeError.InvalidAmount);
            if (quote.Used) return Fail(quote.Ticker, false, TradeError.AlreadySent);
            TradeQuote q = QuoteSell(quote.Ticker, quote.Quantity);
            if (q.Error != TradeError.None) return q;
            quote.Used = true;
            q.Used = true;

            Player player = Player.Current;
            Market market = Market.Current;
            Holding h = market.GetHolding(q.Ticker);
            bool all = Math.Abs(q.Quantity - h.Quantity) <= 1e-9;

            player.Money += q.Total;
            //O preço médio não muda na venda
            h.TotalInvested = Round2(h.TotalInvested - q.Quantity * h.AvgPrice);
            h.Quantity -= q.Quantity;
            if (all || h.Quantity < 1e-8)
                market.Holdings.Remove(h);

            Record(market, TradeKind.Sell, q);
            return q;
        }

        private static void Record(Market market, TradeKind kind, TradeQuote q)
        {
            var t = new MarketTrade
            {
                Kind = kind,
                Ticker = q.Ticker,
                Day = Player.Current.Iterations,
                Quantity = q.Quantity,
                Price = q.Price,
                Fee = q.Fee,
                Tax = q.Tax,
                Profit = q.Profit,
                Total = q.Total
            };
            market.AddTrade(t);
            AfterTrade(t);
        }

        private static void AfterTrade(MarketTrade t)
        {
            Player.Current.RecordInvestment(t);
        }

        // ---------------- Autoteste (sem tela) ----------------

        private static void Fresh()
        {
            var p = new Player();
            p.Money = 1000;
            p.Iterations = 0;
            p.Market = Market.Create();
            p.CurrentWeek = new PeriodStats();
            p.CurrentYear = new PeriodStats();
            Player.Current = p;
        }

        private static string N(double x)
        {
            return x.ToString("0.########", CultureInfo.InvariantCulture);
        }

        private static void Check(StringBuilder sb, string name, bool ok, string detail)
        {
            sb.AppendLine((ok ? "OK     " : "FALHOU ") + name + "  [" + detail + "]");
        }

        private static bool Near(double a, double b, double tol)
        {
            return Math.Abs(a - b) <= tol;
        }

        private static int Decimals(double x)
        {
            return Math.Abs(Floor8(x) - x) < 1e-12 ? 0 : QtyDecimals + 1;
        }

        //Roda os casos em um Player isolado e grava o resultado; o jogo real não é alterado
        public static void SelfTest(string path)
        {
            var sb = new StringBuilder();
            Player original = Player.Current;
            try
            {
                TradeQuote q, r;
                Holding h;
                Market m;

                //1. Comprar $100 de AAPL
                Fresh();
                q = QuoteBuyByValue("AAPL", 100);
                r = Buy(q);
                m = Market.Current;
                h = m.GetHolding("AAPL");
                Check(sb, "Comprar $100 AAPL", r.Error == TradeError.None && Near(r.Fee, 2, 0.001) && Near(Player.Current.Money, 900, 0.011) && h != null && Near(h.TotalInvested, 100, 0.011),
                    "erro=" + r.Error + " fee=" + N(r.Fee) + " caixa=" + N(Player.Current.Money) + " invest=" + (h == null ? "-" : N(h.TotalInvested)));
                Check(sb, "Compra gerou 1 linha em Trades", m.Trades.Count == 1 && m.Trades[0].Kind == TradeKind.Buy, "trades=" + m.Trades.Count);

                //1b. Contabilidade na Finanças
                {
                    PeriodStats w = Player.Current.CurrentWeek;
                    PeriodStats y = Player.Current.CurrentYear;
                    Check(sb, "Fin: compra -> InvestBought 100, Earned 0, Spent 0, caixa 900",
                        Near(w.InvestBought, 100, 0.011) && w.Earned == 0 && w.Spent == 0 && Near(Player.Current.Money, 900, 0.011) && Near(y.InvestBought, 100, 0.011),
                        "bought=" + N(w.InvestBought) + " earned=" + N(w.Earned) + " spent=" + N(w.Spent));
                    Check(sb, "Fin: patrimonio = caixa + carteira logo apos a compra",
                        Near(Player.Current.GetNetWorth(), Player.Current.Money + Market.Current.GetPortfolioValue(), 0.001) && Near(Market.Current.GetPortfolioValue(), 98, 0.011),
                        "pat=" + N(Player.Current.GetNetWorth()) + " carteira=" + N(Market.Current.GetPortfolioValue()));
                    Check(sb, "Fin: invariante apos a compra", Near(w.Earned - w.Spent - w.InvestBought + w.InvestReturned, Player.Current.Money - 1000, 0.01), "caixa=" + N(Player.Current.Money));

                    //Venda com lucro
                    m.Get("AAPL").Price *= 1.5;
                    r = Sell(QuoteSellAll("AAPL"));
                    double gross = Math.Round(r.Quantity * r.Price, 2, MidpointRounding.AwayFromZero);
                    double cost = Math.Round(gross - r.Fee - r.Profit, 2, MidpointRounding.AwayFromZero);
                    Check(sb, "Fin: venda com lucro",
                        r.Error == TradeError.None && r.Profit > 0 && Near(w.InvestGain, Math.Round(gross - cost, 2, MidpointRounding.AwayFromZero), 0.011) && Near(w.InvestFee, 2, 0.011)
                        && Near(w.InvestTax, r.Tax, 0.001) && Near(w.ChannelEarned, 0, 0.001) && w.InvestLoss == 0 && Near(w.InvestReturned, 100, 0.011),
                        "gain=" + N(w.InvestGain) + " fee=" + N(w.InvestFee) + " tax=" + N(w.InvestTax) + " canal=" + N(w.ChannelEarned) + " devolvido=" + N(w.InvestReturned));
                    Check(sb, "Fin: invariante apos venda com lucro", Near(w.Earned - w.Spent - w.InvestBought + w.InvestReturned, Player.Current.Money - 1000, 0.01) && Near(y.Earned, w.Earned, 0.001),
                        "caixa=" + N(Player.Current.Money));
                    Check(sb, "Fin: imposto do mercado nao toca TaxDebt/TotalTaxPaid", Player.Current.TaxDebt == 0 && Player.Current.TotalTaxPaid == 0 && w.TaxCost == 0,
                        "debt=" + N(Player.Current.TaxDebt) + " pago=" + N(Player.Current.TotalTaxPaid));
                    Check(sb, "Fin: LastDay nao muda", Player.Current.LastDay == null, "lastday=" + (Player.Current.LastDay == null ? "null" : "preenchido"));

                    //Venda com prejuizo
                    Fresh();
                    Buy(QuoteBuyByValue("AAPL", 100));
                    Market.Current.Get("AAPL").Price *= 0.5;
                    r = Sell(QuoteSellAll("AAPL"));
                    w = Player.Current.CurrentWeek;
                    Check(sb, "Fin: venda com prejuizo",
                        r.Error == TradeError.None && r.Profit < 0 && Near(w.InvestLoss + w.InvestFee, -r.Profit, 0.011) && w.InvestGain == 0
                        && Near(w.Earned - w.Spent - w.InvestBought + w.InvestReturned, Player.Current.Money - 1000, 0.01),
                        "loss=" + N(w.InvestLoss) + " fee=" + N(w.InvestFee) + " profit=" + N(r.Profit) + " caixa=" + N(Player.Current.Money));

                    //Venda 100% minuscula com Total travado em 0
                    Fresh();
                    Buy(QuoteBuyByValue("KO", 100));
                    Market.Current.Get("KO").Price *= 0.01;
                    r = Sell(QuoteSellAll("KO"));
                    w = Player.Current.CurrentWeek;
                    Check(sb, "Fin: venda minuscula com Total 0 fecha a conta",
                        r.Error == TradeError.None && r.Total == 0 && w.InvestGain == 0 && Near(w.Spent, w.InvestFee + w.InvestTax + w.InvestLoss, 0.001)
                        && Near(w.Earned - w.Spent - w.InvestBought + w.InvestReturned, Player.Current.Money - 1000, 0.01),
                        "total=" + N(r.Total) + " loss=" + N(w.InvestLoss) + " fee=" + N(w.InvestFee) + " caixa=" + N(Player.Current.Money));
                }

                //2. Abaixo do mínimo, sem dinheiro, bloqueado, desconhecido
                Fresh();
                q = QuoteBuyByValue("AAPL", 9);
                Check(sb, "$9 AAPL -> BelowMinimum", q.Error == TradeError.BelowMinimum, "erro=" + q.Error);
                q = QuoteBuyByValue("AAPL", 10);
                Check(sb, "$10 AAPL -> OK ($8 + $2 taxa)", q.Error == TradeError.None && Near(q.Gross, 8, 0.011) && Near(q.Fee, 2, 0.001), "erro=" + q.Error + " gross=" + N(q.Gross) + " fee=" + N(q.Fee));
                q = QuoteBuyByValue("AAPL", 2000);
                Check(sb, "$2000 AAPL -> NotEnoughMoney", q.Error == TradeError.NotEnoughMoney, "erro=" + q.Error);
                q = QuoteBuyByValue("CDB-M", 100);
                Check(sb, "CDB Banco Medio -> Locked", q.Error == TradeError.Locked, "erro=" + q.Error);
                q = QuoteBuyByValue("XYZ", 100);
                Check(sb, "XYZ -> UnknownAsset", q.Error == TradeError.UnknownAsset, "erro=" + q.Error);
                q = QuoteBuyByValue("AAPL", double.NaN);
                Check(sb, "NaN -> InvalidAmount", q.Error == TradeError.InvalidAmount, "erro=" + q.Error);

                //3. Cripto
                Fresh();
                r = Buy(QuoteBuyByValue("BTC", 100));
                h = Market.Current.GetHolding("BTC");
                Check(sb, "Comprar $100 BTC", r.Error == TradeError.None && Near(r.Fee, 0.5, 0.011) && Decimals(r.Quantity) == 0 && Near(Player.Current.Money, 900, 0.011),
                    "erro=" + r.Error + " fee=" + N(r.Fee) + " qty=" + N(r.Quantity) + " caixa=" + N(Player.Current.Money));

                //3b. Cripto: Total nunca passa do valor pedido
                foreach (double want in new double[] { 11.05, 31.15 })
                {
                    Fresh();
                    Player.Current.Money = want;
                    q = QuoteBuyByValue("BTC", want);
                    Check(sb, "BTC por " + N(want) + " com caixa igual",
                        q.Error == TradeError.None && q.Total <= want + 1e-9, "erro=" + q.Error + " total=" + N(q.Total));
                }
                Fresh();
                q = QuoteBuyByValue("BTC", 1.00);
                Check(sb, "BTC por 1,00 -> BelowMinimum", q.Error == TradeError.BelowMinimum, "erro=" + q.Error);
                int cryptoFails = 0;
                for (int cents = 1000; cents <= 20000; cents++)
                {
                    double want = cents / 100.0;
                    Fresh();
                    Player.Current.Money = want;
                    q = QuoteBuyByValue("BTC", want);
                    if (q.Error != TradeError.None || q.Total > want + 1e-9) cryptoFails++;
                }
                Check(sb, "BTC de 10,00 a 200,00 centavo a centavo", cryptoFails == 0, "falhas=" + cryptoFails);

                //4. Selic: 112 dias
                Fresh();
                r = Buy(QuoteBuyByValue("SELIC", 100));
                m = Market.Current;
                for (int d = 1; d <= 112; d++) m.NewDay(d);
                Player.Current.Iterations = 112;
                q = QuoteSellAll("SELIC");
                double profitPct = q.Profit / 100.0;
                Check(sb, "Selic 112 dias: lucro ~14%", q.Error == TradeError.None && profitPct > 0.13 && profitPct < 0.15, "lucro=" + N(q.Profit));
                Check(sb, "Selic: imposto 15% do lucro e fee 0", Near(q.Tax, Math.Round(q.Profit * 0.15, 2, MidpointRounding.AwayFromZero), 0.0051) && q.Fee == 0, "imposto=" + N(q.Tax) + " fee=" + N(q.Fee));
                double before = Player.Current.Money;
                r = Sell(q);
                Check(sb, "Selic: vender tudo", r.Error == TradeError.None && m.GetHolding("SELIC") == null && Near(Player.Current.Money, before + r.Total, 0.001),
                    "erro=" + r.Error + " total=" + N(r.Total) + " caixa=" + N(Player.Current.Money));

                //5. Ação +50%, vender metade
                Fresh();
                Buy(QuoteBuyByValue("AAPL", 100));
                m = Market.Current;
                h = m.GetHolding("AAPL");
                double avg = h.AvgPrice;
                m.Get("AAPL").Price *= 1.5;
                q = QuoteSell("AAPL", h.Quantity / 2);
                r = Sell(q);
                h = m.GetHolding("AAPL");
                Check(sb, "Vender metade com +50%", r.Error == TradeError.None && h != null && Near(h.AvgPrice, avg, 1e-9) && r.Profit > 0 && Near(r.Tax, Math.Round(r.Profit * 0.15, 2, MidpointRounding.AwayFromZero), 0.0051),
                    "erro=" + r.Error + " profit=" + N(r.Profit) + " tax=" + N(r.Tax) + " avg=" + N(avg) + "->" + (h == null ? "-" : N(h.AvgPrice)));

                //6. Prejuízo
                Fresh();
                Buy(QuoteBuyByValue("AAPL", 100));
                m = Market.Current;
                m.Get("AAPL").Price *= 0.5;
                r = Sell(QuoteSellAll("AAPL"));
                Check(sb, "Prejuizo: Tax 0 e Total >= 0", r.Error == TradeError.None && r.Profit < 0 && r.Tax == 0 && r.Total >= 0,
                    "erro=" + r.Error + " profit=" + N(r.Profit) + " tax=" + N(r.Tax) + " total=" + N(r.Total));

                //7. Venda 100% de posição abaixo de $10
                Fresh();
                q = QuoteSell("AAPL", 0.01);
                Check(sb, "Vender 0,01 AAPL sem posicao -> NoPosition", q.Error == TradeError.NoPosition, "erro=" + q.Error);
                Buy(QuoteBuyByValue("KO", 100));
                m = Market.Current;
                m.Get("KO").Price *= 0.05;
                h = m.GetHolding("KO");
                q = QuoteSell("KO", h.Quantity / 2);
                Check(sb, "Venda parcial < $10 -> BelowMinimum", q.Error == TradeError.BelowMinimum, "erro=" + q.Error);
                r = Sell(QuoteSellAll("KO"));
                Check(sb, "Venda 100% < $10 permitida", r.Error == TradeError.None && m.GetHolding("KO") == null, "erro=" + r.Error + " total=" + N(r.Total));

                //8. Vender 2x com o mesmo quote
                Fresh();
                Buy(QuoteBuyByValue("AAPL", 100));
                q = QuoteSellAll("AAPL");
                r = Sell(q);
                TradeQuote r2 = Sell(q);
                Check(sb, "Sell 2x com o mesmo quote: 2a falha", r.Error == TradeError.None && r2.Error != TradeError.None, "1a=" + r.Error + " 2a=" + r2.Error);
                Check(sb, "Trades: uma linha por operacao", Market.Current.Trades.Count == 2, "trades=" + Market.Current.Trades.Count);

                //8b. Comprar 2x com o mesmo quote
                Fresh();
                q = QuoteBuyByValue("AAPL", 100);
                r = Buy(q);
                r2 = Buy(q);
                Check(sb, "Buy 2x mesmo quote: 2a falha, caixa debitado uma vez", r.Error == TradeError.None && r2.Error == TradeError.AlreadySent && Near(Player.Current.Money, 900, 0.011),
                    "1a=" + r.Error + " 2a=" + r2.Error + " caixa=" + N(Player.Current.Money));

                //9. Vender sem posição / quantidade demais
                Fresh();
                q = QuoteSell("AAPL", 1);
                Check(sb, "Vender sem posicao -> NoPosition", q.Error == TradeError.NoPosition, "erro=" + q.Error);
                Buy(QuoteBuyByValue("AAPL", 100));
                q = QuoteSell("AAPL", 5);
                Check(sb, "Vender mais do que tem -> NotEnoughQuantity", q.Error == TradeError.NotEnoughQuantity, "erro=" + q.Error);

                //10. Comprar por valor e vender 100%: nao pode sobrar poeira
                foreach (string tk in new string[] { "DOGE", "PEPE" })
                {
                    int dust = 0, errs = 0;
                    for (int cents = 1000; cents <= 20000; cents++)
                    {
                        double want = cents / 100.0;
                        Fresh();
                        Player.Current.Money = 1000000;
                        r = Buy(QuoteBuyByValue(tk, want));
                        if (r.Error != TradeError.None) { errs++; continue; }
                        r = Sell(QuoteSellAll(tk));
                        if (r.Error != TradeError.None) errs++;
                        if (Market.Current.GetHolding(tk) != null) dust++;
                    }
                    Check(sb, tk + " compra 10,00 a 200,00 e vende 100%: sem sobra", dust == 0 && errs == 0, "sobras=" + dust + " erros=" + errs);
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("FALHOU erro inesperado: " + ex);
            }
            finally
            {
                Player.Current = original;
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }
    }
}
