using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace TubeStar
{
    /// <summary>
    /// Mercado financeiro: preços dos ativos (vai no save, dentro do Player)
    /// </summary>
    public class Market
    {
        public List<AssetState> Assets { get; set; }
        //Último dia simulado
        public int LastDay { get; set; }
        //Gancho para crash do mercado (M5)
        public List<int> CrashDaysLeft { get; set; }
        //Tickers favoritados (lista simples para o save)
        public List<string> Favorites { get; set; }

        //Posições do jogador
        public List<Holding> Holdings { get; set; }
        //Histórico de operações (só as mais recentes)
        public List<MarketTrade> Trades { get; set; }
        public const int MaxTrades = 200;

        //Lucro realizado acumulado (líquido de imposto)
        public double RealizedProfit { get; set; }
        //Soma de todas as compras (com corretagem)
        public double TotalApplied { get; set; }
        //Os totais acima já foram iniciados a partir dos Trades (saves antigos vêm false)
        public bool StatsSeeded { get; set; }

        public Market()
        {
            Assets = new List<AssetState>();
            LastDay = -1;
            CrashDaysLeft = new List<int>();
            Favorites = new List<string>();
            Holdings = new List<Holding>();
            Trades = new List<MarketTrade>();
        }

        public Holding GetHolding(string ticker)
        {
            for (int i = 0; i < Holdings.Count; i++)
                if (Holdings[i].Ticker == ticker) return Holdings[i];
            return null;
        }

        //Valor da carteira a preço de mercado (sem preço válido, usa o investido)
        public double GetPortfolioValue()
        {
            Sync(); //repara save corrompido (listas/itens nulos) antes de calcular
            double total = 0;
            for (int i = 0; i < Holdings.Count; i++)
                total += GetHoldingValue(Holdings[i]);
            return total;
        }

        //Valor de uma posição a preço de mercado (sem preço válido, usa o investido)
        public double GetHoldingValue(Holding h)
        {
            Sync();
            AssetState a = Get(h.Ticker);
            if (a == null || double.IsNaN(a.Price) || double.IsInfinity(a.Price) || a.Price <= 0)
                return h.TotalInvested;
            return h.Quantity * a.Price;
        }

        //Soma do valor investido nas posições abertas
        public double GetInvestedOpen()
        {
            Sync();
            double total = 0;
            for (int i = 0; i < Holdings.Count; i++)
                total += Holdings[i].TotalInvested;
            return total;
        }

        public void AddTrade(MarketTrade t)
        {
            Trades.Add(t);
            if (Trades.Count > MaxTrades)
                Trades.RemoveRange(0, Trades.Count - MaxTrades);
        }

        //Mercado do jogo atual (fica no Player, então vai junto no save)
        public static Market Current
        {
            get
            {
                if (Player.Current.Market == null)
                    Player.Current.Market = Create();
                Player.Current.Market.Sync();
                return Player.Current.Market;
            }
        }

        public static Market Create()
        {
            var market = new Market();
            market.Sync();
            return market;
        }

        //Acerta o mercado com a lista de ativos atual (saves antigos, ativos novos ou removidos)
        public void Sync()
        {
            if (Assets == null) Assets = new List<AssetState>();
            if (CrashDaysLeft == null) CrashDaysLeft = new List<int>();
            if (Favorites == null) Favorites = new List<string>();
            if (Holdings == null) Holdings = new List<Holding>();
            if (Trades == null) Trades = new List<MarketTrade>();
            for (int i = Holdings.Count - 1; i >= 0; i--)
                if (Holdings[i] == null)
                    Holdings.RemoveAt(i);
            for (int i = Trades.Count - 1; i >= 0; i--)
                if (Trades[i] == null)
                    Trades.RemoveAt(i);
            if (Trades.Count > MaxTrades)
                Trades.RemoveRange(0, Trades.Count - MaxTrades);
            //Totais da carteira: saves antigos (StatsSeeded false) começam a partir dos Trades, uma vez só.
            //Jogo novo tem Trades vazio, então o seed não soma nada.
            if (!StatsSeeded)
            {
                for (int i = 0; i < Trades.Count; i++)
                {
                    if (Trades[i].Kind == TradeKind.Sell) RealizedProfit += Trades[i].Profit - Trades[i].Tax;
                    else if (Trades[i].Kind == TradeKind.Buy) TotalApplied += Trades[i].Total;
                }
                StatsSeeded = true;
            }
            //P7: ativo removido com posição
            for (int i = Favorites.Count - 1; i >= 0; i--)
                if (AssetDef.Find(Favorites[i]) == null)
                    Favorites.RemoveAt(i);

            for (int i = Assets.Count - 1; i >= 0; i--)
                if (Assets[i] == null || AssetDef.Find(Assets[i].Ticker) == null)
                    Assets.RemoveAt(i);

            foreach (var def in AssetDef.All)
            {
                var state = Get(def.Ticker);
                if (state == null)
                {
                    state = new AssetState();
                    state.Ticker = def.Ticker;
                    state.Price = def.StartPrice;
                    state.PrevPrice = def.StartPrice;
                    state.TrendLine = def.StartPrice;
                    Assets.Add(state);
                }
                if (state.Daily == null) state.Daily = new List<double>();
                if (state.Weekly == null) state.Weekly = new List<double>();
                if (state.Daily.Count > PriceModel.MaxDaily)
                    state.Daily.RemoveRange(0, state.Daily.Count - PriceModel.MaxDaily);
                if (state.Weekly.Count > PriceModel.MaxWeekly)
                    state.Weekly.RemoveRange(0, state.Weekly.Count - PriceModel.MaxWeekly);
                if (!(state.Price > 0) || double.IsInfinity(state.Price)) state.Price = def.StartPrice;
                if (!(state.TrendLine > 0) || double.IsInfinity(state.TrendLine)) state.TrendLine = state.Price;
            }
        }

        public bool IsFavorite(string ticker)
        {
            return Favorites.Contains(ticker);
        }

        public void ToggleFavorite(string ticker)
        {
            if (!Favorites.Remove(ticker) && AssetDef.Find(ticker) != null)
                Favorites.Add(ticker);
        }

        public AssetState Get(string ticker)
        {
            for (int i = 0; i < Assets.Count; i++)
                if (Assets[i].Ticker == ticker) return Assets[i];
            return null;
        }

        public void NewDay(int day)
        {
            if (day <= LastDay) return;
            LastDay = day;

            //Sorteio do mercado do dia, igual para todos os ativos
            double m = RandomHelpers.Gaussian();
            foreach (var state in Assets)
            {
                var def = AssetDef.Find(state.Ticker);
                if (def != null)
                    PriceModel.Step(def, state, day, m);
            }
        }

        //Teste de balanço: simula um mercado isolado e grava os preços em CSV (não mexe no jogo)
        public static void SimulateCsv(int days, string path)
        {
            var market = Create();
            var sb = new StringBuilder();
            sb.Append("day");
            foreach (var def in AssetDef.All)
                sb.Append(";").Append(def.Ticker);
            sb.AppendLine();

            for (int day = 1; day <= days; day++)
            {
                market.NewDay(day);
                sb.Append(day.ToString(CultureInfo.InvariantCulture));
                foreach (var def in AssetDef.All)
                    sb.Append(";").Append(market.Get(def.Ticker).Price.ToString("G6",CultureInfo.InvariantCulture));
                sb.AppendLine();
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }
    }
}
