using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TubeStar
{
    [XmlInclude(typeof(Rival1))]
    [XmlInclude(typeof(Rival2))]
    [XmlInclude(typeof(Rival3))]
    [XmlInclude(typeof(Rival4))]
    [XmlInclude(typeof(Rival5))]
    [XmlInclude(typeof(Rival6))]
    [XmlInclude(typeof(Rival7))]
    [XmlInclude(typeof(Rival8))]
    [XmlInclude(typeof(Rival9))]
    [XmlInclude(typeof(Rival10))]
    [XmlInclude(typeof(Rival11))]
    [XmlInclude(typeof(Rival12))]
    [XmlInclude(typeof(Rival13))]
    [XmlInclude(typeof(Rival14))]
    [XmlInclude(typeof(Rival15))]
    public class Rival
    {
        private Dictionary<string, List<YouTubeSearchEntry>> _cacheVideos;

        public string Name { get; set; }

        public int ShootingSkill { get; set; }
        public int PostProductionSkill { get; set; }
        public int InitialSubscribers { get; set; }

        public string VideoKeyword1 { get; set; }
        public string VideoKeyword2 { get; set; }
        public string VideoKeyword3 { get; set; }
        public string VideoKeyword4 { get; set; }

        public Channel Channel { get; set; }
        public List<string> UsedVideos { get; set; }

        public Rival()
        {
            _cacheVideos = new Dictionary<string, List<YouTubeSearchEntry>>();
            UsedVideos = new List<string>();
        }

        public void CreateChannel()
        {
            Channel = new Channel();
            Channel.Name = Name;
            Channel.IsRivalChannel = true;
            Channel.Advertising = AdvertisingStrategy.High;
            Channel.Subscribers = InitialSubscribers;

            AddNewVideo();
        }

        public void AddNewVideo()
        {
            try
            {
                string word;
                var rand = RandomHelpers.RandomInt(100);
                if (rand < 25)
                    word = VideoKeyword1;
                else if (rand < 50)
                    word = VideoKeyword2;
                else if (rand < 75)
                    word = VideoKeyword3;
                else
                    word = VideoKeyword4;

                string[] titleTemplates = new string[]
                {
                    "{0} - O Novo Episodio Incrivel!",
                    "Como dominar {0} facilmente",
                    "Top 10 Melhores Momentos de {0}",
                    "{0} Atualizado: Vale a pena conferir?",
                    "Tentando sobreviver em {0} por 24 horas",
                    "Minha reacao sincera sobre {0}",
                    "O Segredo mais bizarro de {0} revelado",
                    "{0}: O Guia definitivo para iniciantes"
                };

                string generatedTitle = String.Format(titleTemplates[RandomHelpers.RandomInt(titleTemplates.Length)], word);

                Video video = new Video();
                video.Name = generatedTitle;
                video.Category = VideoCategory.Comedy;
                video.GenerateRivalQuality(ShootingSkill, PostProductionSkill);
                video.SetErrorImage();

                Channel.Videos.Add(video);
            }
            catch { }
        }
    }
}
