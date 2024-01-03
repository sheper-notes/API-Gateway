
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Eureka;
using Ocelot.Provider.Kubernetes;

namespace API_Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var corsOrigin = builder.Configuration.GetValue<string>("CorsOrigin");
            var auth0Domain = $"https://{builder.Configuration.GetValue<string>("auth0Domain")}/";
            var auth0Audience = builder.Configuration.GetValue<string>("auth0Audience");
            auth0Audience = "https://sheper.eu.auth0.com/api/v2/";
            auth0Domain = "sheper.eu.auth0.com";
            var authenticationProviderKey = "TestKey";
            Console.WriteLine("Loaded: " + corsOrigin);
            Console.WriteLine("Loaded: " + auth0Domain);
            Console.WriteLine("Loaded: " + auth0Audience);

            var corsPolicy = "SheperCorsPolicy";
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddAuthentication(options =>
            //    {
            //        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //    }
            //).AddJwtBearer(options =>
            //{
            //    options.Authority = auth0Domain;
            //    options.Audience = auth0Audience;
            //    options.RequireHttpsMetadata = false;
            //});
            builder.Services.AddSwaggerGen();
            builder.Configuration.AddJsonFile("Controllers/ocelot.json");
            builder.Services.AddOcelot();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: corsPolicy,
                    policy =>
                    {
                        policy.WithOrigins(corsOrigin).AllowCredentials().AllowAnyHeader().AllowAnyMethod();
                    });
            });
            var app = builder.Build();
            app.UseCors(corsPolicy);
            
            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
                app.UseSwaggerUI();
            //}

            //app.UseHttpsRedirection();
            app.UseAuthentication();

            app.UseOcelot().Wait();

            app.UseMiddleware<XSSMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}