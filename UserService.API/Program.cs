using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using UserService.API.Data.DBContext;
using UserService.API.Models.DTO;
using UserService.API.Repository;
using UserService.API.Service;
using UserService.API.Services;

namespace UserService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                var port = Environment.GetEnvironmentVariable("PORT");
                if (!string.IsNullOrEmpty(port))
                {
                    serverOptions.ListenAnyIP(int.Parse(port));
                }
            });

            // Cấu hình DbContext
            builder.Services.AddDbContext<Exe201UserServiceDbContext>(options =>
            {
                var connStr = Environment.GetEnvironmentVariable("DefaultConnectionStringDB")
                                ?? builder.Configuration.GetConnectionString("DefaultConnectionStringDB");
                options.UseSqlServer(connStr);
            });

            var firebaseSection = builder.Configuration.GetSection("FirebaseSettings");
            var bucketName = firebaseSection["BucketName"];

            var firebaseCredentialsJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");

            Console.WriteLine($"[DEBUG] FIREBASE_CREDENTIALS_JSON exists: {!string.IsNullOrEmpty(firebaseCredentialsJson)}");
            Console.WriteLine($"[DEBUG] FIREBASE_CREDENTIALS_JSON length: {firebaseCredentialsJson?.Length ?? 0}");

            if (!string.IsNullOrEmpty(firebaseCredentialsJson))
            {
                try
                {
                    var tempPath = Path.Combine(Path.GetTempPath(), "firebase-credentials.json");
                    File.WriteAllText(tempPath, firebaseCredentialsJson);
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", tempPath);
                    Console.WriteLine($"[DEBUG] Firebase credentials written to: {tempPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to write Firebase credentials: {ex.Message}");
                    throw;
                }
            }
            else
            {
                Console.WriteLine("[WARNING] FIREBASE_CREDENTIALS_JSON not found in environment variables");
                var credentialsPath = firebaseSection["CredentialsPath"];
                if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
                {
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
                    Console.WriteLine($"[DEBUG] Using local credentials file: {credentialsPath}");
                }
                else
                {
                    throw new Exception("Firebase credentials not found! Please set FIREBASE_CREDENTIALS_JSON environment variable.");
                }
            }

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            });

            builder.Services.Configure<FirebaseSettings>(firebaseSection);
            builder.Services.AddSingleton(StorageClient.Create());
            builder.Services.AddScoped<IFirebaseStorageService, FirebaseStorageService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IUserService, Services.UserService>();
            builder.Services.AddHttpClient<IGeminiAIService, GeminiAIService>();

            builder.Services.AddHttpClient();

            // JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
                        )
                    };
                });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            // Swagger + JWT config
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(option =>
            {
                option.DescribeAllParametersInCamelCase();
                option.ResolveConflictingActions(conf => conf.First());
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("AllowAllOrigins");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.Run();
        }
    }
}