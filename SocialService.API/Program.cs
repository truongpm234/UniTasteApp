using Google.Cloud.Storage.V1;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using SocialService.API.Data.DBContext;
using SocialService.API.Hubs;
using SocialService.API.Models.DTO;
using SocialService.API.Repository;
using SocialService.API.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace SocialService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 🔹 1️⃣ Đọc cấu hình từ appsettings + môi trường
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

            // 🔹 2️⃣ Kết nối Database
            builder.Services.AddDbContext<Exe201SocialServiceDbContext>(options =>
            {
                var connStr = Environment.GetEnvironmentVariable("DefaultConnectionStringDB")
                                ?? builder.Configuration.GetConnectionString("DefaultConnectionStringDB");
                options.UseSqlServer(connStr);
            });

            // 🔹 3️⃣ Cấu hình Firebase Storage (y hệt UserService)
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

            builder.Services.Configure<FirebaseSettings>(firebaseSection);
            builder.Services.AddSingleton(StorageClient.Create());
            builder.Services.AddScoped<IFirebaseStorageService, FirebaseStorageService>();

            // 🔹 4️⃣ Controllers & JSON options
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            });
            builder.Services.AddHttpContextAccessor();

            // 🔹 5️⃣ JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
                        )
                    };
                });

            // 🔹 6️⃣ CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            // 🔹 7️⃣ Repository & Service
            builder.Services.AddScoped<IPostRepository, PostRepository>();
            builder.Services.AddScoped<IPostService, PostService>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IPostShareRepository, PostShareRepository>();
            builder.Services.AddScoped<IPostShareService, PostShareService>();
            builder.Services.AddScoped<IPostReactionRepository, PostReactionRepository>();
            builder.Services.AddScoped<IPostReactionService, PostReactionService>();
            builder.Services.AddHttpClient<IRestaurantApiService, RestaurantApiService>();



            // 🔹 8️⃣ SignalR
            builder.Services.AddSignalR();

            // 🔹 9️⃣ Swagger (có JWT Authorize)
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
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });

            var app = builder.Build();

            // 🔹 🔟 Middleware pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAllOrigins");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<ChatHub>("/chathub");

            app.UseSwagger();
            app.UseSwaggerUI();

            app.Run();
        }
    }
}
