
namespace CryptoPredictorWebApi
{
    public partial class Program
    {
        public class CryptoCacheService : ICryptoCacheService
        {
            private readonly IReadOnlyList<CryptoPriceRow> _data;
            private readonly Lazy<IReadOnlyList<string>> _currencyNames;

            public CryptoCacheService(IReadOnlyList<CryptoPriceRow> data)
            {
                _data = data;

                _currencyNames = new Lazy<IReadOnlyList<string>>(
                    () => _data
                        .Select(x => x.Name)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList(),
                    isThreadSafe: true
                );
            }

            public IReadOnlyList<string> GetCurrencies() => _currencyNames.Value;
        }

        public partial class CryptoCalculationsService : ICryptoService
        {
            private IReadOnlyList<CryptoPriceRow> _data = [];
            private IReadOnlyList<string>? _currencyNames;

            public void Initialize(IReadOnlyList<CryptoPriceRow> data)
            {
                _data = data;
                _currencyNames = _data
                       .Select(x => x.Name)
                       .Distinct()
                       .OrderBy(x => x)
                       .ToList();
            }

            public IReadOnlyList<CryptoPriceRow> GetAll() => _data;

            public IReadOnlyList<string> GetCryptoNames() => _currencyNames ?? [];

            public decimal GetMaxCryptoPriceByMonth(string cryptoName, int year, int month) => 
                GetCryptoStatisticsByMonthSlice(cryptoName, year, month, StatisticsCalculationType.Max);

            public decimal GetMinCryptoPriceByMonth(string cryptoName, int year, int month) => 
                GetCryptoStatisticsByMonthSlice(cryptoName, year, month, StatisticsCalculationType.Min);

            public decimal GetOlbestCryptoPriceByMonth(string cryptoName, int year, int month) => 
                GetCryptoStatisticsByMonthSlice(cryptoName, year, month, StatisticsCalculationType.Oldest);

            public decimal GetNewestCryptoPriceByMonth(string cryptoName, int year, int month) => 
                GetCryptoStatisticsByMonthSlice(cryptoName, year, month, StatisticsCalculationType.Newest);

            public decimal GetCryptoStatisticsByMonthSlice(string cryptoName, int year, int month,  StatisticsCalculationType statisticsCalculationType)
            {
                if (month < 1 || month > 12)
                {
                    throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
                }
                return GetCryptoStatisticsByDateRangeSlice(cryptoName, new DateTime(year, month, 1), new DateTime(year, month, 1).AddMonths(1).AddDays(-1), statisticsCalculationType);
            }

            public decimal GetCryptoStatisticsByDateRangeSlice(string cryptoName, DateTime startRange, DateTime endRange, StatisticsCalculationType statisticsCalculationType)
            {
                if (startRange > endRange)
                {
                    throw new ArgumentException("Start range must be less than or equal to end range.");
                }

                if (!GetCryptoNames().Contains(cryptoName, StringComparer.OrdinalIgnoreCase)) {
                    return 0;
                }

                var filteredData = _data.Where(x =>
                       x.Name.Equals(cryptoName, StringComparison.OrdinalIgnoreCase) &&
                       x.Date >= startRange &&
                       x.Date <= endRange);

                if (!filteredData.Any())
                {
                    return 0;
                }

                switch (statisticsCalculationType)
                {
                    case StatisticsCalculationType.Oldest:
                        {
                            return filteredData.OrderBy(x => x.Date).First().Price;
                        }
                        case StatisticsCalculationType.Newest:
                        {
                            return filteredData.OrderByDescending(x => x.Date).First().Price;
                        }
                        case StatisticsCalculationType.Min:
                        {
                            
                            return filteredData.Min(x => x.Price);
                        }
                        case StatisticsCalculationType.Max:
                        {
                            return filteredData.Max(x => x.Price);
                        }
                        default: 
                        { 
                            throw new ArgumentOutOfRangeException(nameof(statisticsCalculationType), "Invalid statistics calculation type."); 
                        }
                }
            }
        
            public List<CryptoNormalizedRange> GetNormalizedValuesForDateRange(int year, int month) =>
                GetNormalizedValuesForDateRange(
                    new DateTime(year, month, 1),
                    new DateTime(year, month, 1).AddMonths(1).AddDays(-1)
                    );

            public List<CryptoNormalizedRange> GetNormalizedValuesForDateRange(DateTime startRange, DateTime endRange)
            {
                if (startRange > endRange)
                {
                    throw new ArgumentException("Start range must be less than or equal to end range.");
                }

                var filteredData = _data.Where(x =>
                       x.Date >= startRange &&
                       x.Date <= endRange);

                if (!filteredData.Any())
                {
                    return [];
                }

                var normalizedData = filteredData.GroupBy(x => x.Name).Select(cryptoItem =>
                {
                    var minPrice = cryptoItem.Min(y => y.Price);
                    var maxPrice = cryptoItem.Max(y => y.Price);
                    return new CryptoNormalizedRange(
                        cryptoItem.First().Name,
                        (maxPrice - minPrice) / (maxPrice + minPrice)
                        );
                });

                return normalizedData.OrderByDescending(x => x.NormalizedRange).ToList();
            }

            public CryptoNormalizedRange GetHighestNormalizedValueCryptoForDateRange(int year, int month) => 
                GetHighestNormalizedValueCryptoForDateRange(
                    new DateTime(year, month, 1),
                    new DateTime(year, month, 1).AddMonths(1).AddDays(-1)
                    );

            public CryptoNormalizedRange GetHighestNormalizedValueCryptoForDateRange(DateTime startRange, DateTime endRange) => 
                GetNormalizedValuesForDateRange(startRange, endRange).FirstOrDefault() ??
                new CryptoNormalizedRange("", 0);
        }
    }
}
