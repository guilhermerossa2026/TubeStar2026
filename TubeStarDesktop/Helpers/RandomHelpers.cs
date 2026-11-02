using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubeStar
{
    public static class RandomHelpers
    {
        private const int _videoCategoryLength = 18;
        private static Lazy<Random> _random;

        static RandomHelpers()
        {
            _random = new Lazy<Random>();
        }

        public static VideoCategory RandomVideoCategory
        {
            get { return (VideoCategory)_random.Value.Next(1, _videoCategoryLength + 1); }
        }

        public static int RandomInt(int max)
        {
            return _random.Value.Next(max);
        }

        public static int RandomInt(int min, int max)
        {
            return _random.Value.Next(min, max);
        }

        public static bool Chance(int percentage = 100)
        {
            percentage = Math.Min(percentage, 100);
            percentage = Math.Max(percentage, 0);

            return _random.Value.Next(100) <= percentage;
        }

        public static bool RandomBool()
        {
            return _random.Value.NextDouble() > 0.5;
        }

        //Número entre 0 e 1
        public static double RandomDouble()
        {
            return _random.Value.NextDouble();
        }

        //Normal padrão (Box-Muller)
        public static double Gaussian()
        {
            double u1 = 1.0 - _random.Value.NextDouble();
            double u2 = _random.Value.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }

        //Quantos de n eventos independentes de chance p (0..1) acontecem.
        //Exato para n pequeno, Poisson para eventos raros, aproximação normal para o resto
        public static int Binomial(int n, double p)
        {
            if (n <= 0 || p <= 0) return 0;
            if (p >= 1) return n;
            if (p > 0.5) return n - Binomial(n, 1 - p);
            //Exato quando é barato: n pequeno, ou p comum com poucos acertos esperados (n < 300)
            if (n <= 30 || (n * p < 15 && p >= 0.05))
            {
                int count = 0;
                for (int i = 0; i < n; i++)
                    if (_random.Value.NextDouble() < p) count++;
                return count;
            }
            if (n * p < 15)
            {
                //Poisson (Knuth): a normal erra muito quando n*p é pequeno
                double limit = Math.Exp(-n * p);
                double product = _random.Value.NextDouble();
                int k = 0;
                while (product > limit && k < n)
                {
                    k++;
                    product *= _random.Value.NextDouble();
                }
                return k;
            }
            double value = n * p + Math.Sqrt(n * p * (1 - p)) * Gaussian();
            return (int)Math.Max(0, Math.Min(n, Math.Round(value)));
        }
    }
}
