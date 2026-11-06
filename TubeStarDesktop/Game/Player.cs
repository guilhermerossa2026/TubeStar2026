using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TubeStar
{
    public class Player
    {
        private static Player _current;
        public static Player Current
        {
            get
            {
                if (_current == null)
                    _current = new Player();
                return _current;
            }
            set { _current = value; }
        }

        public event Action MoneyChanged;

        private double _money;
        public double Money 
        {
            get { return _money; }
            set
            {
                _money = value;
                if (MoneyChanged != null)
                {
                    MoneyChanged();
                }
            }
        }

        public bool QuitJob { get; set; }
        public bool HasPromotion { get; set; }
        public bool Overtime { get; set; }
        //Meio período: 4h e $50 por dia (integral: 9h e $100)
        public bool PartTime { get; set; }
        //Dia (Iterations) da última troca de turno; só pode trocar de novo após 7 dias
        public int LastShiftChangeDay { get; set; }

        public List<Channel> Channels { get; set; }
        public List<Task> TasksInProgress { get; set; }
        public List<Video> Videos { get; set; }

        public int ShootingSkill { get; set; }
        public int PostProductionSkill { get; set; }
        public int VideoAttributePoints { get; set; }
        public bool CanViewQualityBeforeUpload { get; set; }

        public int Iterations { get; set; }

        public int CostOfLivingExtra { get; set; }

        public bool ChallengeMode { get; set; }
        public bool UltraMode { get; set; }
        [Obsolete("Dinamica dos robos removida; mantido so para ler saves antigos.")]
        public bool RobotRulers { get; set; }

        public double LoanPayOff { get; set; }

        //Imposto semanal: total já pago (para uma tela fiscal) e última semana cobrada (iteração/7; -1 = nenhuma)
        public double TotalTaxPaid { get; set; }
        public int LastTaxWeekIndex { get; set; }
        //Imposto que o dinheiro não cobriu; entra na cobrança da semana seguinte
        public double TaxDebt { get; set; }

        //Calendário: resumo do último dia e totais da semana e do ano em andamento
        public PeriodStats LastDay { get; set; }
        public PeriodStats CurrentWeek { get; set; }
        public PeriodStats CurrentYear { get; set; }
        //Semanas e anos já fechados (histórico do Calendário)
        public List<PeriodStats> PastSummaries { get; set; }
        //Top TubeStars (rivais só com números)
        public Ranking Ranking { get; set; }
        //Séries removidas da lista "Continuar série" (chave da série -> último número quando foi removida)
        [XmlIgnore] //Dictionary não é aceito pelo leitor de saves XML antigos
        public Dictionary<string, int> HiddenSeries { get; set; }

        //Guarda o dia simulado nos totais da semana e do ano
        public void RecordDay(PeriodStats day)
        {
            LastDay = day;
            if (CurrentWeek == null)
                CurrentWeek = new PeriodStats();
            if (CurrentYear == null)
                CurrentYear = new PeriodStats();
            CurrentWeek.Add(day);
            CurrentYear.Add(day);
        }

        //Evento aleatório roda depois do dia gravado: soma o efeito dele no dia, semana e ano
        public void RecordEvent(double money, long subscribers)
        {
            if (LastDay == null || (money == 0 && subscribers == 0))
                return;

            var extra = new PeriodStats();
            if (money > 0)
            {
                extra.Earned = money;
                extra.OtherEarned = money;
            }
            else
                extra.Spent = -money;
            extra.NewSubscribers = subscribers;

            LastDay.Add(extra);

            //Se a semana (ou o ano) fechou hoje, o evento pertence ao período fechado
            bool weekClosed = Iterations > 0 && Iterations % 7 == 0 && PastSummaries != null && PastSummaries.Count > 0;
            bool yearClosed = weekClosed && Iterations % 112 == 0 && PastSummaries.Count > 1;
            if (weekClosed)
            {
                PastSummaries[PastSummaries.Count - 1].Add(extra);
                if (yearClosed)
                    PastSummaries[PastSummaries.Count - 2].Add(extra);
            }
            else
            {
                if (CurrentWeek == null)
                    CurrentWeek = new PeriodStats();
                CurrentWeek.Add(extra);
            }

            if (!yearClosed)
            {
                if (CurrentYear == null)
                    CurrentYear = new PeriodStats();
                CurrentYear.Add(extra);
            }
        }

        public const string SpendStore = "store";
        public const string SpendStudy = "study";
        public const string SpendOther = "other";

        //Gasto fora do ciclo do dia (loja, estudos, advogado): soma no dia, semana e ano
        public void RecordSpending(double value, string category)
        {
            if (value <= 0)
                return;

            var extra = new PeriodStats();
            extra.Spent = value;
            if (category == SpendStore)
                extra.StoreCost = value;
            else if (category == SpendStudy)
                extra.StudyCost = value;
            else
                extra.OtherCost = value;

            //Não soma em LastDay: o "Resumo de ontem" não muda
            if (CurrentWeek == null)
                CurrentWeek = new PeriodStats();
            CurrentWeek.Add(extra);
            if (CurrentYear == null)
                CurrentYear = new PeriodStats();
            CurrentYear.Add(extra);
        }

        //Empréstimo recebido: registrado à parte, não conta como entrada
        public void RecordLoanReceived(double value)
        {
            if (value <= 0)
                return;
            if (CurrentWeek == null)
                CurrentWeek = new PeriodStats();
            CurrentWeek.LoanReceived += value;
            if (CurrentYear == null)
                CurrentYear = new PeriodStats();
            CurrentYear.LoanReceived += value;
        }

        //Saldo devedor do empréstimo (0 se não há empréstimo ativo)
        public double GetLoanBalance()
        {
            if (StoreItems.Current.Loan.Purchased)
                return Math.Max(0, LoanPayOff);
            return 0;
        }

        //Patrimônio = dinheiro - imposto atrasado - saldo devedor do empréstimo
        public double GetNetWorth()
        {
            return Money - TaxDebt - GetLoanBalance();
        }

        public Player()
        {
            Reset();
        }

        public void Reset()
        {
            Iterations = -1;
            Money = 950;
            QuitJob = false;
            ShootingSkill = 30;
            PostProductionSkill = 20;
            VideoAttributePoints = 2;
            CanViewQualityBeforeUpload = false;
            CostOfLivingExtra = 0;
            Overtime = false;
            HasPromotion = false;
            PartTime = false;
            LastShiftChangeDay = -999;
            LoanPayOff = 0;
            TotalTaxPaid = 0;
            LastTaxWeekIndex = -1;
            TaxDebt = 0;
            LastDay = null;
            CurrentWeek = null;
            CurrentYear = null;
            PastSummaries = new List<PeriodStats>();
            Ranking = null;
            HiddenSeries = null;

            TasksInProgress = new List<Task>();
            Videos = new List<Video>();
            Channel.UnreleasedVideos = null; //Novo jogo não herda os vídeos não lançados do anterior
            Channel.UnreleasedVideos.Name = EnglishStrings.UnreleasedVideos.Translate();
            Channels = new List<Channel>() { Channel.UnreleasedVideos }; //Default channel

            Studies.Current = null;
            StoreItems.Current = null;
            Rivals.Current = null;
        }
    }
}