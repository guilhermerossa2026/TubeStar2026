using System.Collections.Generic;

namespace TubeStar
{
    /// <summary>
    /// Estado de um ativo (vai pro save). Só propriedades públicas simples.
    /// </summary>
    public class AssetState
    {
        public string Ticker { get; set; }
        public double Price { get; set; }
        public double PrevPrice { get; set; }
        //Preço "justo" que cresce com a tendência anual; o preço volta para perto dele
        public double TrendLine { get; set; }
        //Últimos 112 dias
        public List<double> Daily { get; set; }
        //Últimas 260 semanas
        public List<double> Weekly { get; set; }

        public AssetState()
        {
            Daily = new List<double>();
            Weekly = new List<double>();
        }
    }
}
