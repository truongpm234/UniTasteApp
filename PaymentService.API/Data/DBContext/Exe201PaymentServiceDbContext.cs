using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PaymentService.API.Models.Entity;

namespace PaymentService.API.Data.DBContext;

public partial class Exe201PaymentServiceDbContext : DbContext
{
    public Exe201PaymentServiceDbContext()
    {
    }

    public Exe201PaymentServiceDbContext(DbContextOptions<Exe201PaymentServiceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    public virtual DbSet<Purchase> Purchases { get; set; }

    public virtual DbSet<UserMiniGamePlay> UserMiniGamePlays { get; set; }

    public virtual DbSet<UserVoucher> UserVouchers { get; set; }

    public virtual DbSet<UserWallet> UserWallets { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

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
        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.PaymentTransactionId).HasName("PK__PaymentT__C22AEFE01D784FA7");

            entity.ToTable("PaymentTransaction");

            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.ReferenceId).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TransactionType).HasMaxLength(30);
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(e => e.PurchaseId).HasName("PK__Purchase__6B0A6BBED8B87493");

            entity.ToTable("Purchase");

            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.PurchaseType).HasMaxLength(30);
        });

        modelBuilder.Entity<UserMiniGamePlay>(entity =>
        {
            entity.HasKey(e => e.UserMiniGamePlayId).HasName("PK__UserMini__D90AF9C3DBD9C4C3");

            entity.ToTable("UserMiniGamePlay");

            entity.Property(e => e.Result).HasMaxLength(100);

            entity.HasOne(d => d.RewardVoucher).WithMany(p => p.UserMiniGamePlays)
                .HasForeignKey(d => d.RewardVoucherId)
                .HasConstraintName("FK__UserMiniG__Rewar__4316F928");
        });

        modelBuilder.Entity<UserVoucher>(entity =>
        {
            entity.HasKey(e => e.UserVoucherId).HasName("PK__UserVouc__8017D4993D091B2F");

            entity.ToTable("UserVoucher");

            entity.HasOne(d => d.Voucher).WithMany(p => p.UserVouchers)
                .HasForeignKey(d => d.VoucherId)
                .HasConstraintName("FK__UserVouch__Vouch__403A8C7D");
        });

        modelBuilder.Entity<UserWallet>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserWall__1788CC4C936716B0");

            entity.ToTable("UserWallet");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Balance).HasColumnType("money");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.VoucherId).HasName("PK__Voucher__3AEE792123E470B5");

            entity.ToTable("Voucher");

            entity.HasIndex(e => e.Code, "UQ__Voucher__A25C5AA79E11BD99").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.DiscountType).HasMaxLength(20);
            entity.Property(e => e.DiscountValue).HasColumnType("money");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
