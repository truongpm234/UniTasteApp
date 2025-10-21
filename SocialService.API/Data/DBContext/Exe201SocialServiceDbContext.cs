using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SocialService.API.Models.Entity;

namespace SocialService.API.Data.DBContext;

public partial class Exe201SocialServiceDbContext : DbContext
{
    public Exe201SocialServiceDbContext()
    {
    }

    public Exe201SocialServiceDbContext(DbContextOptions<Exe201SocialServiceDbContext> options)
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
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCC2F05255BD");

            entity.ToTable("Appointment");

            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Location).HasMaxLength(300);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Group).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__Appointme__Group__440B1D61");
        });

        modelBuilder.Entity<AppointmentInvite>(entity =>
        {
            entity.HasKey(e => e.AppointmentInviteId).HasName("PK__Appointm__0E1259C0C9B9A988");

            entity.ToTable("AppointmentInvite");

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentInvites)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK__Appointme__Appoi__46E78A0C");
        });

        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasKey(e => e.FriendshipId).HasName("PK__Friendsh__4D531A54C18CB316");

            entity.ToTable("Friendship");

            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__Group__149AF36ADAF9C3F0");

            entity.ToTable("Group");

            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.GroupAvatarUrl).HasMaxLength(250);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(e => e.GroupMemberId).HasName("PK__GroupMem__3448129277B09D7D");

            entity.ToTable("GroupMember");

            entity.Property(e => e.Role).HasMaxLength(20);

            entity.HasOne(d => d.Group).WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__GroupMemb__Group__3B75D760");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Message__C87C0C9C776ADA57");

            entity.ToTable("Message");

            entity.Property(e => e.Content).HasMaxLength(2000);

            entity.HasOne(d => d.Group).WithMany(p => p.Messages)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__Message__GroupId__3E52440B");
        });

        modelBuilder.Entity<MessageAttachment>(entity =>
        {
            entity.HasKey(e => e.MessageAttachmentId).HasName("PK__MessageA__12924620307BB812");

            entity.ToTable("MessageAttachment");

            entity.Property(e => e.FileType).HasMaxLength(50);
            entity.Property(e => e.FileUrl).HasMaxLength(250);

            entity.HasOne(d => d.Message).WithMany(p => p.MessageAttachments)
                .HasForeignKey(d => d.MessageId)
                .HasConstraintName("FK__MessageAt__Messa__412EB0B6");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
