using System;

namespace TubeStar
{
    /// <summary>
    /// "Promover vídeo" (item 5d): campanhas pagas que entregam views ao longo de alguns dias.
    /// Views promovidas não pagam anúncio, mas podem trazer likes, inscritos e compartilhamentos
    /// </summary>
    public static class Promotion
    {
        public const int Days = 3;

        //Nenhuma, Pequena, Média, Grande
        public static readonly int[] Costs = { 0, 500, 1000, 2000 };
        public static readonly int[] Views = { 0, 3000, 8000, 18000 };

        public static string GetName(int level)
        {
            switch (level)
            {
                case 1: return EnglishStrings.PromoteSmall.Translate();
                case 2: return EnglishStrings.PromoteMedium.Translate();
                case 3: return EnglishStrings.PromoteLarge.Translate();
            }
            return EnglishStrings.None.Translate();
        }

        //Começa a campanha: total sorteado em ±10% do estimado, entregue em Days dias
        public static void Start(Video video, int level)
        {
            if (level <= 0 || level >= Costs.Length)
                return;
            int baseViews = Views[level];
            int variation = baseViews / 10;
            video.PromotionViewsLeft = baseViews - variation + RandomHelpers.RandomInt(variation * 2 + 1);
            video.PromotionDaysLeft = Days;
            video.OnceOffCost += Costs[level];
        }

        //Views promovidas de hoje (divide o que falta pelos dias restantes)
        public static int TakeTodayViews(Video video)
        {
            if (video.PromotionDaysLeft <= 0 || video.PromotionViewsLeft <= 0)
            {
                video.PromotionDaysLeft = 0;
                video.PromotionViewsLeft = 0;
                return 0;
            }
            int today = video.PromotionViewsLeft / video.PromotionDaysLeft;
            video.PromotionViewsLeft -= today;
            video.PromotionDaysLeft--;
            return today;
        }
    }
}
