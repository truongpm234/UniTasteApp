using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ReviewService.API.Models.Entity;

namespace ReviewService.API.Data.DBContext;

public partial class Exe201ReviewServiceDbContext : DbContext
{
    public Exe201ReviewServiceDbContext()
    {
    }

    public Exe201ReviewServiceDbContext(DbContextOptions<Exe201ReviewServiceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentInvite> AppointmentInvites { get; set; }

    public virtual DbSet<Friendship> Friendships { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<GroupMember> GroupMembers { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<MessageAttachment> MessageAttachments { get; set; }

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
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCC20AFAE61D");

            entity.ToTable("Appointment");

            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Location).HasMaxLength(300);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Group).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__Appointme__Group__44FF419A");
        });

        modelBuilder.Entity<AppointmentInvite>(entity =>
        {
            entity.HasKey(e => e.AppointmentInviteId).HasName("PK__Appointm__0E1259C0413E6637");

            entity.ToTable("AppointmentInvite");

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentInvites)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK__Appointme__Appoi__47DBAE45");
        });

        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasKey(e => e.FriendshipId).HasName("PK__Friendsh__4D531A5428321A8B");

            entity.ToTable("Friendship");

            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__Group__149AF36A3469D287");

            entity.ToTable("Group");

            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.GroupAvatarUrl).HasMaxLength(250);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(e => e.GroupMemberId).HasName("PK__GroupMem__34481292319C6251");

            entity.ToTable("GroupMember");

            entity.Property(e => e.Role).HasMaxLength(20);

            entity.HasOne(d => d.Group).WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__GroupMemb__Group__3F466844");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Message__C87C0C9C26E45D4E");

            entity.ToTable("Message");

            entity.Property(e => e.Content).HasMaxLength(2000);

            entity.HasOne(d => d.Group).WithMany(p => p.Messages)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__Message__GroupId__398D8EEE");
        });

        modelBuilder.Entity<MessageAttachment>(entity =>
        {
            entity.HasKey(e => e.MessageAttachmentId).HasName("PK__MessageA__12924620EA278E1D");

            entity.ToTable("MessageAttachment");

            entity.Property(e => e.FileType).HasMaxLength(50);
            entity.Property(e => e.FileUrl).HasMaxLength(250);

            entity.HasOne(d => d.Message).WithMany(p => p.MessageAttachments)
                .HasForeignKey(d => d.MessageId)
                .HasConstraintName("FK__MessageAt__Messa__4222D4EF");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
