
namespace CryptoPredictorWebApi
{
    public interface ICryptoCacheService
    {
        IReadOnlyList<string> GetCurrencies();
    }
}