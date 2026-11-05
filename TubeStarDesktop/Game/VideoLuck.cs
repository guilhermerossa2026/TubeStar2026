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
        private static readonly double[] ReachByGrade = { 0.5, 0.5, 0.5, 3, 5, 8, 11, 15, 20, 25, 30 };

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
            if (grade < 3) { flop = 0.60; good = 0.02; viral = 0.002; }
            else if (grade < 5) { flop = 0.40; good = 0.045; viral = 0.005; }
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

        //Momento do canal: ao postar (nota = Quality/10 e sorte do vídeo), limitado de 0,7 a 1,4
        public const double MomentumMin = 0.7;
        public const double MomentumMax = 1.4;

        public static double ClampMomentum(double m)
        {
            return Math.Max(MomentumMin, Math.Min(MomentumMax, m));
        }

        public static double MomentumOnPost(double momentum, double grade, int luckTier)
        {
            if (grade >= 9) momentum += 0.08;
            else if (grade >= 7) momentum += 0.05;
            else if (grade <= 4) momentum -= 0.06;
            if (luckTier == TierViral) momentum += 0.10;
            return ClampMomentum(momentum);
        }

        //Todo dia: a partir do 4º dia sem postar cai 0,01; depois volta 2% do caminho até 1
        public static double MomentumDaily(double momentum, int daysIdle)
        {
            if (daysIdle >= 4) momentum -= 0.01;
            momentum += (1 - momentum) * 0.02;
            return ClampMomentum(momentum);
        }

        //Ao postar: aplica o momento e zera a perda do canal parado (chamar depois de SubscriberView)
        public static void OnPost(Video video, Channel channel)
        {
            double grade = video.GenerateQuality() / 10.0;
            channel.Momentum = MomentumOnPost(channel.Momentum, grade, video.LuckTier);
            channel.IdleUnsubsTaken = 0;
            channel.IdleUnsubsFraction = 0;
            video.UnsubsPlanned = PlanUnsubs(grade, channel.Subscribers, RandomHelpers.RandomDouble(), RandomHelpers.RandomDouble());
            video.UnsubsTaken = 0;
        }

        //Desinscrição suave de vídeo fraco: inscritos x uniforme(0, taxa da nota), teto de 0,5% dos inscritos
        //u2 decide o arredondamento: floor(x) e mais 1 com probabilidade igual à parte fracionária
        public static int PlanUnsubs(double grade, int subscribers, double u, double u2)
        {
            double rate;
            if (grade >= 5) rate = 0;
            else if (grade >= 4) rate = 0.002;
            else if (grade >= 3) rate = 0.004;
            else rate = 0.006;
            double planned = subscribers * u * rate;
            planned = Math.Min(planned, subscribers * 0.005);
            int whole = (int)Math.Floor(planned);
            if (u2 < planned - whole) whole++;
            return whole;
        }

        //Quanto tirar hoje: 1/3 do que falta (pelo menos 1 se faltar)
        public static int UnsubsStep(int planned, int taken)
        {
            int remaining = planned - taken;
            if (remaining <= 0) return 0;
            return Math.Min(remaining, Math.Max(1, remaining / 3));
        }

        //Canal parado: depois de 14 dias sem postar perde 0,05% por dia, até 5% acumulado
        public const int IdleUnsubsDays = 14;

        //A fração que sobra fica guardada no canal (fraction) para não se perder
        public static int IdleUnsubs(int daysIdle, int subscribers, int idleTaken, ref double fraction)
        {
            if (daysIdle <= IdleUnsubsDays) return 0;
            int cap = (int)((subscribers + idleTaken) * 0.05);
            fraction += subscribers * 0.0005;
            int loss = (int)fraction;
            fraction -= loss;
            return Math.Max(0, Math.Min(loss, cap - idleTaken));
        }

        //Empurrão de busca nos primeiros 3 dias: canais pequenos ganham um mínimo de views de busca
        public const int SearchFloorDays = 3;

        public static double SearchFloor(int subscribers)
        {
            if (subscribers >= 2000) return 0;
            return 80.0 * (1.0 - subscribers / 2000.0);
        }

        //Fator da nota no empurrão: nota >= 7 = 1, cai por faixas até 0 na nota 2
        public static double SearchFloorGradeFactor(double grade)
        {
            if (grade >= 7) return 1;
            if (grade >= 5) return 0.6 + 0.4 * (grade - 5) / 2.0;
            if (grade >= 3) return 0.25 + 0.35 * (grade - 3) / 2.0;
            if (grade > 2) return 0.25 * (grade - 2);
            return 0;
        }
    }
}
