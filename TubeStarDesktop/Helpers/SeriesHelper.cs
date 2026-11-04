using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TubeStar
{
    /// <summary>
    /// Séries de vídeos: títulos que terminam em "#número" (ex.: "Minecraft Guerra #3")
    /// </summary>
    public class SeriesInfo
    {
        public string Name { get; set; }   //Base, na grafia do vídeo mais recente
        public int LastNumber { get; set; } //Maior número existente
        public int Recency { get; set; }    //Menor = mais recente (em produção = -1)
        public VideoCategory Category { get; set; } //Nicho do último episódio (maior número)
        public VideoType? Type { get; set; }        //Tipo do último episódio
    }

    public static class SeriesHelper
    {
        private const int MaxTitleLength = 100;
        private static readonly Regex SeriesRegex = new Regex(@"^(.*?)\s*#(\d+)\s*$", RegexOptions.Compiled);

        //Séries do jogador, da mais recente para a mais antiga
        public static List<SeriesInfo> GetSeries()
        {
            var seen = new HashSet<Video>();
            var map = new Dictionary<string, SeriesInfo>(StringComparer.Ordinal);

            foreach (var channel in Player.Current.Channels)
            {
                foreach (var video in channel.Videos)
                    Add(video, false, seen, map);
            }
            foreach (var video in Player.Current.Videos)
                Add(video, true, seen, map);
            foreach (var task in Player.Current.TasksInProgress)
            {
                var shoot = task as ShootVideo;
                if (shoot != null)
                    Add(shoot.Video, true, seen, map);
                var edit = task as EditVideo;
                if (edit != null)
                    Add(edit.Video, true, seen, map);
            }

            return map.Values.OrderBy(s => s.Recency).ThenBy(s => s.Name).ToList();
        }

        //Séries da lista "Continuar série": sem as removidas pelo jogador (voltam com um episódio novo)
        public static List<SeriesInfo> GetVisibleSeries()
        {
            var hidden = Player.Current.HiddenSeries;
            if (hidden == null || hidden.Count == 0)
                return GetSeries();
            int last;
            return GetSeries().Where(s => !hidden.TryGetValue(Key(s.Name), out last) || s.LastNumber > last).ToList();
        }

        //Tira a série da lista; os vídeos continuam no canal
        public static void Hide(SeriesInfo series)
        {
            if (Player.Current.HiddenSeries == null)
                Player.Current.HiddenSeries = new Dictionary<string, int>();
            Player.Current.HiddenSeries[Key(series.Name)] = series.LastNumber;
        }

        private static void Add(Video video, bool inProduction, HashSet<Video> seen, Dictionary<string, SeriesInfo> map)
        {
            if (video == null || video.Name == null || !seen.Add(video))
                return;

            string baseName;
            int number;
            if (!TryParse(video.Name, out baseName, out number))
                return;

            int recency = inProduction ? -1 : video.Iterations;
            string key = Key(baseName);
            SeriesInfo info;
            if (!map.TryGetValue(key, out info))
            {
                map[key] = new SeriesInfo { Name = baseName, LastNumber = number, Recency = recency, Category = video.Category, Type = video.Type };
                return;
            }
            if (number > info.LastNumber)
            {
                info.LastNumber = number;
                info.Category = video.Category;
                info.Type = video.Type;
            }
            if (recency < info.Recency)
            {
                info.Recency = recency;
                info.Name = baseName;
            }
        }

        //Chave da série: sem diferenciar maiúsculas nem acentos ("Finlândia" = "FINLANDIA")
        private static string Key(string baseName)
        {
            var sb = new StringBuilder();
            foreach (char c in baseName.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant();
        }

        //Série já existente do título (com ou sem "#número"); nula se não houver
        public static SeriesInfo FindSeries(string title)
        {
            if (String.IsNullOrEmpty(title))
                return null;
            string baseName;
            int number;
            if (!TryParse(title.Trim(), out baseName, out number))
                baseName = title.Trim();
            string key = Key(baseName);
            foreach (var series in GetSeries())
            {
                if (Key(series.Name) == key)
                    return series;
            }
            return null;
        }

        private static bool TryParse(string title, out string baseName, out int number)
        {
            baseName = null;
            number = 0;
            var match = SeriesRegex.Match(title);
            if (!match.Success)
                return false;
            baseName = match.Groups[1].Value.Trim();
            if (baseName.Length == 0 || !Int32.TryParse(match.Groups[2].Value, out number))
                return false;
            return true;
        }

        //Próximo título da série. Vazio devolve vazio
        public static string NextTitle(string typed)
        {
            if (typed == null)
                return typed;
            typed = typed.Trim();
            if (typed.Length == 0)
                return typed;

            string baseName = typed;
            int number;
            string parsedBase;
            if (TryParse(typed, out parsedBase, out number))
                baseName = parsedBase;
            else
            {
                //"#" solto no fim (ou número grande demais) sai da base; "#" sozinho não vira série
                baseName = Regex.Replace(typed, @"\s*#\d*\s*$", String.Empty).Trim();
                if (baseName.Length == 0)
                    return typed;
            }

            int next = 1;
            var existing = FindSeries(baseName);
            if (existing != null)
            {
                if (existing.LastNumber == Int32.MaxValue)
                    return typed;
                next = existing.LastNumber + 1;
            }

            string suffix = " #" + next;
            if (baseName.Length + suffix.Length > MaxTitleLength)
                baseName = baseName.Substring(0, MaxTitleLength - suffix.Length).TrimEnd();
            return baseName + suffix;
        }
    }
}
