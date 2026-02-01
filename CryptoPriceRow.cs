namespace CryptoPredictorWebApi
{
    public record CryptoPriceRow(
        string Name,
        decimal Price,
        DateTime Date
    );
}
