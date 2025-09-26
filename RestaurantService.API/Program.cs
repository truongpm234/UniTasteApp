
using Microsoft.EntityFrameworkCore;
using RestaurantService.API.Data.DBContext;
using RestaurantService.API.Models.DTO;
using RestaurantService.API.Repository;
using RestaurantService.API.Service;

namespace RestaurantService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

           builder.Services.AddDbContext<Exe201RestaurantServiceDbContext>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionStrings")));
            builder.Services.AddScoped<IRestaurantService, Service.RestaurantService>();
            builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));
            builder.Services.AddHttpClient<IGooglePlacesService, GooglePlacesService>();
            builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
