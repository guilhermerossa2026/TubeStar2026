using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace TubeStar
{
    public class YouTubeAPI
    {
        public const string ApiKeyFileName = "youtube_api_key.txt";
        private static readonly string ApiKey = LoadApiKey();

        // A chave fica fora do git: arquivo ao lado do exe ou variável de ambiente YOUTUBE_API_KEY
        private static string LoadApiKey()
        {
            try
            {
                var file = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ApiKeyFileName);
                if (System.IO.File.Exists(file))
                {
                    var key = System.IO.File.ReadAllText(file).Trim();
                    if (key.Length > 0)
                        return key;
                }
            }
            catch { }

            return (Environment.GetEnvironmentVariable("YOUTUBE_API_KEY") ?? "").Trim();
        }

        public static Uri GetRandomImages(string search, int max)
        {
            search = search.Replace(' ', ',');
            var uriString = String.Format("https://www.googleapis.com/youtube/v3/search?part=snippet&q={0}&type=video&&maxResults={1}&fields=nextPageToken,prevPageToken,items(id(videoId))&key={2}{3}",
                HttpHelpers.UrlEncoding(search), max, ApiKey, "");
            return new Uri(uriString);
        }

        public static bool HasApiKey
        {
            get { return !String.IsNullOrEmpty(ApiKey); }
        }

        //Busca por relevância (padrão da API), com página opcional (pageToken)
        public static Uri SearchVideos(string search, int max, string pageToken)
        {
            var uriString = String.Format("https://www.googleapis.com/youtube/v3/search?part=snippet&q={0}&type=video&maxResults={1}&fields=nextPageToken,items(id(videoId))&key={2}{3}{4}",
                HttpHelpers.UrlEncoding(search), max, ApiKey, "",
                String.IsNullOrEmpty(pageToken) ? "" : "&pageToken=" + HttpHelpers.UrlEncoding(pageToken));
            return new Uri(uriString);
        }

        //Biblioteca de miniaturas: canal por @handle (1 unidade de cota)
        public static Uri ChannelUploadsUri(string handle)
        {
            var uriString = String.Format("https://www.googleapis.com/youtube/v3/channels?part=contentDetails&forHandle={0}&fields=items(contentDetails(relatedPlaylists(uploads)))&key={1}",
                HttpHelpers.UrlEncoding(handle), ApiKey);
            return new Uri(uriString);
        }

        //Biblioteca de miniaturas: até 50 vídeos de uma playlist de uploads (1 unidade de cota)
        public static Uri PlaylistItemsUri(string playlistId, string pageToken)
        {
            var uriString = String.Format("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId={0}&maxResults=50&fields=nextPageToken,items(snippet(title,resourceId(videoId)))&key={1}{2}",
                HttpHelpers.UrlEncoding(playlistId), ApiKey,
                String.IsNullOrEmpty(pageToken) ? "" : "&pageToken=" + HttpHelpers.UrlEncoding(pageToken));
            return new Uri(uriString);
        }

        public static Uri GetRandomImagesWithTitle(string search, int max)
        {
            search = search.Replace(' ', ',');
            var uriString = String.Format("https://www.googleapis.com/youtube/v3/search?part=snippet&q={0}&type=video&&maxResults={1}&fields=nextPageToken,prevPageToken,items(id(videoId),snippet(title))&key={2}{3}",
                HttpHelpers.UrlEncoding(search), max, ApiKey, "");
            return new Uri(uriString);
        }

        public static Uri GetPhotoUri(string id)
        {
            return new Uri(String.Format("http://i.ytimg.com/vi/{0}/hqdefault.jpg", id));
        }

        public static Uri GetSmallPhotoUri(string id)
        {
            return new Uri(String.Format("http://i.ytimg.com/vi/{0}/default.jpg", id));
        }

        public static Uri GetLinkUri(string id)
        {
            return new Uri(String.Format("http://www.youtube.com/watch?v={0}", id));
        }
    }

    [JsonObject()]
    public class YouTubeSearchResponse
    {
        [JsonProperty("prevPageToken")]
        public string PreviousPageToken { get; set; }

        [JsonProperty("nextPageToken")]
        public string NextPageToken { get; set; }

        [JsonProperty("items")]
        public List<YouTubeSearchEntry> Entries { get; set; }
    }

    [JsonObject()]
    public class YouTubeSearchEntry
    {
        [JsonProperty("id")]
        public YouTubeSearchId Id { get; set; }

        [JsonProperty("snippet")]
        public YouTubeSearchTitle Snippet { get; set; }
    }

    [JsonObject()]
    public class YouTubeSearchTitle
    {
        [JsonProperty("title")]
        public string Title { get; set; }
    }

    [JsonObject()]
    public class YouTubeSearchId
    {
        [JsonProperty("videoId")]
        public string VideoId { get; set; }
    }
}