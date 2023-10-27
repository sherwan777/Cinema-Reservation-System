using Serilog;

namespace CinemaReservationSystemApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
             Host.CreateDefaultBuilder(args)
                 .UseSerilog((hostingContext, loggerConfiguration) =>
                 {
                     loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
                                       .Enrich.FromLogContext()
                                       .WriteTo.Console()
                                       .WriteTo.File("Logs/myapp.txt", rollingInterval: RollingInterval.Day);
                 })
                 .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder.UseStartup<Startup>();
                 });
    }
}
