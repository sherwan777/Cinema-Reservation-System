using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using CinemaReservationSystemApi.Configurations;
using CinemaReservationSystemApi.Services;
using Microsoft.Extensions.Options;


namespace CinemaReservationSystemApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecific",
                    builder =>
                    {
                        builder
                        .WithOrigins("http://localhost:3000" , "https://localhost:44305" , "https://chic-licorice-ebf14b.netlify.app")
                        .AllowCredentials()
                        .WithMethods("GET", "POST" , "PUT" , "DELETE")
                        .WithHeaders("Content-Type", "Authorization");
                    });
            });

            services.AddLogging(config =>
            {
                config.AddDebug();
                config.AddConsole();
            });

            services.AddSingleton<IConfiguration>(provider => new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build());

            services.Configure<MongoDbSettings>(
               Configuration.GetSection(nameof(MongoDbSettings)));

            var smtpSettings = new SMTPSettings();
            Configuration.GetSection("SMTP").Bind(smtpSettings);
            services.AddSingleton(smtpSettings);

            services.AddSingleton<IMongoDbSettings>(sp =>
                sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);


            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CinemaReservationSystemApi", Version = "v1" });
            });

            services.AddSingleton<EmailService>();
            services.AddSingleton<MovieService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<BookingService>();
            services.AddSingleton<CinemaService>();
            services.AddLogging(); 
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CinemaReservationSystemApi v1"));
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowSpecific");

            app.UseRouting();

            app.UseMiddleware<JwtMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CinemaReservationSystemApi v1");
            });
        }
    }
}
