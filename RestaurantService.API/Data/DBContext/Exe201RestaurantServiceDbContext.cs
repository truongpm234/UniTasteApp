using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RestaurantService.API.Models.Entity;

namespace RestaurantService.API.Data.DBContext;

public partial class Exe201RestaurantServiceDbContext : DbContext
{
    public Exe201RestaurantServiceDbContext()
    {
    }

    public Exe201RestaurantServiceDbContext(DbContextOptions<Exe201RestaurantServiceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BannerAd> BannerAds { get; set; }

    public virtual DbSet<BannerBooking> BannerBookings { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Feature> Features { get; set; }

    //public virtual DbSet<OpeningHour> OpeningHours { get; set; }

    public virtual DbSet<PhotoReview> PhotoReviews { get; set; }

    public virtual DbSet<PriceRange> PriceRanges { get; set; }

    public virtual DbSet<Restaurant> Restaurants { get; set; }

    public virtual DbSet<RestaurantCategory> RestaurantCategories { get; set; }

    public virtual DbSet<RestaurantFeature> RestaurantFeatures { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    private string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", true, true)
        .Build();
        var strConn = config["ConnectionStrings:DefaultConnectionStringDB"];
        return strConn;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(GetConnectionString());


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BannerAd>(entity =>
        {
            entity.HasKey(e => e.BannerAdId).HasName("PK__BannerAd__80CC01C9A4A9ADB5");

            entity.ToTable("BannerAd");

            entity.Property(e => e.ImageUrl).HasMaxLength(250);
            entity.Property(e => e.LinkUrl).HasMaxLength(250);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<BannerBooking>(entity =>
        {
            entity.HasKey(e => e.BannerBookingId).HasName("PK__BannerBo__1F3E2DADE2F0AACE");

            entity.ToTable("BannerBooking");

            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A0BB4B8408D");

            entity.ToTable("Category");

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.SourceType).HasMaxLength(30);
        });

        modelBuilder.Entity<Feature>(entity =>
        {
            entity.HasKey(e => e.FeatureId).HasName("PK__Feature__82230BC9E1B8A6F4");

            entity.ToTable("Feature");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        //modelBuilder.Entity<OpeningHour>(entity =>
        //{
        //    entity.HasKey(e => e.OpeningHourId).HasName("PK__OpeningH__F6C47B0D9899D16F");

        //    entity.ToTable("OpeningHour");
        //});

        modelBuilder.Entity<PhotoReview>(entity =>
        {
            entity.HasKey(e => e.PhotoReviewId).HasName("PK__PhotoRev__BFB6F5572F689529");

            entity.ToTable("PhotoReview");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhotoUrl).HasMaxLength(512);

            entity.HasOne(d => d.Review).WithMany(p => p.PhotoReviews)
                .HasForeignKey(d => d.ReviewId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhotoRevi__Revie__4BAC3F29");
        });

        modelBuilder.Entity<PriceRange>(entity =>
        {
            entity.HasKey(e => e.PriceRangeId).HasName("PK__PriceRan__B8A301DF6BC41289");

            entity.ToTable("PriceRange");

            entity.Property(e => e.MaxPrice).HasColumnType("money");
            entity.Property(e => e.MinPrice).HasColumnType("money");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(e => e.RestaurantId).HasName("PK__Restaura__87454C9555671854");

            entity.ToTable("Restaurant");

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.CoverImageUrl).HasMaxLength(2048);
            entity.Property(e => e.GooglePlaceId).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Website).HasMaxLength(500);
        });

        modelBuilder.Entity<RestaurantCategory>(entity =>
        {
            entity.HasKey(e => new { e.RestaurantId, e.CategoryId }).HasName("PK__Restaura__26D5DF35CD295C23");

            entity.ToTable("RestaurantCategory");
        });

        modelBuilder.Entity<RestaurantFeature>(entity =>
        {
            entity.HasKey(e => new { e.RestaurantId, e.FeatureId }).HasName("PK__Restaura__0F677C29EB222D66");

            entity.ToTable("RestaurantFeature");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79CE0D4ED621");

            entity.Property(e => e.UserName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
