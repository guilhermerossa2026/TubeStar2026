using System.Collections.Generic;

namespace TubeStar
{
    /// <summary>
    /// Definição fixa de um ativo (não vai pro save)
    /// </summary>
    public class AssetDef
    {
        public string Ticker { get; private set; }
        public string Name { get; private set; }
        public string Sector { get; private set; }
        public AssetClass Class { get; private set; }
        //Preço inicial em dólar (renda fixa: índice 1.0)
        public double StartPrice { get; private set; }
        //Tendência anual (0.10 = +10% ao ano do jogo)
        public double AnnualTrend { get; private set; }
        //Volatilidade diária (0.02 = 2%)
        public double DailyVol { get; private set; }
        //Juros anuais (só renda fixa)
        public double AnnualRate { get; private set; }
        //Ativo arriscado (memes)
        public bool Risky { get; private set; }
        //Piso do preço, como fração do preço inicial
        public double FloorFraction { get; private set; }
        //Gancho para dividendos (M4)
        public double DividendYield { get; private set; }

        public static readonly List<AssetDef> All = BuildAll();

        public static AssetDef Find(string ticker)
        {
            for (int i = 0; i < All.Count; i++)
                if (All[i].Ticker == ticker) return All[i];
            return null;
        }

        private static AssetDef Make(string ticker, string name, string sector, AssetClass cls, double price, double trend, double vol)
        {
            return new AssetDef
            {
                Ticker = ticker,
                Name = name,
                Sector = sector,
                Class = cls,
                StartPrice = price,
                AnnualTrend = trend,
                DailyVol = vol,
                FloorFraction = 0.10
            };
        }

        private static AssetDef Fixed(string ticker, string name, double rate)
        {
            return new AssetDef
            {
                Ticker = ticker,
                Name = name,
                Sector = "Fixed income",
                Class = AssetClass.FixedIncome,
                StartPrice = 1.0,
                AnnualRate = rate,
                FloorFraction = 0.10
            };
        }

        private static List<AssetDef> BuildAll()
        {
            var l = new List<AssetDef>();
            //Ações Brasil
            l.Add(Make("PETR4", "Petrobras", "Oil", AssetClass.StockBR, 6, 0.06, 0.020));
            l.Add(Make("VALE3", "Vale", "Mining", AssetClass.StockBR, 11, 0.05, 0.020));
            l.Add(Make("ITUB4", "Itaú", "Banks", AssetClass.StockBR, 7, 0.10, 0.015));
            l.Add(Make("BBDC4", "Bradesco", "Banks", AssetClass.StockBR, 3, 0.04, 0.017));
            l.Add(Make("BBAS3", "Banco do Brasil", "Banks", AssetClass.StockBR, 4, 0.07, 0.018));
            l.Add(Make("WEGE3", "WEG", "Industry", AssetClass.StockBR, 8, 0.12, 0.020));
            l.Add(Make("ABEV3", "Ambev", "Beverages", AssetClass.StockBR, 2.4, 0.04, 0.015));
            l.Add(Make("ELET3", "Eletrobras", "Energy", AssetClass.StockBR, 8, 0.08, 0.020));
            l.Add(Make("RENT3", "Localiza", "Car rental", AssetClass.StockBR, 9, 0.09, 0.025));
            l.Add(Make("MGLU3", "Magazine Luiza", "Retail", AssetClass.StockBR, 1.8, 0.03, 0.035));
            //Ações EUA
            l.Add(Make("AAPL", "Apple", "Tech", AssetClass.StockUS, 230, 0.10, 0.015));
            l.Add(Make("MSFT", "Microsoft", "Tech", AssetClass.StockUS, 480, 0.12, 0.014));
            l.Add(Make("NVDA", "Nvidia", "Chips", AssetClass.StockUS, 180, 0.18, 0.030));
            l.Add(Make("GOOGL", "Alphabet (Google)", "Tech", AssetClass.StockUS, 250, 0.11, 0.016));
            l.Add(Make("AMZN", "Amazon", "Tech", AssetClass.StockUS, 220, 0.11, 0.017));
            l.Add(Make("TSLA", "Tesla", "Cars", AssetClass.StockUS, 400, 0.10, 0.040));
            l.Add(Make("KO", "Coca-Cola", "Beverages", AssetClass.StockUS, 70, 0.05, 0.008));
            l.Add(Make("DIS", "Disney", "Media", AssetClass.StockUS, 115, 0.06, 0.015));
            //Cripto
            l.Add(Make("BTC", "Bitcoin", "Crypto", AssetClass.Crypto, 110000, 0.20, 0.030));
            l.Add(Make("ETH", "Ethereum", "Crypto", AssetClass.Crypto, 4000, 0.18, 0.040));
            l.Add(Make("SOL", "Solana", "Crypto", AssetClass.Crypto, 200, 0.15, 0.050));
            l.Add(Make("XRP", "XRP", "Crypto", AssetClass.Crypto, 2.8, 0.08, 0.050));
            var doge = Make("DOGE", "Dogecoin", "Meme crypto", AssetClass.Crypto, 0.25, 0.0, 0.070);
            doge.Risky = true;
            doge.FloorFraction = 0.01;
            l.Add(doge);
            var pepe = Make("PEPE", "Pepe", "Meme crypto", AssetClass.Crypto, 0.00001, -0.35, 0.100);
            pepe.Risky = true;
            pepe.FloorFraction = 0.01;
            l.Add(pepe);
            //Fundos
            l.Add(Make("SPY", "S&P 500 Fund", "Index fund", AssetClass.Fund, 660, 0.09, 0.009));
            l.Add(Make("BOVA11", "Brazil Index Fund", "Index fund", AssetClass.Fund, 27, 0.08, 0.012));
            l.Add(Make("HGLG11", "Warehouse Real Estate Fund", "Real estate fund", AssetClass.Fund, 30, 0.03, 0.006));
            //Renda fixa
            l.Add(Fixed("SELIC", "Treasury Selic", 0.14));
            l.Add(Fixed("CDB-G", "CDB Big Bank", 0.14));
            l.Add(Fixed("CDB-M", "CDB Mid Bank", 0.17));
            return l;
        }
    }
}
