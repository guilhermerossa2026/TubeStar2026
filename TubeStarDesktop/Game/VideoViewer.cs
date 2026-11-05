using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TubeStar
{
    public class SerializableShareData
    {
        public Guid Id { get; set; }
        public int Count { get; set; }
    }

    public static class VideoViewer
    {
        private static ConcurrentDictionary<Guid, int> _videoShares;
        private static ConcurrentDictionary<Guid, int> _videoBoughtViews;

        public static List<SerializableShareData> GetShares()
        {
            return ToSaveData(_videoShares);
        }

        //Só vai para o save o que ainda existe e ainda tem algo a entregar
        private static List<SerializableShareData> ToSaveData(ConcurrentDictionary<Guid, int> source)
        {
            var liveIds = GetLiveVideoIds();
            List<SerializableShareData> data = new List<SerializableShareData>();
            foreach (var item in source)
            {
                if (item.Value > 0 && liveIds.Contains(item.Key))
                    data.Add(new SerializableShareData() { Id = item.Key, Count = item.Value });
            }
            return data;
        }

        private static HashSet<Guid> GetLiveVideoIds()
        {
            var ids = new HashSet<Guid>();
            if (Player.Current != null)
            {
                foreach (var video in Player.Current.Videos)
                    ids.Add(video.Id);
                foreach (var channel in Player.Current.Channels)
                    foreach (var video in channel.Videos)
                        ids.Add(video.Id);
            }
            if (Rivals.Current != null)
            {
                foreach (var rival in Rivals.Current.All)
                    if (rival.Channel != null && rival.Channel.Videos != null)
                        foreach (var video in rival.Channel.Videos)
                            ids.Add(video.Id);
            }
            return ids;
        }

        public static void SetShares(List<SerializableShareData> data)
        {
            _videoShares.Clear();
            foreach (var item in data)
            {
                _videoShares[item.Id] = item.Count;
            }
        }

        public static List<SerializableShareData> GetBoughtViews()
        {
            return ToSaveData(_videoBoughtViews);
        }

        public static void SetBoughtViews(List<SerializableShareData> data)
        {
            _videoBoughtViews.Clear();
            foreach (var item in data)
            {
                _videoBoughtViews[item.Id] = item.Count;
            }
        }

        static VideoViewer()
        {
            Reset();
        }

        public static void Reset()
        {
            _videoShares = new ConcurrentDictionary<Guid, int>();
            _videoBoughtViews = new ConcurrentDictionary<Guid, int>();
        }

        //Tira um vídeo apagado dos dicionários (não entram no save)
        public static void Forget(Video video)
        {
            int ignored;
            _videoShares.TryRemove(video.Id, out ignored);
            _videoBoughtViews.TryRemove(video.Id, out ignored);
        }

        public static void BuyViews(Video video, int number)
        {
            if (!_videoBoughtViews.ContainsKey(video.Id))
            {
                _videoBoughtViews[video.Id] = 0;
            }

            _videoBoughtViews[video.Id] += number;
        }

        public static void FreeViews(Video video, int number)
        {
            Initialize(video);
            _videoShares[video.Id] += number;
        }

        public static void SubscriberView(Channel channel, Video video)
        {
            Initialize(video);
            //Fora do nicho do canal, menos inscritos assistem
            double nicheFactor = video.IsInNiche(channel) ? 1.0 : 0.75;
            //Alcance depende da nota; momento do canal e sorte do vídeo multiplicam
            double reach = VideoLuck.ReachPercent(video.GenerateQuality() / 10.0) * channel.Momentum * video.Luck;
            _videoShares[video.Id] += (int)(channel.Subscribers * (Player.Current.ChallengeMode ? reach / 2 : reach) * nicheFactor);
        }

        public static void Iteration(Channel channel, Video video, ref double dailyIncome, ref double dailyExpenses)
        {
            Initialize(video);
            video.Iterations++;

            if (channel.IsSuspended || video.IsSuspended)
            {
                _videoShares[video.Id] = 0;
                _videoBoughtViews[video.Id] = 0;
                video.PromotionViewsLeft = 0;
                video.PromotionDaysLeft = 0;
                return;
            }

            int tempShares = (int)(_videoShares[video.Id] * (Player.Current.ChallengeMode ? 0.1 : 0.2));
            _videoShares[video.Id] -= tempShares;

            int payedViews = _videoBoughtViews.ContainsKey(video.Id) ? _videoBoughtViews[video.Id] : 0;
            _videoBoughtViews[video.Id] = 0;
            //Campanha "Promover vídeo": parte das views entra a cada dia (sem renda de anúncio, como as compradas)
            payedViews += Promotion.TakeTodayViews(video);

            double searchEngineViews = video.Views / (50 * video.Iterations);
            //Empurrão: nos primeiros dias canais pequenos têm um mínimo de views de busca
            if (video.Iterations <= VideoLuck.SearchFloorDays && !channel.IsRivalChannel)
                searchEngineViews = Math.Max(searchEngineViews, VideoLuck.SearchFloor(channel.Subscribers));
            var qualityViews = Math.Max(0, ((video.Quality.Value - Math.Pow(video.Iterations, Player.Current.ChallengeMode ? 2 : 1)) * video.Quality.Value) / Math.Max((Player.Current.ChallengeMode ? 2 : 1), (100 - video.Quality.Value) / 2));
            //Concorrência do nicho: nicho lotado recebe menos views de busca e de qualidade
            //Tipo que combina com o nicho: +10% nessas views
            var iterationViews = (searchEngineViews + qualityViews) * video.Luck * video.Category.GetCompetitionMultiplier() * video.GetTypeMultiplier() + tempShares;

            //Total de views orgânicas: mesmo laço antigo de pulos de 4 ou 5, só somando (sem ver view por view)
            int organicViews = 0;
            while (organicViews <= iterationViews)
            {
                organicViews += RandomHelpers.RandomInt(4, 6);
            }

            //Simula em lotes (no máximo ~50 chamadas por vídeo/dia) em vez de view por view
            ViewInBatches(channel, video, ref dailyIncome, ref dailyExpenses, organicViews, false);
            ViewInBatches(channel, video, ref dailyIncome, ref dailyExpenses, payedViews, true);

            if (channel.Subscribers < channel.MinimumSubsribers)
                channel.Subscribers = channel.MinimumSubsribers;
        }

        private const int MaxBatches = 50;

        private static void ViewInBatches(Channel channel, Video video, ref double income, ref double expenses, int totalViews, bool payedView)
        {
            if (totalViews <= 0) return;
            int batchSize = Math.Max(1, (int)Math.Ceiling(totalViews / (double)MaxBatches));
            int remaining = totalViews;
            while (remaining > 0)
            {
                //O último lote leva o resto exato
                int batch = Math.Min(batchSize, remaining);
                ViewVideo(channel, video, ref income, ref expenses, batch, payedView);
                remaining -= batch;
            }
        }

        //Probabilidade real de RandomHelpers.Chance(p): Next(100) <= p, ou seja (p+1)/100
        private static double ChanceProb(int percentage)
        {
            percentage = Math.Max(0, Math.Min(100, percentage));
            return Math.Min(1.0, (percentage + 1) / 100.0);
        }

        //Probabilidade por view de CategoryHelpers.CheckInterest(video), com q = 1/15 (nichos):
        //p = q + (1-q)*q*0,5 + (1-q)^2*0,06; com GenreBuster: p + (1-p)*0,36
        private static double InterestProb(Video video)
        {
            double q = 1.0 / VideoCategoryHelpers.Count;
            double p = q + (1 - q) * q * 0.5 + (1 - q) * (1 - q) * ChanceProb(5);
            if (video.Attributes.Contains(VideoAttributes.GenreBuster))
                p = p + (1 - p) * ChanceProb(35);
            return p;
        }

        //Um lote de numViews equivale a numViews views independentes: sorteia quantas seguem cada ramo
        private static void ViewVideo(Channel channel, Video video, ref double income, ref double expenses, int numViews, bool payedView = false)
        {
            video.Views += numViews;
            income += ViewIncome(channel, video, payedView, numViews);
            //Custo por view só vale para as views compradas
            if (payedView)
                expenses += video.CostPerView * numViews;

            //Views extras do SecondTime são bônus, sem custo (antes: Chance(5) e RandomInt(2) por view)
            if (video.Attributes.Contains(VideoAttributes.SecondTime))
            {
                int tempViews = RandomHelpers.Binomial(RandomHelpers.Binomial(numViews, ChanceProb(5)), 0.5);
                if (tempViews > 0)
                {
                    video.Views += tempViews;
                    income += ViewIncome(channel, video, payedView, tempViews);
                }
            }

            double pInterest = InterestProb(video);
            int interested = RandomHelpers.Binomial(numViews, pInterest);
            int likes = RandomHelpers.Binomial(interested, ChanceProb(video.Quality.Value));
            int dislikes = RandomHelpers.Binomial(interested - likes, 0.5);

            if (likes > 0)
            {
                video.Likes += likes;

                int subs = RandomHelpers.Binomial(likes, SubscriptionProb(channel, video));
                if (video.Attributes.Contains(VideoAttributes.Crowdfunding))
                {
                    if (!payedView)
                        Player.Current.Money += subs * VideoAttributes.Crowdfunding.Money;
                }
                else
                {
                    channel.Subscribers += subs;
                }

                ShareVideo(video, likes, pInterest);
            }

            if (dislikes > 0)
            {
                video.Dislikes += dislikes;

                //Share video if "So bad" attribute on video
                if (video.Attributes.Contains(VideoAttributes.SoBad))
                {
                    ShareVideo(video, dislikes, pInterest);
                }
            }

            //Chance of unsubscribe because of dislike or disinterest in category
            CheckUnsubscriptions(channel, video, dislikes + (numViews - interested), payedView);
        }

        //numChecks = quantas views passariam pela checagem de desinscrição
        private static void CheckUnsubscriptions(Channel channel, Video video, int numChecks, bool payedView)
        {
            if (numChecks <= 0) return;
            if (!channel.IsRivalChannel && !payedView)
            {
                int chance = (int)Math.Min(100.0, channel.Subscribers * 100.0 / Math.Max(1, video.Views));
                if (!Player.Current.ChallengeMode) chance = chance / 2;
                double p = ChanceProb(25) * UnsubscriptionProb(channel) * ChanceProb(chance);
                int unsubs = RandomHelpers.Binomial(numChecks, p);
                if (unsubs > 0)
                    channel.Subscribers = Math.Max(0, channel.Subscribers - unsubs);
            }
        }

        //numViews = quantas views tentam compartilhar
        private static void ShareVideo(Video video, int numViews, double pInterest)
        {
            //Like enough to share: RandomInt(100+memetic) >= 60
            int memeticMutator = video.Attributes.Contains(VideoAttributes.Memetic) ? 20 : 0;
            double pShare = (40.0 + memeticMutator) / (100.0 + memeticMutator);
            int sharers = RandomHelpers.Binomial(numViews, pShare);
            if (sharers <= 0) return;

            //Share: cada uma chama RandomInt(10) amigos (média 4,5, variância 8,25)
            int friends = 0;
            if (sharers <= 30)
            {
                for (int i = 0; i < sharers; i++)
                    friends += RandomHelpers.RandomInt(10);
            }
            else
            {
                double value = 4.5 * sharers + Math.Sqrt(8.25 * sharers) * RandomHelpers.Gaussian();
                friends = (int)Math.Max(0, Math.Min(9.0 * sharers, Math.Round(value)));
            }
            int gained = RandomHelpers.Binomial(friends, pInterest);

            //Owner of site - share video on site (Chance(1)&&Chance(10) = 2% * 11% = 0,22%)
            int owners = RandomHelpers.Binomial(sharers, ChanceProb(1) * ChanceProb(10));
            for (int o = 0; o < owners; o++)
            {
                var subscribers = RandomHelpers.RandomInt((int)(25000 * (Player.Current.ChallengeMode ? 0.02 : 0.05)));
                gained += (int)(subscribers * 0.25);
            }

            //Sorte nos compartilhamentos, com trava: no máximo x3 e, acima de 1, até numViews/2 por lote
            int luckyGain = (int)(gained * Math.Min(video.Luck, 3.0));
            if (video.Luck > 1)
                luckyGain = Math.Min(luckyGain, Math.Max(gained, numViews / 2));
            _videoShares[video.Id] += luckyGain;
        }

        //Estimativa de inscritos vindos das views promovidas: interessados x curtiram (nota) x se inscrevem
        public static int EstimateSubscribers(Channel channel, Video video, int views)
        {
            if (views <= 0 || channel == null)
                return 0;
            double pLike = ChanceProb(video.GenerateQuality());
            return (int)Math.Round(views * InterestProb(video) * pLike * SubscriptionProb(channel, video));
        }

        private static double SubscriptionProb(Channel channel, Video video)
        {
            int initialChance = 0;
            switch (channel.Advertising)
            {
                case (AdvertisingStrategy.Low):
                    initialChance = 90;
                    break;
                case (AdvertisingStrategy.Normal):
                    initialChance = 70;
                    break;
                case (AdvertisingStrategy.High):
                    initialChance = 50;
                    break;
                case (AdvertisingStrategy.Aggressive):
                    initialChance = 30;
                    break;
                default: throw new NotSupportedException();
            }

            if (video.Attributes.Contains(VideoAttributes.Hypnotic))
                initialChance += 30;

            //Fora do nicho atrai público novo: mais chance de quem curtiu se inscrever
            double p = ChanceProb(initialChance);
            if (!video.IsInNiche(channel))
                p = Math.Min(1.0, p * 1.3);
            return p;
        }

        private static double UnsubscriptionProb(Channel channel)
        {
            switch (channel.Advertising)
            {
                case (AdvertisingStrategy.Low): return ChanceProb(10);
                case (AdvertisingStrategy.Normal): return ChanceProb(20);
                case (AdvertisingStrategy.High): return ChanceProb(40);
                case (AdvertisingStrategy.Aggressive): return ChanceProb(60);
                default: throw new NotSupportedException();
            }
        }

        private static double ViewIncome(Channel channel, Video video, bool payedView, int numViews)
        {
            //Don't get base view payment for payed views
            var income = payedView ? 0 : channel.IncomePerView;
            if (Player.Current.ChallengeMode)
                income = income / 2;

            var extraIncome = Math.Sqrt(Math.Max(0, channel.Subscribers) /(Player.Current.ChallengeMode ? 20000.0 : 1000.0)) * 0.01;
            extraIncome = Math.Min(extraIncome, Player.Current.ChallengeMode ? 0.25 : 0.50);
            income += extraIncome;

            //RPM do nicho: cada tema paga diferente por view
            income *= video.Category.GetRpmMultiplier();

            //CTR: no ChallengeMode, Chance(90) && Chance(90 ultra / 50 normal) zera a renda daquela view
            int paidViews = numViews;
            if (Player.Current.ChallengeMode)
            {
                double pZero = ChanceProb(90) * (Player.Current.UltraMode ? ChanceProb(90) : ChanceProb(50));
                paidViews = RandomHelpers.Binomial(numViews, 1 - pZero);
            }

            double total = income * paidViews;

            if (video.Attributes.Contains(VideoAttributes.ProductPlacement))
            {
                total += 0.01 * numViews; //Extra 1c
            }

            return total;
        }

        private static void Initialize(Video video)
        {
            if (!_videoShares.ContainsKey(video.Id))
            {
                _videoShares[video.Id] = 0;
            }
        }
    }
}
