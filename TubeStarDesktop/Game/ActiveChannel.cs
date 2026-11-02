using System;
using System.Linq;

namespace TubeStar
{
    /// <summary>
    /// Canal escolhido no seletor do menu lateral. Nulo = "Visão geral" (todos os canais somados)
    /// </summary>
    public static class ActiveChannel
    {
        private static Channel _current;

        public static event Action Changed;

        public static Channel Current
        {
            get
            {
                //Canal apagado ou jogo novo/carregado: volta para a Visão geral
                if (_current != null && (Player.Current == null || !Player.Current.Channels.Contains(_current)))
                    _current = null;
                return _current;
            }
        }

        public static void Select(Channel channel)
        {
            if (channel == Channel.UnreleasedVideos)
                channel = null;
            if (channel == Current)
                return;

            _current = channel;
            //Postar vídeo já abre com o canal escolhido
            if (channel != null)
                Settings.LastChannel = channel;
            if (Changed != null)
                Changed();
        }

        //Inscritos do canal escolhido ou de todos (Visão geral)
        public static long Subscribers
        {
            get
            {
                if (Current != null)
                    return Current.Subscribers;
                return Player.Current.Channels.Where(c => c != Channel.UnreleasedVideos).Sum(c => (long)c.Subscribers);
            }
        }

        //Quanto o canal ganha a cada mil views (ganhos totais / views totais)
        public static double GetRpm(Channel channel)
        {
            long views = channel.Videos.Sum(v => (long)v.Views);
            return views > 0 ? channel.Income * 1000.0 / views : 0;
        }
    }
}
