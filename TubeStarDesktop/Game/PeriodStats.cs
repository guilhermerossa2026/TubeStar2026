using System;
using System.Collections.Generic;

namespace TubeStar
{
    //Totais de um período (dia, semana ou ano) para os resumos do Calendário
    public class PeriodStats
    {
        //Título do resumo (ex.: "Resumo da semana 2 · Ano 1"), usado no histórico do Calendário
        public string Title { get; set; }
        public double Earned { get; set; }
        public double Spent { get; set; }
        public long Views { get; set; }
        public long NewSubscribers { get; set; }
        public int VideosPosted { get; set; }
        public int TasksCompleted { get; set; }
        public List<ChannelPeriodStats> Channels { get; set; }

        public PeriodStats()
        {
            Channels = new List<ChannelPeriodStats>();
        }

        [Newtonsoft.Json.JsonIgnore]
        public double Balance
        {
            get { return Earned - Spent; }
        }

        public ChannelPeriodStats GetChannel(string name)
        {
            foreach (var channel in Channels)
            {
                if (channel.Name == name)
                    return channel;
            }
            var created = new ChannelPeriodStats { Name = name };
            Channels.Add(created);
            return created;
        }

        //Soma outro período neste (views de cada vídeo somadas pelo nome)
        public void Add(PeriodStats other)
        {
            Earned += other.Earned;
            Spent += other.Spent;
            Views += other.Views;
            NewSubscribers += other.NewSubscribers;
            VideosPosted += other.VideosPosted;
            TasksCompleted += other.TasksCompleted;

            foreach (var otherChannel in other.Channels)
            {
                var channel = GetChannel(otherChannel.Name);
                channel.Views += otherChannel.Views;
                channel.NewSubscribers += otherChannel.NewSubscribers;
                foreach (var video in otherChannel.Videos)
                    channel.AddVideoViews(video.Name, video.Views);
            }
        }
    }

    public class ChannelPeriodStats
    {
        public string Name { get; set; }
        public long Views { get; set; }
        public long NewSubscribers { get; set; }
        //Views de cada vídeo no período (para achar o melhor vídeo)
        public List<VideoPeriodStats> Videos { get; set; }

        public ChannelPeriodStats()
        {
            Videos = new List<VideoPeriodStats>();
        }

        public void AddVideoViews(string name, long views)
        {
            foreach (var video in Videos)
            {
                if (video.Name == name)
                {
                    video.Views += views;
                    return;
                }
            }
            Videos.Add(new VideoPeriodStats { Name = name, Views = views });
        }

        [Newtonsoft.Json.JsonIgnore]
        public VideoPeriodStats BestVideo
        {
            get
            {
                VideoPeriodStats best = null;
                foreach (var video in Videos)
                {
                    if (video.Views > 0 && (best == null || video.Views > best.Views))
                        best = video;
                }
                return best;
            }
        }
    }

    public class VideoPeriodStats
    {
        public string Name { get; set; }
        public long Views { get; set; }
    }
}
