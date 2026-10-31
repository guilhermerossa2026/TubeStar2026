using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TubeStar
{
    public static class SaveLoadHelper
    {
        public static string SaveDirectory
        {
            get { return AppDomain.CurrentDomain.BaseDirectory + @"\Saves"; }
        }

        public static string SaveFile
        {
            get { return SaveDirectory + @"\save.tss"; }
        }

        private static JsonSerializerSettings JsonSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };
            }
        }

        public static void Save(string fileName)
        {
            try
            {
                var directory = Path.GetDirectoryName(fileName);
                if (!String.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                SaveObj obj = new SaveObj();
                obj.Player = Player.Current;
                obj.ShareViews = VideoViewer.GetShares();
                obj.BoughtViews = VideoViewer.GetBoughtViews();
                obj.Studies = Studies.Current;
                obj.StoreItems = StoreItems.Current;

                foreach (var rival in Rivals.Current.All)
                {
                    rival.Channel.SubscribersOverTime.Clear();
                    rival.Channel.IncomeOverTime.Clear();
                    rival.Channel.ExpensesOverTime.Clear();

                    foreach (var video in rival.Channel.Videos)
                    {
                        video.Comments.Clear();
                    }
                }
                obj.Rivals = Rivals.Current.All;

                var json = JsonConvert.SerializeObject(obj, JsonSettings);
                File.WriteAllText(fileName, json);
            }
            catch(Exception)
            {
                CustomMessageBox.ShowDialog(EnglishStrings.Failure.Translate(), EnglishStrings.SaveFail.Translate(), MessagePicture.Error);
            }
        }

        public static void Load(string fileName)
        {
            try
            {
                var content = File.ReadAllText(fileName);
                SaveObj saveObj = null;
                bool isXml = !string.IsNullOrWhiteSpace(content) && content.TrimStart().StartsWith("<");

                if (isXml)
                {
                    // Compatibilidade retroativa para saves antigos em XML
                    saveObj = SerializationHelpers.FromXml<SaveObj>(content);
                }
                else
                {
                    // Formato moderno JSON
                    saveObj = JsonConvert.DeserializeObject<SaveObj>(content, JsonSettings);
                }

                if (saveObj == null || saveObj.Player == null || saveObj.Player.Channels == null)
                    throw new Exception("Falha ao desserializar save.");

                //O XmlSerializer acrescenta à lista criada pelo construtor do Player,
                //então o XML antigo traz um canal "não lançados" extra no início.
                //No JSON (Replace) a lista vem exatamente como foi salva.
                if (isXml && saveObj.Player.Channels.Count > 0)
                    saveObj.Player.Channels.RemoveAt(0);

                //Valida antes de trocar o jogo atual: save corrompido não deixa estado quebrado
                if (saveObj.Player.Channels.Count == 0)
                    throw new Exception("Save sem canais.");

                Player.Current = saveObj.Player;
                Channel.UnreleasedVideos = Player.Current.Channels[0];

                VideoViewer.SetShares(saveObj.ShareViews ?? new List<SerializableShareData>());
                VideoViewer.SetBoughtViews(saveObj.BoughtViews ?? new List<SerializableShareData>());
                Studies.Current = saveObj.Studies;
                StoreItems.Current = saveObj.StoreItems;

                if (saveObj.Rivals != null && saveObj.Rivals.Count > 0)
                    Rivals.Current.PopulateFromList(saveObj.Rivals);
            }
            catch (Exception)
            {
                //A desserialização já mexeu nos estáticos (o construtor do Player chama Reset):
                //volta para um jogo novo coerente em vez de deixar peças misturadas
                VideoViewer.Reset();
                Player.Current.Reset();
                CustomMessageBox.ShowDialog(EnglishStrings.Failure.Translate(), EnglishStrings.LoadFail.Translate(), MessagePicture.Error);
            }
        }
    }
}