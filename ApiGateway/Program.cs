<<<<<<< HEAD
﻿using ApiGateway.Aggregators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
=======
﻿using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
>>>>>>> a1129b948ccc7d4674db9eb146672d75d5e673f0
using System.Text;

namespace ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

<<<<<<< HEAD
            // ✅ Load ocelot.json TRƯỚC KHI truy cập Jwt config
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            // ✅ Debug nếu cần
            Console.WriteLine("✅ Jwt:Key = " + builder.Configuration["Jwt:Key"]);

            // ✅ Add Ocelot + Aggregator
            builder.Services.AddOcelot()
                .AddSingletonDefinedAggregator<UserPaymentAggregator>();

            // ✅ Swagger và Controllers
=======
            // Load Ocelot config
            builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

            // Add services
            builder.Services.AddOcelot();
>>>>>>> a1129b948ccc7d4674db9eb146672d75d5e673f0
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });

            builder.Services.AddAuthorization();

            // 🔑 Add Authentication + JWT
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });
            builder.Configuration.AddEnvironmentVariables();

            builder.Services.AddAuthorization();

            var app = builder.Build();

<<<<<<< HEAD
            // ✅ Swagger UI
=======
            // Swagger
>>>>>>> a1129b948ccc7d4674db9eb146672d75d5e673f0
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapGet("/", () => "API Gateway is running!");

<<<<<<< HEAD
            // ✅ Middleware pipeline
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // ✅ Tạm thời bỏ qua xác thực SSL (cho dev)
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            // ✅ Khởi động Ocelot
=======
            app.UseHttpsRedirection();

            // Middleware pipeline
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Ocelot Gateway
>>>>>>> a1129b948ccc7d4674db9eb146672d75d5e673f0
            app.UseOcelot().Wait();

            app.Run();
        }
    }
}
