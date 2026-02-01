using Microsoft.Extensions.Logging;
using static CryptoPredictorWebApi.Program;

namespace CryptoPredictorWebApi
{
    public sealed class DataReaderHostedService : IHostedService
    {
        private readonly Task _completedTask = Task.CompletedTask;
        private Timer? _timer;
        private readonly ICryptoService _cryptoService;
        private readonly IDataProviderService _dataProviderService;

        public DataReaderHostedService(ICryptoService cryptoService, IDataProviderService dataProviderService)
        {
            _cryptoService = cryptoService;
            _dataProviderService = dataProviderService;
        }

        private void DoWork(object? state)
        {
            var cryptoPriceData = _dataProviderService.GetData();
            _cryptoService.Initialize(cryptoPriceData);
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);

            return _completedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return _completedTask;
        }
    }
}
