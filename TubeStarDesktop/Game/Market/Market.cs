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

        public Market()
        {
            Assets = new List<AssetState>();
            LastDay = -1;
            CrashDaysLeft = new List<int>();
            Favorites = new List<string>();
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
                if (state.Price <= 0) state.Price = def.StartPrice;
                if (state.TrendLine <= 0) state.TrendLine = state.Price;
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
