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

        //Centro da entrega (60% a 130% do estimado), puxado pela nota do vídeo (0-100). Sem nota: 0,95
        public static double Center(Video video)
        {
            if (video == null || !video.Quality.HasValue)
                return 0.95;
            double grade = Math.Max(0, Math.Min(100, video.Quality.Value));
            return 0.6 + 0.7 * (grade / 100.0);
        }

        //Views esperadas da campanha (centro) para mostrar como "cerca de"
        public static int EstimateViews(Video video, int level)
        {
            if (level <= 0 || level >= Views.Length)
                return 0;
            return (int)Math.Round(Views[level] * Center(video));
        }

        //Começa a campanha: sorteia ±15% em torno do centro (limitado a 60%-130%), entregue em Days dias
        public static void Start(Video video, int level)
        {
            if (level <= 0 || level >= Costs.Length)
                return;
            double center = Center(video);
            double factor = center * (0.85 + RandomHelpers.RandomInt(301) / 1000.0);
            factor = Math.Max(0.6, Math.Min(1.3, factor));
            video.PromotionViewsLeft = (int)Math.Round(Views[level] * factor);
            video.PromotionDaysLeft = Days;
            video.OnceOffCost += Costs[level];
            video.PromotionCost += Costs[level];
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
