using System;
using System.Collections.Generic;
using System.Linq;

namespace TubeStar
{
    /// <summary>
    /// Um canal do Top TubeStars: só números (rivais são offline, sem vídeos)
    /// </summary>
    public class RankChannel
    {
        public string Name { get; set; }
        public string Theme { get; set; }
        public bool IsFamous { get; set; }
        public bool IsPlayer { get; set; }
        //Canal do jogador ligado a esta linha (Channel.RankId)
        public string PlayerChannelId { get; set; }

        public long Subscribers { get; set; }
        public long Views { get; set; }
        public int VideoCount { get; set; }

        //Tendência diária de crescimento (ex.: 0,004 = 0,4%) e quanto ela oscila
        public double Trend { get; set; }
        public double Volatility { get; set; }

        //0 = ainda sem posição
        public int Position { get; set; }
        public int PreviousPosition { get; set; }
        public int WeekStartPosition { get; set; }
        public long WeekStartSubscribers { get; set; }
        public long WeekStartViews { get; set; }
        public int BestPosition { get; set; }
        public int DaysAtTop { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public int Change
        {
            get { return PreviousPosition == 0 || Position == 0 ? 0 : PreviousPosition - Position; }
        }

        [Newtonsoft.Json.JsonIgnore]
        public int WeekChange
        {
            get { return WeekStartPosition == 0 || Position == 0 ? 0 : WeekStartPosition - Position; }
        }

        [Newtonsoft.Json.JsonIgnore]
        public long WeekGrowth
        {
            get { return Subscribers - WeekStartSubscribers; }
        }

        [Newtonsoft.Json.JsonIgnore]
        public long WeekViews
        {
            get { return Views - WeekStartViews; }
        }
    }

    /// <summary>
    /// Ranking vivo: 15 rivais famosos + canais genéricos + canais do jogador, 100% offline
    /// </summary>
    public class Ranking
    {
        private const int GenericChannelCount = 500;

        public List<RankChannel> Channels { get; set; }

        public Ranking()
        {
            Channels = new List<RankChannel>();
        }

        //Ranking do jogo atual (fica no Player, então vai junto no save)
        public static Ranking Current
        {
            get
            {
                if (Player.Current.Ranking == null || Player.Current.Ranking.Channels.Count == 0)
                {
                    Player.Current.Ranking = Create();
                    Player.Current.Ranking.Refresh(false);
                }
                return Player.Current.Ranking;
            }
        }

        private static Ranking Create()
        {
            var ranking = new Ranking();
            var usedNames = new HashSet<string>();

            //Famosos: os 15 rivais atuais, entre ~300 mil e ~15 mi de inscritos
            foreach (var rival in Rivals.Current.All)
            {
                double skill = (rival.ShootingSkill + rival.PostProductionSkill) / 200.0;
                long subs = (long)Math.Pow(10, 5.5 + 1.7 * Math.Min(1, skill) + RandomHelpers.RandomDouble() * 0.4);
                ranking.Channels.Add(NewChannel(rival.Name, rival.VideoKeyword1, subs, true));
                usedNames.Add(rival.Name);
            }

            //Genéricos: de ~100 a ~3 mi de inscritos (muitos pequenos, poucos grandes)
            while (ranking.Channels.Count < GenericChannelCount + Rivals.Current.All.Count)
            {
                string name = GenerateName();
                if (!usedNames.Add(name))
                    continue;
                long subs = (long)Math.Pow(10, 2 + Math.Pow(RandomHelpers.RandomDouble(), 1.6) * 4.5);
                ranking.Channels.Add(NewChannel(name, Themes[RandomHelpers.RandomInt(Themes.Length)].Translate(), subs, false));
            }
            return ranking;
        }

        private static RankChannel NewChannel(string name, string theme, long subscribers, bool famous)
        {
            var channel = new RankChannel
            {
                Name = name,
                Theme = theme,
                IsFamous = famous,
                Subscribers = subscribers,
                Views = (long)(subscribers * (40 + RandomHelpers.RandomDouble() * 120)),
                VideoCount = 20 + RandomHelpers.RandomInt(600),
                Trend = -0.001 + RandomHelpers.RandomDouble() * 0.008,
                Volatility = 0.002 + RandomHelpers.RandomDouble() * 0.01,
                WeekStartSubscribers = subscribers
            };
            channel.WeekStartViews = channel.Views;
            return channel;
        }

        //Fim do dia: rivais crescem (ou caem), canais do jogador são atualizados e as posições recalculadas
        public void NewDay(int day)
        {
            foreach (var channel in Channels.Where(c => !c.IsPlayer))
            {
                //Tendência muda devagar; de vez em quando um viral
                channel.Trend = Math.Max(-0.004, Math.Min(0.012, channel.Trend + RandomHelpers.Gaussian() * 0.0005));
                double rate = channel.Trend + RandomHelpers.Gaussian() * channel.Volatility;
                if (RandomHelpers.RandomInt(1000) < 3)
                    rate += 0.03 + RandomHelpers.RandomDouble() * 0.1;

                long growth = (long)(channel.Subscribers * rate) + RandomHelpers.RandomInt(0, 15);
                channel.Subscribers = Math.Max(0, channel.Subscribers + growth);
                channel.Views += (long)(channel.Subscribers * (0.01 + RandomHelpers.RandomDouble() * 0.05));
                if (RandomHelpers.RandomInt(100) < 40)
                    channel.VideoCount++;
            }

            Refresh(true);

            //Semana fechou: a próxima começa a contar daqui
            if (day > 0 && day % 7 == 0)
            {
                foreach (var channel in Channels)
                {
                    channel.WeekStartSubscribers = channel.Subscribers;
                    channel.WeekStartViews = channel.Views;
                    channel.WeekStartPosition = channel.Position;
                }
            }
        }

        //Sincroniza os canais do jogador e recalcula posições; newDay conta "dias no #1" e guarda a posição de ontem
        public void Refresh(bool newDay)
        {
            SyncPlayerChannels();

            var ordered = Channels.OrderByDescending(c => c.Subscribers).ToList();
            for (int i = 0; i < ordered.Count; i++)
            {
                var channel = ordered[i];
                int position = i + 1;
                if (newDay || channel.PreviousPosition == 0)
                    channel.PreviousPosition = channel.Position == 0 ? position : channel.Position;
                if (channel.WeekStartPosition == 0)
                    channel.WeekStartPosition = position;
                channel.Position = position;
                if (channel.BestPosition == 0 || position < channel.BestPosition)
                    channel.BestPosition = position;
                if (newDay && position == 1)
                    channel.DaysAtTop++;
            }
            Channels = ordered;
        }

        private void SyncPlayerChannels()
        {
            var playerChannels = Player.Current.Channels.Where(c => c != null && c != Channel.UnreleasedVideos).ToList();
            foreach (var channel in playerChannels)
            {
                if (String.IsNullOrEmpty(channel.RankId))
                    channel.RankId = Guid.NewGuid().ToString("N");
            }
            var ids = new HashSet<string>(playerChannels.Select(c => c.RankId));

            //Canal apagado sai do ranking (renomear mantém a linha e o histórico)
            Channels.RemoveAll(c => c.IsPlayer && !ids.Contains(c.PlayerChannelId));

            foreach (var channel in playerChannels)
            {
                var entry = Channels.FirstOrDefault(c => c.IsPlayer && c.PlayerChannelId == channel.RankId);
                if (entry == null)
                {
                    entry = new RankChannel { PlayerChannelId = channel.RankId, IsPlayer = true, WeekStartSubscribers = channel.Subscribers, WeekStartViews = channel.Videos.Sum(v => (long)v.Views) };
                    Channels.Add(entry);
                }
                entry.Name = channel.Name;
                entry.Subscribers = channel.Subscribers;
                entry.Views = channel.Videos.Sum(v => (long)v.Views);
                entry.VideoCount = channel.Videos.Count;
            }
        }

        //Partes dos nomes genéricos
        private static readonly string[] NameStarts = { "Canal", "Mundo", "Toca", "Casa", "Clube", "Planeta", "Universo", "Diário", "Turma", "Vida", "Estúdio", "Rolê", "Papo", "Zona", "Base" };
        private static readonly string[] NameEnds = { "Gamer", "Fit", "da Cozinha", "Tech", "do Humor", "Pet", "Viajante", "Retrô", "Nerd", "Geek", "do Rock", "da Moda", "Mistério", "do Futebol", "Criativo", "Maker", "Animado", "Curioso", "Raiz", "Zen" };
        private static readonly string[] Nicks = { "lucas", "bia", "duda", "gabi", "rafa", "nanda", "theo", "julia", "caio", "lari", "pedro", "mari", "enzo", "luiza", "davi", "sofia", "leo", "manu", "biel", "isa" };
        private static readonly string[] NickEnds = { "plays", "vlogs", "tv", "games", "oficial", "br", "show", "life", "cria", "react" };

        private static string GenerateName()
        {
            if (RandomHelpers.RandomBool())
            {
                string name = NameStarts[RandomHelpers.RandomInt(NameStarts.Length)] + " " + NameEnds[RandomHelpers.RandomInt(NameEnds.Length)];
                return RandomHelpers.RandomInt(4) == 0 ? name + " " + (RandomHelpers.RandomBool() ? "TV" : "BR") : name;
            }
            string nick = Nicks[RandomHelpers.RandomInt(Nicks.Length)] + (RandomHelpers.RandomBool() ? "_" : "") + NickEnds[RandomHelpers.RandomInt(NickEnds.Length)];
            return RandomHelpers.RandomInt(3) == 0 ? nick + RandomHelpers.RandomInt(10, 100) : nick;
        }

        private static readonly EnglishStrings[] Themes =
        {
            EnglishStrings.ThemeGames, EnglishStrings.ThemeVlog, EnglishStrings.ThemeTech, EnglishStrings.ThemeComedy, EnglishStrings.ThemeEducation,
            EnglishStrings.ThemeFinance, EnglishStrings.ThemeFitness, EnglishStrings.ThemeCooking, EnglishStrings.ThemeMusic, EnglishStrings.ThemeBeauty,
            EnglishStrings.ThemeSports, EnglishStrings.ThemeReacts, EnglishStrings.ThemeMystery, EnglishStrings.ThemeTravel, EnglishStrings.ThemeKids
        };
    }
}
