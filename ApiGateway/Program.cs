using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Đọc ocelot.json
            builder.Configuration
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);


            // Đọc biến môi trường (Render, Azure, Heroku, ... sẽ inject env cho bạn)
            builder.Configuration.AddEnvironmentVariables();

            // Thêm dịch vụ Ocelot
            builder.Services.AddOcelot();

            // Thêm swagger cho debug gateway (không bắt buộc)
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();

            // Cấu hình JWT authentication (ưu tiên lấy từ env, fallback sang file nếu cần)
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];
            var jwtKey = builder.Configuration["Jwt:Key"];

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,  
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine($"[AUTH FAILED] {ctx.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                var userId = ctx.Principal?.FindFirst("sub")?.Value
                          ?? ctx.Principal?.FindFirst("nameid")?.Value;
                Console.WriteLine($"[TOKEN VALID] User: {userId}");
                return Task.CompletedTask;
            }
        };
    });
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Swagger UI cho dev
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapGet("/", () => "API Gateway is running!");

            app.UseHttpsRedirection();

            // AuthN/AuthZ middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Ocelot pipeline (bắt buộc phải cuối cùng)
            app.UseOcelot().Wait();

            app.Run();
        }
    }
}
