using System;

namespace TubeStar
{
    /// <summary>
    /// Único lugar da conta do RPM (dólares por mil views)
    /// </summary>
    public static class RpmCalculator
    {
        //RPM base de cada nicho
        public static double BaseRpm(VideoCategory niche)
        {
            switch (niche)
            {
                case VideoCategory.Finance: return 55;
                case VideoCategory.Technology: return 40;
                case VideoCategory.Education: return 35;
                case VideoCategory.Travel: return 30;
                case VideoCategory.Fitness: return 30;
                case VideoCategory.Cooking: return 25;
                case VideoCategory.Beauty: return 25;
                case VideoCategory.Sports: return 25;
                case VideoCategory.Mystery: return 20;
                case VideoCategory.Games: return 20;
                case VideoCategory.Vlog: return 20;
                case VideoCategory.Comedy: return 15;
                case VideoCategory.Music: return 15;
                case VideoCategory.Reacts: return 15;
                case VideoCategory.Kids: return 10;
            }
            return 20; //Sem nicho = Games
        }

        //Semana do calendário do jogo (ano de 112 dias, semana de 7), mesma conta do CalendarDialog
        public static int CurrentWeek()
        {
            int day = Math.Max(0, Player.Current.Iterations);
            int dayOfYear = day % 112 + 1;
            return (dayOfYear - 1) / 7 + 1;
        }

        //Época do ano: Alta (13-16), Fraca (1-2) ou Normal
        public static string SeasonLabel(int week)
        {
            if (week >= 13 && week <= 16) return "High";
            if (week <= 2) return "Low";
            return "Normal";
        }

        private static double SeasonFactor(int week)
        {
            switch (week)
            {
                case 1:
                case 2: return 0.75;
                case 13: return 1.10;
                case 14: return 1.15;
                case 15: return 1.25;
                case 16: return 1.30;
            }
            return 1.0;
        }

        //Variação semanal de ±5%, fixa para (semana, nicho): não precisa salvar
        private static double WeeklyVariation(int week, VideoCategory niche)
        {
            var random = new Random(week * 100 + (int)niche);
            return 0.95 + random.NextDouble() * 0.10;
        }

        //(+5 = RPM base antigo, para o boost continuar igual)
                //Quanto as views de busca e qualidade sobem em nichos de RPM baixo, para a renda não despencar
        public static double ViewBoost(VideoCategory niche)
        {
            return Math.Max(1.0, Math.Pow(25.0 / (BaseRpm(niche) + 5), 0.35));
        }

        public static double Rpm(Channel channel, Video video)
        {
            return Rpm(channel, video, CurrentWeek());
        }

        public static double Rpm(Channel channel, Video video, int week)
        {
            return Compute(channel, video.Category, video.Quality.Value, video.Attributes.Contains(VideoAttributes.ProductPlacement), week);
        }

        //RPM de hoje do canal (nicho do canal, vídeo de nota média, sem merchandising)
        public static double ChannelRpm(Channel channel)
        {
            VideoCategory niche = channel.Niche.HasValue ? channel.Niche.Value : VideoCategory.Games;
            return Compute(channel, niche, 50, false, CurrentWeek());
        }

        private static double Compute(Channel channel, VideoCategory niche, int qualityValue, bool productPlacement, int week)
        {
            double rpm = BaseRpm(niche);
            rpm *= SeasonFactor(week);
            rpm *= WeeklyVariation(week, niche);

            //Estratégia + qualidade do vídeo, somadas e limitadas a ±20%
            double strategy = 0;
            switch (channel.Advertising)
            {
                case AdvertisingStrategy.Low: strategy = -0.15; break;
                case AdvertisingStrategy.High: strategy = 0.10; break;
                case AdvertisingStrategy.Aggressive: strategy = 0.20; break;
            }
            double grade = qualityValue / 10.0;
            double quality = Math.Max(-0.10, Math.Min(0.10, (grade - 5.0) / 5.0 * 0.10));
            rpm *= 1.0 + Math.Max(-0.20, Math.Min(0.20, strategy + quality));

            //Inscritos: sobe com a raiz, teto de +15%
            double subs = Math.Max(0, channel.Subscribers);
            rpm *= 1.0 + Math.Min(0.15, Math.Sqrt(subs / 1000000.0) * 0.15);

            if (productPlacement)
                rpm *= 1.15;

            if (Player.Current.ChallengeMode)
                rpm /= 2;

            return rpm;
        }
    }
}
