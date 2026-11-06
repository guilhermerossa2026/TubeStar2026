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
        //Setor (texto traduzido via Translate)
        public EnglishStrings Sector { get; private set; }
        //Nome traduzido (só os ativos com nome descritivo; os outros usam o Name)
        public EnglishStrings? NameKey { get; private set; }
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
        //Cor da bolinha do ativo (hex aproximado da marca)
        public string Color { get; private set; }

        private static readonly string[,] Colors =
        {
            { "PETR4", "#7FB08A" }, { "VALE3", "#D9A441" }, { "ITUB4", "#D98A5C" }, { "BBDC4", "#C97A86" },
            { "BBAS3", "#D9C25C" }, { "WEGE3", "#6FA3B8" }, { "ABEV3", "#B8A06F" }, { "ELET3", "#7FA3D9" },
            { "RENT3", "#8FB86F" }, { "MGLU3", "#6F9BD9" }, { "AAPL", "#B8B8B8" }, { "MSFT", "#6FA3D9" },
            { "NVDA", "#8FBF6F" }, { "GOOGL", "#D9877A" }, { "AMZN", "#D9A441" }, { "TSLA", "#C97A7A" },
            { "KO", "#C97A7A" }, { "DIS", "#7A8FD9" }, { "BTC", "#D9A441" }, { "ETH", "#8A96C9" },
            { "SOL", "#9B88C9" }, { "XRP", "#B8B8B8" }, { "DOGE", "#D9B85C" }, { "PEPE", "#7FB08A" },
            { "SPY", "#6FA3D9" }, { "BOVA11", "#7FB08A" }, { "HGLG11", "#B8A06F" },
            { "SELIC", "#7FB08A" }, { "CDB-G", "#6FA3D9" }, { "CDB-M", "#D9A441" }
        };

        public static readonly List<AssetDef> All = BuildAll();

        //Nome para mostrar ao jogador
        public string DisplayName()
        {
            if (NameKey.HasValue) return NameKey.Value.Translate();
            return Name;
        }

        //Etiqueta do tipo: Ação, Cripto, Fundo ou Renda fixa
        public string TypeLabel()
        {
            switch (Class)
            {
                case AssetClass.Crypto: return EnglishStrings.MarketTypeCrypto.Translate();
                case AssetClass.Fund: return EnglishStrings.MarketTypeFund.Translate();
                case AssetClass.FixedIncome: return EnglishStrings.MarketTypeFixed.Translate();
                default: return EnglishStrings.MarketTypeStock.Translate();
            }
        }

        public static AssetDef Find(string ticker)
        {
            for (int i = 0; i < All.Count; i++)
                if (All[i].Ticker == ticker) return All[i];
            return null;
        }

        private static AssetDef Make(string ticker, string name, EnglishStrings sector, AssetClass cls, double price, double trend, double vol)
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
                Sector = EnglishStrings.MarketSectorFixedIncome,
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
            l.Add(Make("PETR4", "Petrobras", EnglishStrings.MarketSectorOil, AssetClass.StockBR, 6, 0.06, 0.020));
            l.Add(Make("VALE3", "Vale", EnglishStrings.MarketSectorMining, AssetClass.StockBR, 11, 0.05, 0.020));
            l.Add(Make("ITUB4", "Itaú", EnglishStrings.MarketSectorBanks, AssetClass.StockBR, 7, 0.10, 0.015));
            l.Add(Make("BBDC4", "Bradesco", EnglishStrings.MarketSectorBanks, AssetClass.StockBR, 3, 0.04, 0.017));
            l.Add(Make("BBAS3", "Banco do Brasil", EnglishStrings.MarketSectorBanks, AssetClass.StockBR, 4, 0.07, 0.018));
            l.Add(Make("WEGE3", "WEG", EnglishStrings.MarketSectorIndustry, AssetClass.StockBR, 8, 0.12, 0.020));
            l.Add(Make("ABEV3", "Ambev", EnglishStrings.MarketSectorBeverages, AssetClass.StockBR, 2.4, 0.04, 0.015));
            l.Add(Make("ELET3", "Eletrobras", EnglishStrings.MarketSectorEnergy, AssetClass.StockBR, 8, 0.08, 0.020));
            l.Add(Make("RENT3", "Localiza", EnglishStrings.MarketSectorCarRental, AssetClass.StockBR, 9, 0.09, 0.025));
            l.Add(Make("MGLU3", "Magazine Luiza", EnglishStrings.MarketSectorRetail, AssetClass.StockBR, 1.8, 0.03, 0.035));
            //Ações EUA
            l.Add(Make("AAPL", "Apple", EnglishStrings.MarketSectorTech, AssetClass.StockUS, 230, 0.10, 0.015));
            l.Add(Make("MSFT", "Microsoft", EnglishStrings.MarketSectorTech, AssetClass.StockUS, 480, 0.12, 0.014));
            l.Add(Make("NVDA", "Nvidia", EnglishStrings.MarketSectorChips, AssetClass.StockUS, 180, 0.18, 0.030));
            l.Add(Make("GOOGL", "Alphabet (Google)", EnglishStrings.MarketSectorTech, AssetClass.StockUS, 250, 0.11, 0.016));
            l.Add(Make("AMZN", "Amazon", EnglishStrings.MarketSectorTech, AssetClass.StockUS, 220, 0.11, 0.017));
            l.Add(Make("TSLA", "Tesla", EnglishStrings.MarketSectorCars, AssetClass.StockUS, 400, 0.10, 0.040));
            l.Add(Make("KO", "Coca-Cola", EnglishStrings.MarketSectorBeverages, AssetClass.StockUS, 70, 0.05, 0.008));
            l.Add(Make("DIS", "Disney", EnglishStrings.MarketSectorMedia, AssetClass.StockUS, 115, 0.06, 0.015));
            //Cripto
            l.Add(Make("BTC", "Bitcoin", EnglishStrings.MarketSectorCrypto, AssetClass.Crypto, 110000, 0.20, 0.030));
            l.Add(Make("ETH", "Ethereum", EnglishStrings.MarketSectorCrypto, AssetClass.Crypto, 4000, 0.18, 0.040));
            l.Add(Make("SOL", "Solana", EnglishStrings.MarketSectorCrypto, AssetClass.Crypto, 200, 0.15, 0.050));
            l.Add(Make("XRP", "XRP", EnglishStrings.MarketSectorCrypto, AssetClass.Crypto, 2.8, 0.08, 0.050));
            var doge = Make("DOGE", "Dogecoin", EnglishStrings.MarketSectorMemeCrypto, AssetClass.Crypto, 0.25, 0.0, 0.070);
            doge.Risky = true;
            doge.FloorFraction = 0.01;
            l.Add(doge);
            var pepe = Make("PEPE", "Pepe", EnglishStrings.MarketSectorMemeCrypto, AssetClass.Crypto, 0.00001, -0.35, 0.100);
            pepe.Risky = true;
            pepe.FloorFraction = 0.01;
            l.Add(pepe);
            //Fundos
            var spy = Make("SPY", "S&P 500 Fund", EnglishStrings.MarketSectorIndexFund, AssetClass.Fund, 660, 0.09, 0.009);
            spy.NameKey = EnglishStrings.MarketNameSp500;
            l.Add(spy);
            var bova = Make("BOVA11", "Brazil Index Fund", EnglishStrings.MarketSectorIndexFund, AssetClass.Fund, 27, 0.08, 0.012);
            bova.NameKey = EnglishStrings.MarketNameBova;
            l.Add(bova);
            var hglg = Make("HGLG11", "Warehouse Real Estate Fund", EnglishStrings.MarketSectorRealEstateFund, AssetClass.Fund, 30, 0.03, 0.006);
            hglg.NameKey = EnglishStrings.MarketNameHglg;
            l.Add(hglg);
            //Renda fixa
            var selic = Fixed("SELIC", "Treasury Selic", 0.14);
            selic.NameKey = EnglishStrings.MarketNameSelic;
            l.Add(selic);
            var cdbBig = Fixed("CDB-G", "CDB Big Bank", 0.14);
            cdbBig.NameKey = EnglishStrings.MarketNameCdbBig;
            l.Add(cdbBig);
            var cdbMid = Fixed("CDB-M", "CDB Mid Bank", 0.17);
            cdbMid.NameKey = EnglishStrings.MarketNameCdbMid;
            l.Add(cdbMid);

            foreach (var def in l)
            {
                def.Color = "#B8B8B8";
                for (int i = 0; i < Colors.GetLength(0); i++)
                    if (Colors[i, 0] == def.Ticker) def.Color = Colors[i, 1];
            }
            return l;
        }
    }
}
