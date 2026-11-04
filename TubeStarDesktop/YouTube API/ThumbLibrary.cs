using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TubeStar
{
    /// <summary>
    /// Biblioteca de miniaturas por nicho: títulos dos vídeos recentes de canais conhecidos (lista em thumb_channels.txt),
    /// guardada em Saves\thumb_library.json e atualizada em segundo plano a cada 7 dias.
    /// Usa channels.list e playlistItems.list (1 unidade cada); não conta no contador "Buscas hoje" (só search.list conta).
    /// A tela só lê o snapshot pronto, nunca espera a atualização.
    /// </summary>
    public static class ThumbLibrary
    {
        public const string ChannelsFileName = "thumb_channels.txt";
        private const int MaxAgeDays = 7;
        private const int PagesPerChannel = 2;
        private const int PagesPerGamesChannel = 10; //canais de Games têm muitos vídeos antigos (500)
        private const int CurrentVersion = 4; //versão menor = biblioteca antiga, baixa de novo uma vez
        private const int MaxResults = 50;

        public class Entry
        {
            public string Id;
            public string Title;
            public string Niche;
            public string Channel;

            [JsonIgnore]
            public HashSet<string> Words;
        }

        private class Data
        {
            public string Updated;
            public int Version;
            public List<Entry> Videos = new List<Entry>();
        }

        private static readonly string[] StopWords = { "de", "do", "da", "dos", "das", "e", "o", "a", "os", "as", "com", "para", "pra",
            "meu", "minha", "no", "na", "um", "uma", "the", "my", "how", "to", "of", "and", "video", "vídeo", "ep", "parte", "part" };

        private static HashSet<string> _stop;
        //Siglas curtas que importam no título ("GTA RP", "EA FC")
        private static readonly HashSet<string> ShortKeep = new HashSet<string> { "rp", "fc", "pc", "ps", "tv", "ia", "ai", "3d", "4k", "vr" };
        private static volatile Data _snapshot; //só é trocado inteiro por outro pronto
        private static bool _started;

        //Progresso da atualização (lido pela UI; escrito pela thread de fundo)
        private static volatile bool _updating;
        private static volatile bool _failed; //a última atualização falhou e a biblioteca anterior continua em uso
        private static int _channelsDone;
        private static int _channelsTotal;

        public static bool IsUpdating { get { return _updating; } }
        public static bool LastUpdateFailed { get { return _failed; } }
        public static int ChannelsDone { get { return Thread.VolatileRead(ref _channelsDone); } }
        public static int ChannelsTotal { get { return Thread.VolatileRead(ref _channelsTotal); } }

        public static int VideoCount
        {
            get
            {
                var data = _snapshot;
                return data == null ? 0 : data.Videos.Count;
            }
        }

        //Disparado da thread de fundo: a UI precisa usar Dispatcher.BeginInvoke
        public static event Action ProgressChanged;

        private static void RaiseProgress()
        {
            try
            {
                var handler = ProgressChanged;
                if (handler != null)
                    handler();
            }
            catch { }
        }
        private static readonly Random _random = new Random();

        private static string LibraryFile
        {
            get { return SaveLoadHelper.SaveDirectory + @"\thumb_library.json"; }
        }

        private static string ChannelsFile
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ChannelsFileName); }
        }

        //Chamado ao abrir o jogo: lê a biblioteca e, se for velha ou não existir, atualiza em segundo plano
        public static void StartBackground()
        {
            if (_started)
                return;
            _started = true;
            var thread = new Thread(Run);
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();
        }

        private static void Run()
        {
            try
            {
                Data data = ReadFile();
                if (data != null)
                    _snapshot = data;

                if (!NeedsUpdate(data) || !YouTubeAPI.HasApiKey)
                    return;
                Update(data);
            }
            catch { }
            finally
            {
                _updating = false;
                RaiseProgress();
            }
        }

        private static Data ReadFile()
        {
            try
            {
                if (File.Exists(LibraryFile))
                {
                    var data = JsonConvert.DeserializeObject<Data>(File.ReadAllText(LibraryFile));
                    if (data != null && data.Videos != null)
                    {
                        Prepare(data);
                        return data;
                    }
                }
            }
            catch { }
            return null;
        }

        private static bool NeedsUpdate(Data data)
        {
            if (data == null || String.IsNullOrEmpty(data.Updated) || data.Version < CurrentVersion)
                return true;
            DateTime updated;
            if (!DateTime.TryParse(data.Updated, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out updated))
                return true;
            return (DateTime.UtcNow - updated).TotalDays > MaxAgeDays;
        }

        private static void Prepare(Data data)
        {
            foreach (var entry in data.Videos)
                entry.Words = new HashSet<string>(Normalize(entry.Title));
        }

        #region Atualização

        private static void Update(Data old)
        {
            var lines = ReadChannelLines();
            if (lines.Count == 0)
                return;

            _channelsDone = 0;
            _channelsTotal = lines.Count;
            _updating = true;
            RaiseProgress();

            //Vídeos por canal: canal que falha mantém os vídeos da biblioteca anterior
            var byChannel = new Dictionary<string, List<Entry>>();
            var order = new List<string>();
            if (old != null)
            {
                //Só mantém vídeos de canais que ainda estão no thumb_channels.txt
                var current = new HashSet<string>();
                foreach (var line in lines)
                    current.Add(line.Name);
                foreach (var entry in old.Videos)
                {
                    string name = entry.Channel ?? "";
                    if (!current.Contains(name))
                        continue;
                    if (!byChannel.ContainsKey(name))
                    {
                        byChannel[name] = new List<Entry>();
                        order.Add(name);
                    }
                    byChannel[name].Add(entry);
                }
            }

            int succeeded = 0;
            bool stopped = false;
            foreach (var line in lines)
            {
                bool stop;
                var videos = FetchChannel(line, out stop);
                Interlocked.Increment(ref _channelsDone);
                RaiseProgress();
                if (stop)
                {
                    stopped = true;
                    break;
                }
                if (videos == null || videos.Count == 0)
                    continue;
                succeeded++;
                if (!byChannel.ContainsKey(line.Name))
                    order.Add(line.Name);
                byChannel[line.Name] = videos;
            }

            //Nada novo (sem internet, cota, etc.): mantém a anterior como está
            _failed = stopped || succeeded == 0;
            if (succeeded == 0)
                return;

            var data = new Data();
            //Parou no meio (cota): mantém a versão antiga para completar na próxima abertura
            data.Version = stopped ? (old != null ? old.Version : 0) : CurrentVersion;
            //Parou no meio (cota): mantém a data antiga para tentar de novo na próxima abertura
            data.Updated = stopped
                ? (old != null ? old.Updated : null)
                : DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var seen = new HashSet<string>();
            foreach (var name in order)
            {
                foreach (var entry in byChannel[name])
                {
                    if (seen.Add(entry.Id))
                        data.Videos.Add(entry);
                }
            }
            Prepare(data);

            try
            {
                Directory.CreateDirectory(SaveLoadHelper.SaveDirectory);
                //Grava num temporário e troca: fechar o jogo no meio não deixa o arquivo pela metade
                string tmp = LibraryFile + ".tmp";
                File.WriteAllText(tmp, JsonConvert.SerializeObject(data));
                if (File.Exists(LibraryFile))
                    File.Replace(tmp, LibraryFile, null);
                else
                    File.Move(tmp, LibraryFile);
            }
            catch { }
            _snapshot = data;
        }

        private class ChannelLine
        {
            public string Niche;
            public string Reference; //@handle ou ID UC...
            public string Name;
        }

        //"Nicho | @handle ou UC... | Nome"; # começa comentário
        private static List<ChannelLine> ReadChannelLines()
        {
            var list = new List<ChannelLine>();
            try
            {
                if (!File.Exists(ChannelsFile))
                    return list;
                foreach (var raw in File.ReadAllLines(ChannelsFile, Encoding.UTF8))
                {
                    string line = raw;
                    int hash = line.IndexOf('#');
                    if (hash >= 0)
                        line = line.Substring(0, hash);
                    var parts = line.Split('|');
                    if (parts.Length < 2)
                        continue;
                    string niche = parts[0].Trim();
                    string reference = parts[1].Trim();
                    if (niche.Length == 0 || reference.Length == 0 || !Enum.IsDefined(typeof(VideoCategory), niche))
                        continue;
                    string name = parts.Length > 2 ? parts[2].Trim() : "";
                    list.Add(new ChannelLine { Niche = niche, Reference = reference, Name = name.Length > 0 ? name : reference });
                }
            }
            catch { }
            return list;
        }

        //Os 2 passos de um canal; stop = a API recusou (cota/chave): para tudo
        private static List<Entry> FetchChannel(ChannelLine line, out bool stop)
        {
            stop = false;
            try
            {
                //Playlist direta (PL...) ou link com list=: sem UULF e sem channels.list
                string playlist = PlaylistIdOf(line.Reference);
                if (playlist != null)
                    return FetchPlaylist(playlist, line);

                string uploads;
                if (line.Reference.Length == 24 && line.Reference.StartsWith("UC"))
                {
                    //A lista de uploads de um canal UCxxxx é sempre UUxxxx
                    uploads = "UU" + line.Reference.Substring(2);
                }
                else
                {
                    var channel = JObject.Parse(Download(YouTubeAPI.ChannelUploadsUri(line.Reference)));
                    JToken token = channel.SelectToken("items[0].contentDetails.relatedPlaylists.uploads");
                    if (token == null)
                        return null;
                    uploads = (string)token;
                }

                //Primeiro a playlist só de vídeos longos (UULF: sem Shorts e sem lives); se não existir ou vier vazia, usa a de uploads
                List<Entry> videos = null;
                try
                {
                    videos = FetchPlaylist("UULF" + uploads.Substring(2), line);
                }
                catch (WebException ex)
                {
                    //404 etc. só cai para o fallback; cota/chave para tudo
                    if (IsStopError(ex))
                    {
                        stop = true;
                        return null;
                    }
                }
                catch { }
                if (videos == null || videos.Count == 0)
                    videos = FetchPlaylist(uploads, line);
                return videos;
            }
            catch (WebException ex)
            {
                //Cota ou chave inválida/não configurada para tudo; outro erro pula só este canal
                stop = IsStopError(ex);
                return null;
            }
            catch
            {
                return null;
            }
        }

        //ID da playlist se a referência for uma (começa com PL ou é um link com list=); senão null
        private static string PlaylistIdOf(string reference)
        {
            int pos = reference.IndexOf("list=", StringComparison.OrdinalIgnoreCase);
            if (pos >= 0)
            {
                string id = reference.Substring(pos + 5);
                int end = id.IndexOfAny(new[] { '&', '#', ' ' });
                if (end >= 0)
                    id = id.Substring(0, end);
                return id.Length > 0 ? id : null;
            }
            return reference.StartsWith("PL") ? reference : null;
        }

        //Baixa as páginas de uma playlist, sem Shorts marcados no título; erros de rede sobem para quem chamou
        private static List<Entry> FetchPlaylist(string playlistId, ChannelLine line)
        {
            var videos = new List<Entry>();
            string pageToken = null;
            int pages = line.Niche == VideoCategory.Games.ToString() ? PagesPerGamesChannel : PagesPerChannel;
            for (int page = 0; page < pages; page++)
            {
                var json = JObject.Parse(Download(YouTubeAPI.PlaylistItemsUri(playlistId, pageToken)));
                JArray items = json["items"] as JArray;
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        JToken title = item.SelectToken("snippet.title");
                        JToken id = item.SelectToken("snippet.resourceId.videoId");
                        if (title == null || id == null)
                            continue;
                        string t = (string)title;
                        if (t == "Private video" || t == "Deleted video" || IsShortTitle(t))
                            continue;
                        videos.Add(new Entry { Id = (string)id, Title = t, Niche = line.Niche, Channel = line.Name });
                    }
                }
                JToken next = json["nextPageToken"];
                if (next == null)
                    break;
                pageToken = (string)next;
            }
            return videos;
        }

        //Título com "#short" ou "#shorts" (sem diferenciar maiúsculas nem acentos)
        public static bool IsShortTitle(string title)
        {
            if (String.IsNullOrEmpty(title))
                return false;
            //Só a hashtag inteira: "#short" e "#shorts" sim; "#shortfilm" e "#shortcut" não
            return System.Text.RegularExpressions.Regex.IsMatch(title, @"#shorts?(?![\p{L}\p{N}_-])", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        //Lê o corpo do erro uma vez: true = cota esgotada ou chave inválida/não configurada
        private static bool IsStopError(WebException ex)
        {
            var response = ex.Response as HttpWebResponse;
            if (response == null)
                return false;
            int code = (int)response.StatusCode;
            if (code == 429)
                return true;
            if (code != 400 && code != 403)
                return false;
            string body;
            try
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                    body = reader.ReadToEnd();
            }
            catch { return false; }
            return body.IndexOf("quota", StringComparison.OrdinalIgnoreCase) >= 0
                || body.IndexOf("keyInvalid", StringComparison.OrdinalIgnoreCase) >= 0
                || body.IndexOf("API key", StringComparison.OrdinalIgnoreCase) >= 0
                || body.IndexOf("accessNotConfigured", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        //WebClient sem Timeout próprio: limita cada requisição a 20 s
        private class TimeoutWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address);
                if (request != null)
                {
                    request.Timeout = 20000;
                    var http = request as HttpWebRequest;
                    if (http != null)
                        http.ReadWriteTimeout = 20000;
                }
                return request;
            }
        }

        private static string Download(Uri uri)
        {
            using (var client = new TimeoutWebClient())
            {
                client.Encoding = Encoding.UTF8;
                return client.DownloadString(uri);
            }
        }

        #endregion

        #region Consulta (usada pela tela)

        public static bool HasVideos
        {
            get { return _snapshot != null && _snapshot.Videos.Count > 0; }
        }

        //minúsculas, sem acento, sem pontuação, sem "#número" nem números soltos, sem palavras curtas e stopwords
        public static List<string> Normalize(string text)
        {
            var result = new List<string>();
            if (String.IsNullOrEmpty(text))
                return result;
            if (_stop == null)
                _stop = new HashSet<string>(StopWords);

            var sb = new StringBuilder();
            string lower = text.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            for (int i = 0; i < lower.Length; i++)
            {
                char c = lower[i];
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category == UnicodeCategory.NonSpacingMark)
                    continue;
                if (c == '#')
                {
                    //"#12" some junto com o número
                    int j = i + 1;
                    while (j < lower.Length && Char.IsDigit(lower[j]))
                        j++;
                    if (j > i + 1)
                        i = j - 1;
                    sb.Append(' ');
                }
                else if (Char.IsLetterOrDigit(c))
                    sb.Append(c);
                else
                    sb.Append(' ');
            }

            foreach (var word in sb.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if ((word.Length <= 2 && !ShortKeep.Contains(word)) || _stop.Contains(word) || IsNumber(word) || result.Contains(word))
                    continue;
                result.Add(word);
            }
            return result;
        }

        private static bool IsNumber(string word)
        {
            foreach (char c in word)
            {
                if (!Char.IsDigit(c))
                    return false;
            }
            return true;
        }

        //1 palavra: precisa bater; 2 a 3: pelo menos 2; 4 ou mais: metade (mínimo 2)
        private static int WordsNeeded(int count)
        {
            if (count <= 1)
                return count;
            if (count <= 3)
                return 2;
            return Math.Max(2, (count + 1) / 2);
        }

        private static void Shuffle<T>(List<T> list)
        {
            lock (_random)
            {
                for (int i = list.Count - 1; i > 0; i--)
                {
                    int j = _random.Next(i + 1);
                    T temp = list[i];
                    list[i] = list[j];
                    list[j] = temp;
                }
            }
        }

        //Até 50 ids cujo título combina com o do jogador; mesmo nicho primeiro, depois mais palavras batendo
        public static List<string> Match(string title, VideoCategory? niche, HashSet<string> exclude = null)
        {
            var ids = new List<string>();
            var data = _snapshot;
            var words = Normalize(title);
            if (data == null || words.Count == 0)
                return ids;

            int needed = WordsNeeded(words.Count);
            string nicheName = niche.HasValue ? niche.Value.ToString() : null;
            var found = new List<KeyValuePair<int, Entry>>();
            foreach (var entry in data.Videos)
            {
                if (entry.Words == null || (exclude != null && exclude.Contains(entry.Id)))
                    continue;
                int hits = 0;
                foreach (var word in words)
                {
                    if (entry.Words.Contains(word))
                        hits++;
                }
                if (hits >= needed)
                    found.Add(new KeyValuePair<int, Entry>(hits, entry));
            }

            //Embaralha antes: a ordenação estável abaixo mantém a prioridade e deixa cada grupo igual em ordem aleatória
            Shuffle(found);

            //Ordenação estável: mesmo nicho, depois mais palavras
            var indexed = new List<KeyValuePair<int, KeyValuePair<int, Entry>>>();
            for (int i = 0; i < found.Count; i++)
                indexed.Add(new KeyValuePair<int, KeyValuePair<int, Entry>>(i, found[i]));
            indexed.Sort((l, r) =>
            {
                int ln = l.Value.Value.Niche == nicheName ? 1 : 0;
                int rn = r.Value.Value.Niche == nicheName ? 1 : 0;
                if (ln != rn)
                    return rn.CompareTo(ln);
                if (l.Value.Key != r.Value.Key)
                    return r.Value.Key.CompareTo(l.Value.Key);
                return l.Key.CompareTo(r.Key);
            });

            foreach (var item in indexed)
            {
                if (ids.Count >= MaxResults)
                    break;
                ids.Add(item.Value.Value.Id);
            }
            return ids;
        }

        //Meio-acerto: até 50 ids do nicho escolhido que batem pelo menos 1 palavra do título (mais palavras primeiro)
        public static List<string> MatchPartial(string title, VideoCategory niche, HashSet<string> exclude = null)
        {
            var ids = new List<string>();
            var data = _snapshot;
            var words = Normalize(title);
            if (data == null || words.Count == 0)
                return ids;

            string nicheName = niche.ToString();
            var found = new List<KeyValuePair<int, Entry>>();
            foreach (var entry in data.Videos)
            {
                if (entry.Words == null || entry.Niche != nicheName || (exclude != null && exclude.Contains(entry.Id)))
                    continue;
                int hits = 0;
                foreach (var word in words)
                {
                    if (entry.Words.Contains(word))
                        hits++;
                }
                if (hits >= 1)
                    found.Add(new KeyValuePair<int, Entry>(hits, entry));
            }

            //Embaralha antes: a ordenação estável abaixo mantém a prioridade e deixa cada grupo igual em ordem aleatória
            Shuffle(found);

            //Ordenação estável: mais palavras primeiro
            var indexed = new List<KeyValuePair<int, KeyValuePair<int, Entry>>>();
            for (int i = 0; i < found.Count; i++)
                indexed.Add(new KeyValuePair<int, KeyValuePair<int, Entry>>(i, found[i]));
            indexed.Sort((l, r) =>
            {
                if (l.Value.Key != r.Value.Key)
                    return r.Value.Key.CompareTo(l.Value.Key);
                return l.Key.CompareTo(r.Key);
            });

            foreach (var item in indexed)
            {
                if (ids.Count >= MaxResults)
                    break;
                ids.Add(item.Value.Value.Id);
            }
            return ids;
        }

        //Vídeos aleatórios do nicho (usado quando a cota da busca acabou)
        public static List<string> RandomOfNiche(VideoCategory niche, int count, HashSet<string> exclude = null)
        {
            var ids = new List<string>();
            var data = _snapshot;
            if (data == null)
                return ids;

            string nicheName = niche.ToString();
            foreach (var entry in data.Videos)
            {
                if (entry.Niche == nicheName && (exclude == null || !exclude.Contains(entry.Id)))
                    ids.Add(entry.Id);
            }

            //Embaralha (Fisher-Yates) e corta
            lock (_random)
            {
                for (int i = ids.Count - 1; i > 0; i--)
                {
                    int j = _random.Next(i + 1);
                    string temp = ids[i];
                    ids[i] = ids[j];
                    ids[j] = temp;
                }
            }
            if (ids.Count > count)
                ids.RemoveRange(count, ids.Count - count);
            return ids;
        }

        #endregion
    }
}
