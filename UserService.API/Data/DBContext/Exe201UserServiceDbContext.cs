using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UserService.API.Models.Entity;

namespace UserService.API.Data.DBContext;

public partial class Exe201UserServiceDbContext : DbContext
{
    public Exe201UserServiceDbContext()
    {
    }

    public Exe201UserServiceDbContext(DbContextOptions<Exe201UserServiceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Accessory> Accessories { get; set; }

    public virtual DbSet<AiLog> AiLogs { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<PremiumPackage> PremiumPackages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAccessory> UserAccessories { get; set; }

    public virtual DbSet<UserPreference> UserPreferences { get; set; }

    public virtual DbSet<UserPremium> UserPremia { get; set; }

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
        modelBuilder.Entity<Accessory>(entity =>
        {
            entity.HasKey(e => e.AccessoryId).HasName("PK__Accessor__09C3F09B5A22A22C");

            entity.ToTable("Accessory");

            entity.Property(e => e.ImageUrl).HasMaxLength(250);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("money");
            entity.Property(e => e.Type).HasMaxLength(30);
        });

        modelBuilder.Entity<AiLog>(entity =>
        {
            entity.HasKey(e => e.AiLogId).HasName("PK__AiLog__1E592B35AD3285A4");

            entity.ToTable("AiLog");

            entity.Property(e => e.InputText).HasMaxLength(2000);
            entity.Property(e => e.OutputText).HasMaxLength(2000);
            entity.Property(e => e.RequestType).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.AiLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__AiLog__UserId__49C3F6B7");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.PasswordResetTokenId).HasName("PK__Password__16066128C5665C37");

            entity.ToTable("PasswordResetToken");

            entity.HasIndex(e => e.Token, "UQ__Password__1EB4F81765FDFB00").IsUnique();

            entity.Property(e => e.Token).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.PasswordResetTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__PasswordR__UserI__3B75D760");
        });

        modelBuilder.Entity<PremiumPackage>(entity =>
        {
            entity.HasKey(e => e.PremiumPackageId).HasName("PK__PremiumP__1868B94DA9976874");

            entity.ToTable("PremiumPackage");

            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("money");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C00A9CCBD");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D1053489C3FC0C").IsUnique();

            entity.Property(e => e.AvatarUrl).HasMaxLength(250);
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.PasswordHash).HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.RoleId);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<UserAccessory>(entity =>
        {
            entity.HasKey(e => e.UserAccessoryId).HasName("PK__UserAcce__AF1DBC1AD9B1EDE2");

            entity.ToTable("UserAccessory");

            entity.HasOne(d => d.Accessory).WithMany(p => p.UserAccessories)
                .HasForeignKey(d => d.AccessoryId)
                .HasConstraintName("FK__UserAcces__Acces__46E78A0C");

            entity.HasOne(d => d.User).WithMany(p => p.UserAccessories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserAcces__UserI__45F365D3");
        });

        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.HasKey(e => e.UserPreferenceId);
            entity.ToTable("UserPreference");

            entity.Property(e => e.PreferredPlaceTypes).HasMaxLength(200);
            entity.Property(e => e.PreferredPriceRange).HasMaxLength(50);
            entity.Property(e => e.PreferredLocation).HasMaxLength(200);
            entity.Property(e => e.GoingWith).HasMaxLength(100);
            entity.Property(e => e.Purpose).HasMaxLength(100);
            entity.Property(e => e.RequiredFeatures).HasMaxLength(300);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<UserPreference>(e => e.UserId)
                .HasConstraintName("FK_UserPreference_User");
        });

        modelBuilder.Entity<UserPremium>(entity =>
        {
            entity.HasKey(e => e.UserPremiumId).HasName("PK__UserPrem__215D48B0493ED1BF");

            entity.ToTable("UserPremium");

            entity.HasOne(d => d.PremiumPackage).WithMany(p => p.UserPremia)
                .HasForeignKey(d => d.PremiumPackageId)
                .HasConstraintName("FK__UserPremi__Premi__412EB0B6");

            entity.HasOne(d => d.User).WithMany(p => p.UserPremia)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserPremi__UserI__403A8C7D");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
