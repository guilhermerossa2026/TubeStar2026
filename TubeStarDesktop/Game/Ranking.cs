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
        //Nicho do canal (filtro do ranking); nulo em rankings de saves antigos até a migração
        public VideoCategory? Niche { get; set; }
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
        //Teto de inscritos: o crescimento freia perto dele (0 = save antigo, ainda sem teto)
        public long Ceiling { get; set; }

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
                else
                {
                    if (Player.Current.Ranking.Channels.Any(c => !c.IsPlayer && !c.Niche.HasValue))
                        Player.Current.Ranking.MigrateNiches();
                    if (Player.Current.Ranking.Channels.Any(c => !c.IsPlayer && c.Ceiling == 0))
                        Player.Current.Ranking.MigrateCeilings();
                }
                return Player.Current.Ranking;
            }
        }

        private static Ranking Create()
        {
            var ranking = new Ranking();
            var usedNames = new HashSet<string>();

            //Famosos: os 15 rivais atuais, entre ~300 mil e ~15 mi de inscritos
            int famousIndex = 0;
            foreach (var rival in Rivals.Current.All)
            {
                double skill = (rival.ShootingSkill + rival.PostProductionSkill) / 200.0;
                long subs = (long)Math.Pow(10, 5.5 + 1.7 * Math.Min(1, skill) + RandomHelpers.RandomDouble() * 0.4);
                ranking.Channels.Add(NewChannel(rival.Name, FamousNiche(famousIndex++), subs, true));
                usedNames.Add(rival.Name);
            }

            //Genéricos: de ~100 a ~3 mi de inscritos (muitos pequenos, poucos grandes)
            while (ranking.Channels.Count < GenericChannelCount + Rivals.Current.All.Count)
            {
                string name = GenerateName();
                if (!usedNames.Add(name))
                    continue;
                long subs = (long)Math.Pow(10, 2 + Math.Pow(RandomHelpers.RandomDouble(), 1.6) * 4.5);
                ranking.Channels.Add(NewChannel(name, (VideoCategory)RandomHelpers.RandomInt(1, VideoCategoryHelpers.Count + 1), subs, false));
            }
            return ranking;
        }

        private static RankChannel NewChannel(string name, VideoCategory niche, long subscribers, bool famous)
        {
            var channel = new RankChannel
            {
                Name = name,
                Theme = niche.GetString(),
                Niche = niche,
                IsFamous = famous,
                Subscribers = subscribers,
                Views = (long)(subscribers * (40 + RandomHelpers.RandomDouble() * 120)),
                VideoCount = 20 + RandomHelpers.RandomInt(600),
                Trend = -0.001 + RandomHelpers.RandomDouble() * 0.008,
                Volatility = 0.002 + RandomHelpers.RandomDouble() * 0.01,
                Ceiling = RollCeiling(subscribers, famous),
                WeekStartSubscribers = subscribers
            };
            channel.WeekStartViews = channel.Views;
            return channel;
        }

        private const long FamousMaxCeiling = 150000000;
        private const long GenericMaxCeiling = 20000000;

        //Sorteia o teto (escala log): famosos 5 mi a 150 mi; genéricos de ~5x os inscritos até 20 mi (sempre >= 1,5x)
        private static long RollCeiling(long subscribers, bool famous)
        {
            double ceiling;
            if (famous)
                ceiling = Math.Pow(10, 6.7 + RandomHelpers.RandomDouble() * 1.48);
            else
            {
                double low = Math.Min(Math.Max(1, subscribers) * 5.0, GenericMaxCeiling);
                ceiling = low * Math.Pow(GenericMaxCeiling / low, RandomHelpers.RandomDouble());
            }
            ceiling = Math.Max(ceiling, subscribers * 1.5);
            return (long)Math.Min(ceiling, famous ? FamousMaxCeiling : GenericMaxCeiling);
        }

        //Fim do dia: rivais crescem (ou caem), canais do jogador são atualizados e as posições recalculadas
        public void NewDay(int day)
        {
            foreach (var channel in Channels.Where(c => !c.IsPlayer))
            {
                //Tendência muda devagar e volta para a média; de vez em quando um viral
                channel.Trend += (0.002 - channel.Trend) * 0.02 + RandomHelpers.Gaussian() * 0.0005;
                channel.Trend = Math.Max(-0.004, Math.Min(0.008, channel.Trend));
                double rate = channel.Trend + RandomHelpers.Gaussian() * channel.Volatility;
                if (RandomHelpers.RandomInt(1000) < 3)
                    rate += 0.03 + RandomHelpers.RandomDouble() * 0.1;

                //Crescimento logístico: freia perto do teto; acima dele o canal cai devagar
                double room = 1.0 - (double)channel.Subscribers / channel.Ceiling;
                if (room <= 0)
                    rate = -0.002 * Math.Min(5, 1 - room * 10);
                else if (rate > 0)
                    rate *= room;

                long growth = (long)(channel.Subscribers * rate) + (room > 0 ? RandomHelpers.RandomInt(0, 15) : 0);
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
                entry.Niche = channel.Niche;
                entry.Theme = channel.Niche.HasValue ? channel.Niche.Value.GetString() : null;
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

        //Nicho de cada rival famoso, na ordem de Rivals.Current.All (Rival1..Rival15), pelo tema dos vídeos dele
        private static readonly VideoCategory[] FamousNiches =
        {
            VideoCategory.Mystery,    //NightTerrors98 (Slenderman)
            VideoCategory.Games,      //xxxSUPAGAMERxxx (retro game)
            VideoCategory.Music,      //Black Crow Music (heavy metal)
            VideoCategory.Comedy,     //CATTY_KATE (gatos)
            VideoCategory.Mystery,    //TheTruthSeeker (aliens)
            VideoCategory.Education,  //CraftyHowTo's
            VideoCategory.Sports,     //<XTRMEFreak> (skate)
            VideoCategory.Vlog,       //The 'Good Deed' Man
            VideoCategory.Vlog,       //BehindMyCurtain
            VideoCategory.Comedy,     //Herpaderp
            VideoCategory.Technology, //TechTalk with Tech Terry
            VideoCategory.Reacts,     //MegaAnimeFan89
            VideoCategory.Reacts,     //ArmchairMovieCritic
            VideoCategory.Beauty,     //Fashion4Life
            VideoCategory.Comedy      //EpicFails4Win
        };

        private static VideoCategory FamousNiche(int index)
        {
            return index < FamousNiches.Length ? FamousNiches[index] : VideoCategory.Vlog;
        }

        //Rankings criados antes do teto: sorteia o teto e traz para baixo quem já passou dele (nunca mexe no jogador)
        private void MigrateCeilings()
        {
            foreach (var channel in Channels.Where(c => !c.IsPlayer && c.Ceiling == 0))
            {
                long max = channel.IsFamous ? FamousMaxCeiling : GenericMaxCeiling;
                //Passou do máximo: fica no máximo (o teto abaixo é calculado com os inscritos já limitados)
                long subs = Math.Min(channel.Subscribers, max);
                channel.Ceiling = RollCeiling(subs, channel.IsFamous);
                if (channel.Subscribers > channel.Ceiling)
                {
                    double ratio = channel.Subscribers > 0 ? (double)channel.Ceiling / channel.Subscribers : 1;
                    channel.Views = (long)(channel.Views * ratio);
                    channel.Subscribers = channel.Ceiling;
                    channel.WeekStartSubscribers = channel.Subscribers;
                    channel.WeekStartViews = channel.Views;
                }
                channel.Trend = Math.Max(-0.004, Math.Min(0.008, channel.Trend));
            }
        }

        //Rankings criados antes dos nichos: descobre o nicho pelo tema gravado (genéricos) ou pela ordem (famosos)
        private void MigrateNiches()
        {
            foreach (var channel in Channels.Where(c => c.IsFamous && !c.Niche.HasValue))
            {
                int index = Rivals.Current.All.FindIndex(r => r.Name == channel.Name);
                channel.Niche = FamousNiche(Math.Max(0, index));
                channel.Theme = channel.Niche.Value.GetString();
            }
            foreach (var channel in Channels.Where(c => !c.IsFamous && !c.IsPlayer && !c.Niche.HasValue))
            {
                foreach (VideoCategory niche in Enum.GetValues(typeof(VideoCategory)))
                {
                    if (niche.GetString() == channel.Theme)
                        channel.Niche = niche;
                }
                //Tema não reconhecido (ex.: idioma trocado): sorteia, para a migração não repetir
                if (!channel.Niche.HasValue)
                    channel.Niche = (VideoCategory)RandomHelpers.RandomInt(1, VideoCategoryHelpers.Count + 1);
                channel.Theme = channel.Niche.Value.GetString();
            }
        }
    }
}
