using System;

namespace TubeStar
{
    //Imposto semanal sobre o lucro do canal (faixas marginais)
    public static class TaxCalculator
    {
        public const double ExemptLimit = 500;
        public const double MiddleLimit = 5000;

        //Lucro do canal na semana: ganho do canal menos vídeos e promoções (salário, custo de vida e empréstimo não entram)
        public static double WeeklyProfit(PeriodStats week)
        {
            return week.ChannelEarned - week.VideoCost - week.VideoPromotionCost;
        }

        //Até $500 isento; $500 a $5.000 a 15%; acima de $5.000 a 25% (o Advogado Fiscal tira 5 pontos de cada faixa)
        public static double Tax(double profit, bool hasTaxLawyer)
        {
            if (profit <= ExemptLimit)
                return 0;

            double discount = hasTaxLawyer ? StoreItems.Current.TaxLawyer.RateReduction : 0;
            double middleRate = 0.15 - discount;
            double topRate = 0.25 - discount;

            double middle = Math.Min(profit, MiddleLimit) - ExemptLimit;
            double top = Math.Max(0, profit - MiddleLimit);
            return Math.Round(middle * middleRate + top * topRate, 2);
        }
    }
}
