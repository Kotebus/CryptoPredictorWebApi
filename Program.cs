
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;

namespace CryptoPredictorWebApi
{
    public partial class Program
    {
        private const string SWAGGER_ENDPOINT = "/swagger/v1/swagger.json";
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<IDataProviderService, DataProviderCsvService>();
            builder.Services.AddSingleton<ICryptoService, CryptoCalculationsService>();
            builder.Services.AddSingleton<ICryptoCacheService>(provider =>
            {
                var cryptoService = provider.GetRequiredService<ICryptoService>();
                return new CryptoCacheService(cryptoService.GetAll());
            });

            builder.Services.AddHostedService<DataReaderHostedService>();

            // Add services to the container.
            builder.Services.AddAuthorization();
            //builder.Services.AddAuthentication("Bearer").AddJwtBearer();

            //// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            //builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Crypro API",
                    Description = "An ASP.NET Core Web API for managing Crypto Currency Data",
                    Contact = new OpenApiContact
                    {
                        Name = "Maxim Suleimanov",
                        Email = "max.suleimanov.dev@gmail.com",
                        Url = new Uri("https://github.com/Kotebus")
                    },
                });
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 10,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options => 
                { 
                    options.SwaggerEndpoint(SWAGGER_ENDPOINT, "v1"); 
                    options.RoutePrefix = "";
                });
            }

            app.UseHttpsRedirection();
            app.MapSwagger().RequireAuthorization();
            app.UseAuthorization();

            app.Use(async (context, next) => {
                Console.WriteLine($"[{context.Request.Path} start: {DateTime.UtcNow}]");
                await next(context);
                Console.WriteLine($"[{context.Request.Path} finish: {DateTime.UtcNow}]");
            });

            app.MapGet("/cryptonames", (ICryptoService cryptoService) =>
            {
                var data = cryptoService.GetCryptoNames();
                return Results.Ok(new { data });
            })
                .WithName("GetCryptoNames")
                .WithSummary("Returns the names of crypto currencies presented in the data");

            app.MapGet("/data", (ICryptoService cryptoService) =>
            {
                var data = cryptoService.GetAll();
                return Results.Ok(new { data });
            })
                .WithName("GetData")
                .WithSummary("Returns all the crypto currencies data");

            //{"name":"BTC","price":45922.01,"date":"2022-01-03T21:00:00"}

            app.MapGet("/price/{statisticsCalculationType}/{cryptoName}/{year}/{month}",
                (
                    [FromHeader(Name = "Name of crypto currency")]
                    string cryptoName, int year, int month, ICryptoService cryptoService, StatisticsCalculationType statisticsCalculationType) =>
            {
                try
                {
                    var price = cryptoService.GetCryptoStatisticsByMonthSlice(cryptoName, year, month, statisticsCalculationType);
                    return Results.Ok(new { price });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { ex.Message });
                }
            })
                .WithName("GetCryptoPriceSliceByDate")
                .WithSummary("Returns price for requested crypto currency for a specific month, calculated by different parameters");

            app.MapGet("/pricerange/{statisticsCalculationType}/{cryptoName}/{startDate}/{endDate}",
                (
                    [FromHeader(Name = "Name of crypto currency")]
                    string cryptoName,
                    [FromHeader(Name = "Filtration period start date")]
                    DateTime startDate,
                    [FromHeader(Name = "Filtration period end date")]
                    DateTime endDate, 
                    ICryptoService cryptoService, StatisticsCalculationType statisticsCalculationType) =>
            {
                try
                {
                    var price = cryptoService.GetCryptoStatisticsByDateRangeSlice(cryptoName, startDate, endDate, statisticsCalculationType);
                    return Results.Ok(new { price });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { ex.Message });
                }
            })
                .WithName("GetCryptoPriceSliceByDateRange")
                .WithSummary("Returns price for requested crypto currency for a specific date range, calculated by different parameters");


            app.MapGet("/normalized/{year}/{month}", (int year, int month, ICryptoService cryptoService) =>
            {
                try
                {
                    var data = cryptoService.GetNormalizedValuesForDateRange(year, month);
                    return Results.Ok(new { data });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { ex.Message });
                }
            })
                .WithName("GetNormalizedCryptoValuesByDate")
                .WithSummary("Returns normalized values for all crypto currencies by specific month");


            app.MapGet("/maxnormalized/{year}/{month}", (int year, int month, ICryptoService cryptoService) =>
            {
                try
                {
                    var data = cryptoService.GetHighestNormalizedValueCryptoForDateRange(year, month);
                    return Results.Ok(new { data });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { ex.Message });
                }
            })
                .WithName("GetMaxNormalizedCryptoByDate")
                .WithSummary("Returns normalized values for all crypto currencies by specific date range");

            app.Run();
        }
    }
}
