namespace TubeStar
{
    //Só acrescentar valores no fim: o save grava o número
    public enum TradeKind { Buy, Sell, Returned }

    /// <summary>
    /// Registro de uma operação no mercado (vai no save, dentro do Market)
    /// </summary>
    public class MarketTrade
    {
        public TradeKind Kind { get; set; }
        public string Ticker { get; set; }
        public int Day { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public double Fee { get; set; }
        public double Tax { get; set; }
        public double Profit { get; set; }
        public double Total { get; set; }

        public MarketTrade()
        {
        }
    }
}
