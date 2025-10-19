using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PaymentService.API.Models.Entity;

namespace PaymentService.API.Data.DBContext
{
    public partial class Exe201PaymentServiceDbContext : DbContext
    {
        public Exe201PaymentServiceDbContext(DbContextOptions<Exe201PaymentServiceDbContext> options)
            : base(options)
        {
        }

        // ===== DbSet =====
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<UserMiniGamePlay> UserMiniGamePlays { get; set; }
        public DbSet<UserVoucher> UserVouchers { get; set; }
        public DbSet<ServicePackage> ServicePackages { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }



        private string GetConnectionString()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            return config["ConnectionStrings:DefaultConnectionStringDB"];
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(GetConnectionString());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ===== PaymentTransaction =====
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(e => e.PaymentTransactionId)
                      .HasName("PK__PaymentTransaction");

                entity.ToTable("PaymentTransaction");

                entity.Property(e => e.Amount).HasColumnType("money");
                entity.Property(e => e.ReferenceId).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.TransactionType).HasMaxLength(30);
            });

            // ===== Purchase =====
            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.HasKey(e => e.PurchaseId)
                      .HasName("PK__Purchase");

                entity.ToTable("Purchase");

                entity.Property(e => e.Amount).HasColumnType("money");
                entity.Property(e => e.Description).HasMaxLength(250);
                entity.Property(e => e.PurchaseType).HasMaxLength(30);

                entity.HasOne(d => d.ServicePackage)
                      .WithMany(p => p.Purchases)
                      .HasForeignKey(d => d.ServicePackageId)
                      .HasConstraintName("FK_Purchase_ServicePackage");
            });

            // ===== UserMiniGamePlay =====
            modelBuilder.Entity<UserMiniGamePlay>(entity =>
            {
                entity.HasKey(e => e.UserMiniGamePlayId)
                      .HasName("PK__UserMiniGamePlay");

                entity.ToTable("UserMiniGamePlay");

                entity.Property(e => e.Result).HasMaxLength(100);

                entity.HasOne(d => d.RewardVoucher)
                      .WithMany(p => p.UserMiniGamePlays)
                      .HasForeignKey(d => d.RewardVoucherId)
                      .HasConstraintName("FK_UserMiniGamePlay_Voucher");
            });

            // ===== UserVoucher =====
            modelBuilder.Entity<UserVoucher>(entity =>
            {
                entity.HasKey(e => e.UserVoucherId)
                      .HasName("PK__UserVoucher");

                entity.ToTable("UserVoucher");

                entity.HasOne(d => d.Voucher)
                      .WithMany(p => p.UserVouchers)
                      .HasForeignKey(d => d.VoucherId)
                      .HasConstraintName("FK_UserVoucher_Voucher");
            });
            //=====Voucher=====
            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.HasKey(e => e.VoucherId);
                entity.ToTable("Voucher");
                entity.Property(e => e.Code).HasMaxLength(50);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(250);
                entity.Property(e => e.DiscountType).HasMaxLength(20);
            });

            // ===== ServicePackage =====
            modelBuilder.Entity<ServicePackage>(entity =>
            {
                entity.HasKey(e => e.ServicePackageId)
                      .HasName("PK__ServicePackage");

                entity.ToTable("ServicePackage");

                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(250);
                entity.Property(e => e.Price).HasColumnType("money");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
