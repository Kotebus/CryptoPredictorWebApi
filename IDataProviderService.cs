namespace CryptoPredictorWebApi
{
    public interface IDataProviderService
    {
        List<CryptoPriceRow> GetData();
    }
}