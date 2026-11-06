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

        public static bool Save(string fileName)
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

                //Rivais não guardam mais canal com vídeos (os números ficam em Player.Ranking)
                foreach (var rival in Rivals.Current.All)
                    rival.Channel = null;
                obj.Rivals = Rivals.Current.All;
                obj.DailyAgenda = DailyPlanner.GetCurrentAgendaForSave();
                obj.MoneyAtStartOfDay = DailyPlanner.GetMoneyAtStartOfDayForSave();

                var json = JsonConvert.SerializeObject(obj, JsonSettings);

                //Grava num arquivo temporário e só então troca: falha no meio não destrói o save.
                //File.Replace guarda o save anterior em .bak (só 1 cópia)
                var tmp = fileName + ".tmp";
                if (File.Exists(tmp))
                    File.Delete(tmp);
                File.WriteAllText(tmp, json);
                if (File.Exists(fileName))
                    File.Replace(tmp, fileName, fileName + ".bak");
                else
                    File.Move(tmp, fileName);
                return true;
            }
            catch(Exception)
            {
                try
                {
                    if (File.Exists(fileName + ".tmp"))
                        File.Delete(fileName + ".tmp");
                }
                catch (Exception) { }

                CustomMessageBox.ShowDialog(EnglishStrings.Failure.Translate(), EnglishStrings.SaveFail.Translate(), MessagePicture.Error);
                return false;
            }
        }

        //Devolve true se carregou (do save principal ou da cópia .bak). Se falhar, deixa um jogo novo coerente
        public static bool Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    TryLoadFile(fileName);
                    return true;
                }
                catch (Exception)
                {
                    ResetStatics();
                }
            }

            var backup = fileName + ".bak";
            if (File.Exists(backup))
            {
                try
                {
                    TryLoadFile(backup);
                    CustomMessageBox.ShowDialog(EnglishStrings.Failure.Translate(), EnglishStrings.SaveRestoredFromBackup.Translate(), MessagePicture.Error);
                    return true;
                }
                catch (Exception)
                {
                    ResetStatics();
                }
            }

            PreserveCorruptedFiles(fileName);
            CustomMessageBox.ShowDialog(EnglishStrings.Failure.Translate(), EnglishStrings.LoadFail.Translate(), MessagePicture.Error);
            return false;
        }

        //Renomeia os arquivos ilegíveis para que um novo jogo nunca sobrescreva o save ruim
        private static void PreserveCorruptedFiles(string fileName)
        {
            var stamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            var directory = Path.GetDirectoryName(fileName);
            var extension = Path.GetExtension(fileName);
            var baseName = Path.Combine(directory, "save_corrompido_" + stamp);

            try
            {
                if (File.Exists(fileName))
                    File.Move(fileName, baseName + extension);
            }
            catch (Exception) { }

            try
            {
                if (File.Exists(fileName + ".bak"))
                    File.Move(fileName + ".bak", baseName + "_bak" + extension);
            }
            catch (Exception) { }
        }

        //A desserialização já mexeu nos estáticos (o construtor do Player chama Reset):
        //volta para um jogo novo coerente em vez de deixar peças misturadas
        private static void ResetStatics()
        {
            VideoViewer.Reset();
            Player.Current.Reset();
            DailyPlanner.SetAgendaToRestore(null, null);
        }

        private static void TryLoadFile(string fileName)
        {
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

                //Se o save tem a agenda, o próximo DailyPlanner volta ao mesmo dia
                DailyPlanner.SetAgendaToRestore(saveObj.DailyAgenda, saveObj.MoneyAtStartOfDay);

                //Saves antigos não têm sorte nem momento: valor neutro
                foreach (var channel in Player.Current.Channels)
                {
                    if (channel.Momentum <= 0) channel.Momentum = 1;
                    foreach (var video in channel.Videos)
                        if (video.Luck <= 0) video.Luck = 1;
                }
                foreach (var video in Player.Current.Videos)
                    if (video.Luck <= 0) video.Luck = 1;

                RelinkReferences();
            }
        }

        //Na desserialização cada referência vira uma cópia solta. Religa as tarefas aos objetos reais:
        //vídeos (pelo Id) e estudos (a tarefa em andamento passa a ser o item de Studies.Current)
        private static void RelinkReferences()
        {
            var videos = new Dictionary<Guid, Video>();
            foreach (var video in Player.Current.Videos)
                if (video != null && !videos.ContainsKey(video.Id))
                    videos.Add(video.Id, video);
            foreach (var channel in Player.Current.Channels)
                foreach (var video in channel.Videos)
                    if (video != null && !videos.ContainsKey(video.Id))
                        videos.Add(video.Id, video);

            var studies = Studies.Current;
            foreach (var task in Player.Current.TasksInProgress)
            {
                var edit = task as EditVideo;
                if (edit != null && edit.Video != null && videos.ContainsKey(edit.Video.Id))
                    edit.Video = videos[edit.Video.Id];

                var shoot = task as ShootVideo;
                if (shoot != null && shoot.Video != null && videos.ContainsKey(shoot.Video.Id))
                    shoot.Video = videos[shoot.Video.Id];

                //Os Prerequisite leem Studies.Current, então basta trocar o item da lista pela tarefa
                if (task is StudyProductionI) studies.ProductionI = (StudyProductionI)task;
                else if (task is StudyProductionII) studies.ProductionII = (StudyProductionII)task;
                else if (task is StudyProductionIII) studies.ProductionIII = (StudyProductionIII)task;
                else if (task is StudyPostProductionI) studies.PostProductionI = (StudyPostProductionI)task;
                else if (task is StudyPostProductionII) studies.PostProductionII = (StudyPostProductionII)task;
                else if (task is StudyPostProductionIII) studies.PostProductionIII = (StudyPostProductionIII)task;
                else if (task is StudyAudienceAnalysisI) studies.AudienceAnalysisI = (StudyAudienceAnalysisI)task;
                else if (task is StudyAudienceAnalysisII) studies.AudienceAnalysisII = (StudyAudienceAnalysisII)task;
                else if (task is StudyQualityAnalysis) studies.QualityAnalysis = (StudyQualityAnalysis)task;
            }
        }
    }
}