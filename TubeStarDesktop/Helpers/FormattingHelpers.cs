using System;
using System.Globalization;

namespace TubeStar
{
    public static class FormattingHelpers
    {
        private static readonly CultureInfo CurrentCulture = new CultureInfo("en-US");

        public static string ToCurrencyString(this int instance)
        {
            return ((double)instance).ToCurrencyString();
        }

        public static string ToCurrencyString(this double instance)
        {
            instance = Math.Round(instance, 2);
            return instance.ToString("C", CurrentCulture);
        }

        //Formato curto, como no YouTube: 845, 12,4 mil, 1,19 mi, 2,34 bi, 1,25 tri (corta, não arredonda para cima)
        public static string ToShortNumber(this long instance)
        {
            if (instance == long.MinValue)
                instance = long.MinValue + 1;
            if (instance < 0)
                return "-" + ToShortNumber(-instance);
            if (instance < 1000)
                return instance.ToString();

            double value;
            EnglishStrings suffix;
            if (instance < 1000000)
            {
                value = instance / 1000.0;
                suffix = EnglishStrings.NumberThousand;
            }
            else if (instance < 1000000000)
            {
                value = instance / 1000000.0;
                suffix = EnglishStrings.NumberMillion;
            }
            else if (instance < 1000000000000)
            {
                value = instance / 1000000000.0;
                suffix = EnglishStrings.NumberBillion;
            }
            else
            {
                value = instance / 1000000000000.0;
                suffix = EnglishStrings.NumberTrillion;
            }

            int decimals = value < 10 ? 2 : (value < 100 ? 1 : 0);
            if (suffix == EnglishStrings.NumberThousand)
                decimals = Math.Min(decimals, 1);
            double factor = Math.Pow(10, decimals);
            //Conta em inteiros: com double, 1,15 x 100 dá 114,999... e viraria 1,14
            long unit = suffix == EnglishStrings.NumberThousand ? 1000 : (suffix == EnglishStrings.NumberMillion ? 1000000 : (suffix == EnglishStrings.NumberBillion ? 1000000000 : 1000000000000));
            value = (instance * (long)factor / unit) / factor;

            string text = value.ToString("0." + new string('#', Math.Max(decimals, 1)), CultureInfo.InvariantCulture).Replace(".", EnglishStrings.NumberDecimalSeparator.Translate());
            return text + " " + suffix.Translate();
        }

        public static string ToShortNumber(this int instance)
        {
            return ((long)instance).ToShortNumber();
        }

        public static string ToNumberString(this long instance)
        {
            return instance.ToString("N0", CurrentCulture);
        }

        public static string ToNumberString(this int instance)
        {
            return instance.ToString("N0", CurrentCulture);
        }
    }
}