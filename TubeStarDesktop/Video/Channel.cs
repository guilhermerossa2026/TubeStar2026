using System;
using System.Collections.Generic;
using System.Linq;

namespace TubeStar
{
    public class Channel : UniqueObject
    {
        public string Name { get; set; }
        public bool IsRivalChannel { get; set; }
        //Nicho escolhido na criação (fixo); nulo em saves antigos
        public VideoCategory? Niche { get; set; }
        //Liga o canal à linha dele no Top TubeStars (não muda ao renomear)
        public string RankId { get; set; }
        public int MinimumSubsribers { get; set; }
        //Momento do canal (0,7 a 1,4; 1 = neutro): sobe com vídeos bons e virais, cai com fracos e com dias sem postar, e volta devagar para 1
        public double Momentum { get; set; }
        public int IdleUnsubsTaken { get; set; }
        //Fração de inscrito da perda por ficar parado, guardada até somar 1
        public double IdleUnsubsFraction { get; set; }

        //Monetização: 0 = bloqueado, 1 = em análise, 2 = aprovado (saves antigos começam bloqueados)
        public int MonetizationState { get; set; }
        //Dia (Player.Iterations) em que a análise termina; -1 = sem análise
        public int MonetizationReviewEndDay { get; set; }
        //Já avisou que o canal bateu a meta
        public bool MonetizationGoalAnnounced { get; set; }

        public const int MonetizationSubsGoal = 5000;
        public const long MonetizationViewsGoal = 160000;

        //Canais de rival não passam pela monetização
        public bool CanEarnAds()
        {
            return IsRivalChannel || MonetizationState >= 2;
        }

        public long TotalViews()
        {
            return Videos.Sum(v => (long)v.Views);
        }

        public bool MonetizationGoalReached()
        {
            return Subscribers >= MonetizationSubsGoal && TotalViews() >= MonetizationViewsGoal;
        }

        private AdvertisingStrategy _advertising;
        public AdvertisingStrategy Advertising
        {
            get { return _advertising; }
            set { _advertising = value; }
        }

        public HashSet<Video> Videos { get; set; }
        public int Subscribers { get; set; }

        public double Income { get; set; }

        public bool IsSuspended { get; set; }

        //Stats
        public List<double> IncomeOverTime { get; set; }
        public List<double> ExpensesOverTime { get; set; }
        public List<double> SubscribersOverTime { get; set; }

        public Channel()
            : base()
        {
            Momentum = 1;
            MonetizationReviewEndDay = -1;
            Videos = new HashSet<Video>();
            Advertising = AdvertisingStrategy.Normal;
            IncomeOverTime = new List<double>();
            ExpensesOverTime = new List<double>();
            SubscribersOverTime = new List<double>();
        }

        private static Channel _unreleasedVideos;
        public static Channel UnreleasedVideos
        {
            get
            {
                if (_unreleasedVideos == null)
                    _unreleasedVideos = new Channel() { Name = EnglishStrings.UnreleasedVideos.Translate() };
                return _unreleasedVideos;
            }
            set { _unreleasedVideos = value; }
        }
    }
}