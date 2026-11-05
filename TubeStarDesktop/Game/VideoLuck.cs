using System;

namespace TubeStar
{
    //Alcance por nota, sorte do vídeo e empurrão inicial de busca
    public static class VideoLuck
    {
        public const int TierNormal = 0;
        public const int TierFlop = 1;
        public const int TierGood = 2;
        public const int TierViral = 3;

        //Nota (0 a 10) -> % dos inscritos que assistem no dia da postagem (interpolado)
        private static readonly double[] ReachByGrade = { 1, 1, 1, 3, 5, 8, 11, 15, 20, 25, 30 };

        //Devolve a fração (0,01 a 0,30)
        public static double ReachPercent(double grade)
        {
            grade = Math.Max(0, Math.Min(10, grade));
            int low = (int)Math.Floor(grade);
            if (low >= 10) return ReachByGrade[10] / 100.0;
            double t = grade - low;
            return (ReachByGrade[low] + (ReachByGrade[low + 1] - ReachByGrade[low]) * t) / 100.0;
        }

        //Sorteia a sorte do vídeo e grava em Luck e LuckTier
        public static void DrawLuck(Video video, Channel channel)
        {
            double grade = video.GenerateQuality() / 10.0;
            double momentum = Math.Max(0.01, channel.Momentum);
            int tier;
            video.Luck = Draw(grade, momentum, channel.Subscribers, RandomHelpers.RandomDouble(), RandomHelpers.RandomDouble(), RandomHelpers.RandomDouble(), out tier);
            video.LuckTier = tier;
        }

        //Parte pura do sorteio (r = faixa, u = valor dentro da faixa)
        public static double Draw(double grade, double momentum, int subscribers, double r, double u, double u2, out int tier)
        {
            double flop, good, viral;
            if (grade < 5) { flop = 0.30; good = 0.045; viral = 0.005; }
            else if (grade < 7) { flop = 0.20; good = 0.08; viral = 0.02; }
            else if (grade < 9) { flop = 0.12; good = 0.11; viral = 0.03; }
            else { flop = 0.08; good = 0.13; viral = 0.04; }

            //Momento mexe na chance de viral; as outras faixas se ajustam para somar 100%
            double newViral = Math.Min(0.5, viral * momentum);
            double scale = (1 - viral) > 0 ? (1 - newViral) / (1 - viral) : 1;
            flop *= scale;
            good *= scale;
            viral = newViral;

            if (r < flop)
            {
                tier = TierFlop;
                return 0.5;
            }
            if (r < flop + good)
            {
                tier = TierGood;
                return 2.0;
            }
            if (r < flop + good + viral)
            {
                tier = TierViral;
                double mult = 5 + 45 * u2 * u2 * u2;
                return Math.Min(mult, subscribers > 1000000 ? 20.0 : 50.0);
            }
            tier = TierNormal;
            return 0.8 + 0.5 * u;
        }

        //Empurrão de busca nos primeiros 3 dias: canais pequenos ganham um mínimo de views de busca
        public const int SearchFloorDays = 3;

        public static double SearchFloor(int subscribers)
        {
            if (subscribers >= 2000) return 0;
            return 80.0 * (1.0 - subscribers / 2000.0);
        }
    }
}
