using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

namespace SocialService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 🔹 1️⃣ Đọc cấu hình
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // 🔹 2️⃣ Cấu hình Kestrel (Render hoặc local)
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                var port = Environment.GetEnvironmentVariable("PORT");
                if (!string.IsNullOrEmpty(port))
                {
                    serverOptions.ListenAnyIP(int.Parse(port));
                }
            });

            // 🔹 3️⃣ Kết nối Database
            builder.Services.AddDbContext<Exe201SocialServiceDbContext>(options =>
            {
                var connStr = Environment.GetEnvironmentVariable("DefaultConnectionStringDB")
                                ?? builder.Configuration.GetConnectionString("DefaultConnectionStringDB");
                options.UseSqlServer(connStr);
            });

            // 🔹 4️⃣ Firebase (nếu bạn có dùng upload ảnh)
            var firebaseSection = builder.Configuration.GetSection("FirebaseSettings");
            var firebaseCredentialsJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");
            if (!string.IsNullOrEmpty(firebaseCredentialsJson))
            {
                var tempPath = Path.Combine(Path.GetTempPath(), "firebase-credentials.json");
                File.WriteAllText(tempPath, firebaseCredentialsJson);
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", tempPath);
            }
            else
            {
                var credentialsPath = firebaseSection["CredentialsPath"];
                if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
                {
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
                }
                else
                {
                    throw new Exception("Firebase credentials not found!");
                }
            }

            builder.Services.Configure<FirebaseSettings>(firebaseSection);
            builder.Services.AddSingleton(StorageClient.Create());
            builder.Services.AddScoped<IFirebaseStorageService, FirebaseStorageService>();
            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
            builder.Services.AddHttpContextAccessor();

            // 🔹 5️⃣ JSON config
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            });

            // 🔹 6️⃣ JWT Authentication (nhận token từ query string)
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
                        )
                    };

                    // ✅ Cho phép lấy token từ query string (cho SignalR)
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            // 🔹 7️⃣ CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials()
                          .AllowAnyOrigin();

                });
            });

            // 🔹 8️⃣ Repository & Service
            builder.Services.AddScoped<IPostRepository, PostRepository>();
            builder.Services.AddScoped<IPostService, PostService>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IPostShareRepository, PostShareRepository>();
            builder.Services.AddScoped<IPostShareService, PostShareService>();
            builder.Services.AddScoped<IPostReactionRepository, PostReactionRepository>();
            builder.Services.AddScoped<IPostReactionService, PostReactionService>();
            builder.Services.AddHttpClient<IRestaurantApiService, RestaurantApiService>();
            builder.Services.AddScoped<IMessageRepository, MessageRepository>();
            builder.Services.AddScoped<IMessageService, MessageService>();
            builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();
            builder.Services.AddScoped<IFriendshipService, FriendshipService>();

            // 🔹 9️⃣ SignalR
            builder.Services.AddSignalR();

            // 🔹 🔟 Swagger (có JWT)
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "SocialService API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Nhập token JWT hợp lệ",
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

            // 🔹 11️⃣ Build app
            var app = builder.Build();

            // Middleware pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();

            // 🔹 12️⃣ Map controllers & hub
            app.MapControllers();
            app.MapHub<ChatHub>("/chathub"); // ✅ endpoint SignalR

            app.Run();
        }
    }
}
