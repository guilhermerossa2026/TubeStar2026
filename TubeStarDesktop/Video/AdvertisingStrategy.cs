using System;
using System.Xml.Serialization;

namespace TubeStar
{
    [Serializable]
    public enum AdvertisingStrategy
    {
        [XmlEnum("1")]
        Low = 1,

        [XmlEnum("2")]
        Normal = 2,

        [XmlEnum("3")]
        High = 3,

        [XmlEnum("4")]
        Aggressive = 4,
    }

    public static class AdvertisingStrategyHelpers
    {
        public static string GetString(this AdvertisingStrategy category)
        {
            switch (category)
            {
                case AdvertisingStrategy.Low: return EnglishStrings.Low.Translate();
                case AdvertisingStrategy.Normal: return EnglishStrings.Normal.Translate();
                case AdvertisingStrategy.High: return EnglishStrings.High.Translate();
                case AdvertisingStrategy.Aggressive: return EnglishStrings.Aggressive.Translate();
            }
            return String.Empty;
        }
    }
}