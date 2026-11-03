using System;

namespace TubeStar
{
    /// <summary>
    /// Tipo do vídeo (substitui os atributos na criação). Cada tipo combina com alguns nichos
    /// </summary>
    [Serializable]
    public enum VideoType
    {
        Tutorial = 1,
        Review = 2,
        React = 3,
        Challenge = 4,
        Vlog = 5,
        Top10 = 6,
        Documentary = 7,
        Gameplay = 8,
    }

    public static class VideoTypeHelpers
    {
        //Bônus de views quando o tipo combina com o nicho do vídeo (sem penalidade se não combinar)
        public const double MatchBonus = 1.10;

        public static string GetString(this VideoType type)
        {
            switch (type)
            {
                case VideoType.Tutorial: return EnglishStrings.TypeTutorial.Translate();
                case VideoType.Review: return EnglishStrings.TypeReview.Translate();
                case VideoType.React: return EnglishStrings.TypeReact.Translate();
                case VideoType.Challenge: return EnglishStrings.TypeChallenge.Translate();
                case VideoType.Vlog: return EnglishStrings.TypeVlog.Translate();
                case VideoType.Top10: return EnglishStrings.TypeTop10.Translate();
                case VideoType.Documentary: return EnglishStrings.TypeDocumentary.Translate();
                case VideoType.Gameplay: return EnglishStrings.TypeGameplay.Translate();
            }
            return String.Empty;
        }

        public static VideoCategory[] GetMatchingNiches(this VideoType type)
        {
            switch (type)
            {
                case VideoType.Tutorial: return new[] { VideoCategory.Education, VideoCategory.Technology, VideoCategory.Cooking, VideoCategory.Fitness, VideoCategory.Beauty };
                case VideoType.Review: return new[] { VideoCategory.Technology, VideoCategory.Games, VideoCategory.Beauty, VideoCategory.Finance };
                case VideoType.React: return new[] { VideoCategory.Reacts, VideoCategory.Comedy, VideoCategory.Music, VideoCategory.Games };
                case VideoType.Challenge: return new[] { VideoCategory.Games, VideoCategory.Comedy, VideoCategory.Fitness, VideoCategory.Sports, VideoCategory.Kids };
                case VideoType.Vlog: return new[] { VideoCategory.Vlog, VideoCategory.Travel, VideoCategory.Fitness, VideoCategory.Cooking };
                case VideoType.Top10: return new[] { VideoCategory.Mystery, VideoCategory.Games, VideoCategory.Sports, VideoCategory.Music, VideoCategory.Travel };
                case VideoType.Documentary: return new[] { VideoCategory.Mystery, VideoCategory.Education, VideoCategory.Finance, VideoCategory.Sports, VideoCategory.Travel };
                case VideoType.Gameplay: return new[] { VideoCategory.Games, VideoCategory.Kids };
            }
            return new VideoCategory[0];
        }

        public static bool Matches(this VideoType type, VideoCategory niche)
        {
            return Array.IndexOf(type.GetMatchingNiches(), niche) >= 0;
        }

        //Multiplicador de views do vídeo: vídeo antigo (sem tipo) fica neutro
        public static double GetTypeMultiplier(this Video video)
        {
            return video.Type.HasValue && video.Type.Value.Matches(video.Category) ? MatchBonus : 1.0;
        }

        //Nome do tipo para exibir; vazio em vídeo antigo
        public static string GetTypeString(this Video video)
        {
            return video.Type.HasValue ? video.Type.Value.GetString() : String.Empty;
        }
    }
}
