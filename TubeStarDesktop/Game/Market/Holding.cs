namespace TubeStar
{
    /// <summary>
    /// Posição do jogador em um ativo (vai no save, dentro do Market)
    /// </summary>
    public class Holding
    {
        public string Ticker { get; set; }
        public double Quantity { get; set; }
        public double AvgPrice { get; set; }
        public double TotalInvested { get; set; }
        public int FirstBuyDay { get; set; }

        public Holding()
        {
        }
    }
}
