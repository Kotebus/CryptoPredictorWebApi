
namespace CryptoPredictorWebApi
{
    public partial class Program
    {
        public interface ICryptoService
        {
            void Initialize(IReadOnlyList<CryptoPriceRow> data);
            IReadOnlyList<CryptoPriceRow> GetAll();
            IReadOnlyList<string> GetCryptoNames();
            decimal GetMaxCryptoPriceByMonth(string cryptoName, int year, int month);
            decimal GetMinCryptoPriceByMonth(string cryptoName, int year, int month);
            decimal GetOlbestCryptoPriceByMonth(string cryptoName, int year, int month);
            decimal GetNewestCryptoPriceByMonth(string cryptoName, int year, int month);
            decimal GetCryptoStatisticsByMonthSlice(string cryptoName, int year, int month, StatisticsCalculationType statisticsCalculationType);
            decimal GetCryptoStatisticsByDateRangeSlice(string cryptoName, DateTime startRange, DateTime endRange, StatisticsCalculationType statisticsCalculationType);

            List<CryptoNormalizedRange> GetNormalizedValuesForDateRange(int year, int month);
            List<CryptoNormalizedRange> GetNormalizedValuesForDateRange(DateTime startRange, DateTime endRange);
            CryptoNormalizedRange GetHighestNormalizedValueCryptoForDateRange(int year, int month);
            CryptoNormalizedRange GetHighestNormalizedValueCryptoForDateRange(DateTime startRange, DateTime endRange);
        }
    }
}
