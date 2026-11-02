using System;
using System.Collections.Generic;
using System.Linq;

namespace TubeStar
{
    /// <summary>
    /// Nicho do vídeo e do canal (substitui as 18 categorias antigas)
    /// </summary>
    [Serializable]
    public enum VideoCategory
    {
        Games = 1,
        Vlog = 2,
        Technology = 3,
        Comedy = 4,
        Education = 5,
        Finance = 6,
        Fitness = 7,
        Cooking = 8,
        Music = 9,
        Beauty = 10,
        Sports = 11,
        Reacts = 12,
        Mystery = 13,
        Travel = 14,
        Kids = 15,
    }

    public static class VideoCategoryHelpers
    {
        public const int Count = 15;

        public static string GetString(this VideoCategory category)
        {
            switch (category)
            {
                case VideoCategory.Games: return EnglishStrings.ThemeGames.Translate();
                case VideoCategory.Vlog: return EnglishStrings.ThemeVlog.Translate();
                case VideoCategory.Technology: return EnglishStrings.ThemeTech.Translate();
                case VideoCategory.Comedy: return EnglishStrings.ThemeComedy.Translate();
                case VideoCategory.Education: return EnglishStrings.ThemeEducation.Translate();
                case VideoCategory.Finance: return EnglishStrings.ThemeFinance.Translate();
                case VideoCategory.Fitness: return EnglishStrings.ThemeFitness.Translate();
                case VideoCategory.Cooking: return EnglishStrings.ThemeCooking.Translate();
                case VideoCategory.Music: return EnglishStrings.ThemeMusic.Translate();
                case VideoCategory.Beauty: return EnglishStrings.ThemeBeauty.Translate();
                case VideoCategory.Sports: return EnglishStrings.ThemeSports.Translate();
                case VideoCategory.Reacts: return EnglishStrings.ThemeReacts.Translate();
                case VideoCategory.Mystery: return EnglishStrings.ThemeMystery.Translate();
                case VideoCategory.Travel: return EnglishStrings.ThemeTravel.Translate();
                case VideoCategory.Kids: return EnglishStrings.ThemeKids.Translate();
            }
            return String.Empty;
        }

        //Quanto cada view paga em relação ao normal (1 = normal): anunciantes pagam mais por Finanças que por Infantil
        public static double GetRpmMultiplier(this VideoCategory category)
        {
            switch (category)
            {
                case VideoCategory.Finance: return 1.8;
                case VideoCategory.Technology: return 1.5;
                case VideoCategory.Education: return 1.3;
                case VideoCategory.Beauty: return 1.2;
                case VideoCategory.Fitness: return 1.2;
                case VideoCategory.Travel: return 1.1;
                case VideoCategory.Vlog: return 0.9;
                case VideoCategory.Mystery: return 0.9;
                case VideoCategory.Music: return 0.8;
                case VideoCategory.Comedy: return 0.8;
                case VideoCategory.Reacts: return 0.7;
                case VideoCategory.Games: return 0.7;
                case VideoCategory.Kids: return 0.6;
            }
            return 1.0;
        }

        //Concorrência: nicho lotado recebe menos views orgânicas (1 = normal, menor = mais concorrido)
        public static double GetCompetitionMultiplier(this VideoCategory category)
        {
            switch (category)
            {
                case VideoCategory.Games: return 0.8;
                case VideoCategory.Comedy: return 0.85;
                case VideoCategory.Reacts: return 0.85;
                case VideoCategory.Music: return 0.85;
                case VideoCategory.Vlog: return 0.9;
                case VideoCategory.Beauty: return 0.9;
                case VideoCategory.Kids: return 0.9;
                case VideoCategory.Technology: return 0.95;
                case VideoCategory.Sports: return 0.95;
                case VideoCategory.Education: return 1.05;
                case VideoCategory.Finance: return 1.1;
                case VideoCategory.Mystery: return 1.15;
                case VideoCategory.Travel: return 1.15;
            }
            return 1.0;
        }

        //Vídeo no nicho do canal? Canal sem nicho (save antigo) conta como no nicho
        public static bool IsInNiche(this Video video, Channel channel)
        {
            return channel == null || !channel.Niche.HasValue || channel.Niche.Value == video.Category;
        }
    }
}
