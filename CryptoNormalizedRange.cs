namespace CryptoPredictorWebApi
{
    public partial class Program
    {
        public record CryptoNormalizedRange(
            string Name,
            decimal NormalizedRange
         );
    }
}
