using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace TubeStar
{
    public class Video : UniqueObject
    {
        public string Name { get; set; }
        public VideoCategory Category { get; set; }
        public VideoType? Type { get; set; }

        //Campanha "Promover vídeo" em andamento (views ainda por entregar e dias restantes)
        public int PromotionViewsLeft { get; set; }
        public int PromotionDaysLeft { get; set; }

        public int? ShootQuality { get; set; }
        public int? EditQuality { get; set; }
        public int? Quality { get; set; }
        public int? QualityBias { get; set; }
        public bool ExternalQuality { get; set; }

        public int Views { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public List<VideoComment> Comments { get; set; }

        public bool HasBeenEdited { get; set; }

        public int ExtraShootingHours { get; set; }
        public int ExtraEditingHours { get; set; }
        public int Cost { get; set; }
        //Quanto o vídeo já rendeu (anúncios + crowdfunding), somado a cada dia
        public double Income { get; set; }
        //Gastos do vídeo depois de postado (views compradas)
        public double Expenses { get; set; }
        //Inscritos que o vídeo trouxe (ganhos menos os que saíram por causa dele), somado a cada dia
        public int SubscribersGained { get; set; }

        public int Iterations { get; set; }

        public double CostPerView { get; set; }
        public double OnceOffCost { get; set; }

        public List<VideoAttribute> Attributes { get; set; }

        public string YouTubeVideoId { get; set; }
        private byte[] _imageBytes;
        public byte[] ImageBytes
        {
            get { return _imageBytes; }
            set
            {
                _imageBytes = value;
                _thumbnail = null;
            }
        }

        //Cache em memória da miniatura (não é salvo)
        private BitmapImage _thumbnail;

        public BitmapImage GetThumbnail()
        {
            if (_thumbnail == null && _imageBytes != null)
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = StreamHelpers.BytesToStream(_imageBytes);
                image.EndInit();
                image.Freeze();
                _thumbnail = image;
            }
            return _thumbnail;
        }
        public event Action OnImageFetched;

        public bool IsSuspended { get; set; }

        public Video()
            : base()
        {
            Comments = new List<VideoComment>();
            Attributes = new List<VideoAttribute>();
        }

        public bool ReadyForRelease()
        {
            foreach (var task in Player.Current.TasksInProgress)
            {
                var editTask = task as EditVideo;
                if (editTask != null && editTask.Video == this)
                {
                    return false;
                }
            }
            return true;
        }

        //Investimento na produção (item 5c): níveis $0/$100/$300/$800 dão até +0/+8/+15/+25 de nota.
        //Outros valores (vídeos antigos, custo de $200/$400) seguem a regra antiga: até +5 a cada $100
        public static readonly int[] InvestmentLevels = { 0, 100, 300, 800 };
        public static readonly int[] InvestmentBonuses = { 0, 8, 15, 25 };

        public static int InvestmentMaxBonus(int cost)
        {
            int index = Array.IndexOf(InvestmentLevels, cost);
            return index >= 0 ? InvestmentBonuses[index] : (cost / 100) * 5;
        }

        public int GenerateQuality()
        {
            if (ExternalQuality && Quality.HasValue)
                return Quality.Value;

            var quality = 0;

            if (!ShootQuality.HasValue)
            {
                ShootQuality = 0;

                int schoolSkill = 0;
                Studies.Current.All.ForEach(s =>
                {
                    if (s.IsCompleted && s.SkillModifierType == SkillModifierType.Shooting)
                        schoolSkill += s.SkillModifier;
                });

                int itemSkill = 0;
                StoreItems.Current.All.ForEach(s =>
                {
                    if (s.Purchased && s.SkillModifierType == SkillModifierType.Shooting)
                        itemSkill += s.SkillModifier;
                });

                ShootQuality += RandomHelpers.RandomInt(Player.Current.ShootingSkill - schoolSkill);
                ShootQuality += RandomHelpers.RandomInt(schoolSkill);
                ShootQuality += RandomHelpers.RandomInt(itemSkill);
                ShootQuality += RandomHelpers.RandomInt(ExtraShootingHours * 2);
                ShootQuality += RandomHelpers.RandomInt(InvestmentMaxBonus(Cost));

                if (Attributes.Contains(VideoAttributes.LearnFromMistakes))
                {
                    var remove = RandomHelpers.RandomInt(5);
                    ShootQuality -= 5 + remove;
                    Player.Current.ShootingSkill += remove;
                }

                if (StoreItems.Current.Consultant.Purchased)
                {
                    ShootQuality += StoreItems.Current.Consultant.SkillModifier;
                }
            }

            quality += ShootQuality.Value;

            if (HasBeenEdited)
            {
                if (!EditQuality.HasValue)
                {
                    EditQuality = 0;

                    int schoolSkill = 0;
                    Studies.Current.All.ForEach(s => 
                    {
                        if (s.IsCompleted && s.SkillModifierType == SkillModifierType.PostProduction)
                            schoolSkill += s.SkillModifier;
                    });

                    int itemSkill = 0;
                    StoreItems.Current.All.ForEach(s =>
                    {
                        if (s.Purchased && s.SkillModifierType == SkillModifierType.PostProduction)
                            itemSkill += s.SkillModifier;
                    });
                    EditQuality += RandomHelpers.RandomInt(Player.Current.PostProductionSkill - schoolSkill);
                    EditQuality += RandomHelpers.RandomInt(schoolSkill);
                    EditQuality += RandomHelpers.RandomInt(itemSkill);
                    EditQuality += RandomHelpers.RandomInt(ExtraEditingHours * 3);

                    if (Attributes.Contains(VideoAttributes.LearnFromMistakes))
                    {
                        var remove = RandomHelpers.RandomInt(5);
                        EditQuality -= 5 + remove;
                        Player.Current.PostProductionSkill += remove;
                    }
                }

                quality += EditQuality.Value;
            }

            if (Attributes.Contains(VideoAttributes.Cats))
            {
                quality += VideoAttributes.Cats.QualityIncrease;
            }

            quality = Math.Max(quality, 0);
            quality = Math.Min(quality, 100);

            //Imperfection bias
            if (quality == 100)
            {
                if (!QualityBias.HasValue)
                {
                    QualityBias = RandomHelpers.RandomInt(5);
                }
                quality -= QualityBias.Value;
            }

            Quality = quality;
            return Quality.Value;
        }

        public int GetCommentsCount()
        {
            //Calculado a partir de likes/dislikes (não é salvo)
            return Likes / 20 + Dislikes / 10;
        }

        public int GenerateRivalQuality(int shootingSkill, int postProductionSkill)
        {
            var quality = RandomHelpers.RandomInt(shootingSkill) + RandomHelpers.RandomInt(postProductionSkill);

            quality = Math.Max(quality, 0);
            quality = Math.Min(quality, 100);

            //Imperfection bias
            if (quality == 100)
            {
                if (!QualityBias.HasValue)
                {
                    QualityBias = RandomHelpers.RandomInt(5);
                }
                quality -= QualityBias.Value;
            }

            ExternalQuality = true;
            Quality = quality;
            return Quality.Value;
        }

        public void ExternalSetImageBytes(byte[] bytes)
        {
            ImageBytes = bytes;
            if (OnImageFetched != null)
                OnImageFetched();
        }

        //Miniatura automática: busca só pelo título; sem resultado, pela categoria.
        //Sem sorteio: usa o 1º resultado (ordem de relevância)
        public void FetchFirstImage()
        {
            //Mesmo tamanho de página do Criar vídeo (50): aproveita a memória de buscas
            ThumbSearch.Search(Name, 50, null, (page) =>
            {
                if (page.Ids.Count > 0)
                {
                    SetImageFromId(page.Ids[0]);
                }
                else
                {
                    ThumbSearch.Search(Category.GetString(), 50, null, (p) =>
                    {
                        if (p.Ids.Count > 0)
                            SetImageFromId(p.Ids[0]);
                        else
                            SetErrorImage();
                    }, () =>
                    {
                        SetErrorImage();
                    });
                }
            }, () =>
            {
                SetErrorImage();
            });
        }


        //Última imagem pedida: respostas antigas (cliques rápidos) são ignoradas
        private string _requestedImageId;

        public void SetImageFromId(string youTubeVideoId)
        {
            Uri photoUri = YouTubeAPI.GetPhotoUri(youTubeVideoId);
            _requestedImageId = youTubeVideoId;

            WebClientHelpers.DownloadImage(photoUri, (result) =>
            {
                if (youTubeVideoId != _requestedImageId)
                    return;
                YouTubeVideoId = youTubeVideoId;
                ImageBytes = StreamHelpers.StreamToBytes(result);

                if (OnImageFetched != null)
                    OnImageFetched();
            }, () =>
            {
                if (youTubeVideoId == _requestedImageId)
                    SetErrorImage();
            });
        }

        public void SetErrorImage()
        {
            var info = System.Windows.Application.GetResourceStream(new Uri("../Resources/InternetDown.jpg", UriKind.Relative));
            ExternalSetImageBytes(StreamHelpers.StreamToBytes(info.Stream));
        }
    }
}