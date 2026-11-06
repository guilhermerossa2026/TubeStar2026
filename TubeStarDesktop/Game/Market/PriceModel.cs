using System;

namespace TubeStar
{
    /// <summary>
    /// Regras de como o preço de um ativo muda a cada dia
    /// </summary>
    public static class PriceModel
    {
        public const int MaxDaily = 112;
        public const int MaxWeekly = 260;

        //Segunda a sexta (dias 0 a 4 da semana)
        public static bool IsTradingDay(int day)
        {
            return day % 7 < 5;
        }

        //m = sorteio do mercado do dia (igual para todos os ativos)
        public static void Step(AssetDef def, AssetState state, int day, double m)
        {
            bool crypto = def.Class == AssetClass.Crypto;
            bool fixedIncome = def.Class == AssetClass.FixedIncome;

            state.PrevPrice = state.Price;

            if (fixedIncome)
            {
                state.Price = Round(state.Price * Math.Pow(1 + def.AnnualRate, 1.0 / 112));
            }
            else if (crypto || IsTradingDay(day))
            {
                double periods = crypto ? 112.0 : 80.0;
                double drift = def.AnnualTrend / periods - def.DailyVol * def.DailyVol / 2; //log-normal: a média segue a tendência
                double z = 0.5 * m + 0.866 * RandomHelpers.Gaussian();
                double shock = def.DailyVol * z;
                if (RandomHelpers.RandomDouble() < 0.02)
                    shock *= 2.5;

                state.TrendLine *= 1 + def.AnnualTrend / periods;
                double r = state.Price / state.TrendLine;
                double pull = 0;
                if (r > 3) pull = -0.01 * Math.Log(r / 3);
                else if (r < 0.5) pull = 0.01 * Math.Log(0.5 / r);

                double newsHook = 0; //gancho para notícias (M5)

                double logRet = drift + shock + pull + newsHook;
                double up = crypto ? Math.Log(1.30) : Math.Log(1.12);
                double down = crypto ? Math.Log(0.70) : Math.Log(0.88);
                logRet = Math.Max(down, Math.Min(up, logRet));

                double price = Math.Max(state.Price * Math.Exp(logRet), def.StartPrice * def.FloorFraction);
                state.Price = Round(price);
            }
            //Ação ou fundo fora de dia útil: o preço fica igual

            state.Daily.Add(state.Price);
            if (state.Daily.Count > MaxDaily)
                state.Daily.RemoveRange(0, state.Daily.Count - MaxDaily);

            if (day % 7 == 0)
            {
                state.Weekly.Add(state.Price);
                if (state.Weekly.Count > MaxWeekly)
                    state.Weekly.RemoveRange(0, state.Weekly.Count - MaxWeekly);
            }
        }

        //Arredonda para 6 algarismos significativos
        public static double Round(double value)
        {
            if (value <= 0 || double.IsNaN(value) || double.IsInfinity(value)) return value;
            int digits = 5 - (int)Math.Floor(Math.Log10(value));
            if (digits < 0) digits = 0;
            if (digits > 15) digits = 15;
            return Math.Round(value, digits);
        }
    }
}
