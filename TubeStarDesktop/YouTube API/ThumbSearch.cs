using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace TubeStar
{
    /// <summary>
    /// Único caminho para buscar miniaturas na API. A cota do YouTube é de 100 buscas por dia (zera à meia-noite do Pacífico),
    /// então cada busca fica guardada em Saves\search_cache.json e o mesmo título nunca é buscado duas vezes
    /// </summary>
    public static class ThumbSearch
    {
        public const int DailyLimit = 100;
        private const int MaxCachedSearches = 500;

        public class Page
        {
            public List<string> Ids = new List<string>();
            public string NextPageToken;
            //Veio da memória (não gastou busca); não é salvo
            [Newtonsoft.Json.JsonIgnore]
            public bool FromCache;
        }

        private class CacheData
        {
            public Dictionary<string, Page> Pages = new Dictionary<string, Page>();
            public List<string> Order = new List<string>();
            public string Day;
            public int Count;
            public string BlockedDay;
        }

        private static CacheData _data;
        private static Dictionary<string, List<KeyValuePair<Action<Page>, Action>>> _pending = new Dictionary<string, List<KeyValuePair<Action<Page>, Action>>>();

        private static string CacheFile
        {
            get { return SaveLoadHelper.SaveDirectory + @"\search_cache.json"; }
        }

        //Dia da cota (horário do Pacífico)
        private static string Today
        {
            get
            {
                DateTime now = DateTime.UtcNow;
                try
                {
                    now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(now, "Pacific Standard Time");
                }
                catch { now = now.AddHours(-8); }
                return now.ToString("yyyy-MM-dd");
            }
        }

        private static CacheData Data
        {
            get
            {
                if (_data == null)
                {
                    try
                    {
                        if (File.Exists(CacheFile))
                            _data = JsonConvert.DeserializeObject<CacheData>(File.ReadAllText(CacheFile));
                    }
                    catch { }
                    if (_data == null)
                        _data = new CacheData();
                    if (_data.Pages == null) _data.Pages = new Dictionary<string, Page>();
                    if (_data.Order == null) _data.Order = new List<string>();
                }
                if (_data.Day != Today)
                {
                    _data.Day = Today;
                    _data.Count = 0;
                }
                return _data;
            }
        }

        public static int SearchesToday
        {
            get { return Data.Count; }
        }

        //Cota acabou hoje: não tenta de novo até virar o dia
        public static bool QuotaReached
        {
            get { return Data.BlockedDay == Today || Data.Count >= DailyLimit; }
        }

        //"Buscas hoje: 12/100"
        public static string UsageText
        {
            get
            {
                string text = String.Format(EnglishStrings.ThumbSearchesToday.Translate(), QuotaReached ? DailyLimit : SearchesToday, DailyLimit);
                return QuotaReached ? text + " " + EnglishStrings.ThumbLimitReached.Translate() : text;
            }
        }

        //Mensagem de erro certa: cota esgotada ou sem internet/chave
        public static string ErrorText
        {
            get { return QuotaReached ? EnglishStrings.ThumbQuotaReached.Translate() : EnglishStrings.ThumbOffline.Translate(); }
        }

        //Minúsculas, sem "#número" (nem "#" solto) e com espaços normalizados: "Minecraft #18" e "#19" são a mesma busca
        public static string CleanQuery(string query)
        {
            string text = Regex.Replace(query ?? "", @"#\s*\d*", " ");
            text = Regex.Replace(text, @"\s+", " ");
            return text.Trim().ToLowerInvariant();
        }

        private static string Key(string query, int max, string pageToken)
        {
            return CleanQuery(query) + "|" + max + "|" + (pageToken ?? "");
        }

        public static bool IsCached(string query, int max, string pageToken)
        {
            return Data.Pages.ContainsKey(Key(query, max, pageToken));
        }

        //onError: sem internet, sem chave ou cota esgotada (ver QuotaReached)
        public static void Search(string query, int max, string pageToken, Action<Page> onDone, Action onError)
        {
            string key = Key(query ?? "", max, pageToken);
            Page cached;
            if (Data.Pages.TryGetValue(key, out cached) && cached != null && cached.Ids != null)
            {
                cached.FromCache = true;
                onDone(cached);
                return;
            }
            if (!YouTubeAPI.HasApiKey || QuotaReached)
            {
                if (onError != null)
                    onError();
                return;
            }

            //Mesma busca já em andamento: espera a resposta dela em vez de gastar outra
            List<KeyValuePair<Action<Page>, Action>> waiting;
            if (_pending.TryGetValue(key, out waiting))
            {
                waiting.Add(new KeyValuePair<Action<Page>, Action>(onDone, onError));
                return;
            }
            waiting = new List<KeyValuePair<Action<Page>, Action>>();
            waiting.Add(new KeyValuePair<Action<Page>, Action>(onDone, onError));
            _pending[key] = waiting;

            Data.Count++;
            Save();

            var client = new WebClient();
            client.Encoding = Encoding.UTF8;
            client.DownloadStringCompleted += (s, e) =>
            {
                Page page = null;
                if (e.Error == null)
                {
                    try
                    {
                        var response = JsonConvert.DeserializeObject<YouTubeSearchResponse>(e.Result);
                        if (response != null)
                        {
                            page = new Page { NextPageToken = response.NextPageToken };
                            if (response.Entries != null)
                            {
                                foreach (var entry in response.Entries)
                                {
                                    if (entry != null && entry.Id != null && !String.IsNullOrEmpty(entry.Id.VideoId)
                                        && !(entry.Snippet != null && ThumbLibrary.IsShortTitle(entry.Snippet.Title)))
                                        page.Ids.Add(entry.Id.VideoId);
                                }
                            }
                        }
                    }
                    catch (JsonException) { }
                }
                else if (IsQuotaError(e.Error))
                {
                    Data.BlockedDay = Today;
                    Save();
                }
                else if (e.Error is WebException && ((WebException)e.Error).Response == null && Data.Count > 0)
                {
                    //Não chegou ao Google (sem internet): não gastou cota
                    Data.Count--;
                    Save();
                }

                //Resultado vazio não fica guardado (pode ser falha passageira)
                if (page != null && page.Ids.Count > 0)
                    Add(key, page);

                _pending.Remove(key);
                foreach (var callback in waiting)
                {
                    if (page != null)
                        callback.Key(page);
                    else if (callback.Value != null)
                        callback.Value();
                }
            };
            client.DownloadStringAsync(YouTubeAPI.SearchVideos(CleanQuery(query), max, pageToken));
        }

        internal static bool IsQuotaError(Exception error)
        {
            var web = error as WebException;
            if (web == null)
                return false;
            var response = web.Response as HttpWebResponse;
            if (response == null)
                return false;
            if ((int)response.StatusCode == 429)
                return true;
            if (response.StatusCode != HttpStatusCode.Forbidden)
                return false;
            try
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                    return reader.ReadToEnd().IndexOf("quota", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch { return false; }
        }

        private static void Add(string key, Page page)
        {
            Data.Pages[key] = page;
            Data.Order.Remove(key);
            Data.Order.Add(key);
            while (Data.Order.Count > MaxCachedSearches)
            {
                Data.Pages.Remove(Data.Order[0]);
                Data.Order.RemoveAt(0);
            }
            Save();
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(SaveLoadHelper.SaveDirectory);
                File.WriteAllText(CacheFile, JsonConvert.SerializeObject(_data));
            }
            catch { }
        }
    }
}
