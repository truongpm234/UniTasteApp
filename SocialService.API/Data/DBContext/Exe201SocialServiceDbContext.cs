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

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<CommentReaction> CommentReactions { get; set; }

    public virtual DbSet<Friendship> Friendships { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<GroupMember> GroupMembers { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<MessageAttachment> MessageAttachments { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostMedium> PostMedia { get; set; }

    public virtual DbSet<PostReaction> PostReactions { get; set; }

    public virtual DbSet<PostRestaurantTag> PostRestaurantTags { get; set; }

    public virtual DbSet<PostShare> PostShares { get; set; }

    public virtual DbSet<ReactionType> ReactionTypes { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<UserFeedback> UserFeedbacks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCC2465C86AB");

            entity.ToTable("Appointment");

            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Location).HasMaxLength(300);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Group).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__Appointme__Group__4316F928");
        });

        modelBuilder.Entity<AppointmentInvite>(entity =>
        {
            entity.HasKey(e => e.AppointmentInviteId).HasName("PK__Appointm__0E1259C03761ED62");

            entity.ToTable("AppointmentInvite");

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentInvites)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK__Appointme__Appoi__440B1D61");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comment__C3B4DFCA11B60C4A");

            entity.ToTable("Comment", tb => tb.HasTrigger("tr_Comment_Count"));

            entity.Property(e => e.Content).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_Comment_Parent");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK_Comment_Post");
        });

        modelBuilder.Entity<CommentReaction>(entity =>
        {
            entity.HasKey(e => e.CommentReactionId).HasName("PK__CommentR__609B106AADD9CAB2");

            entity.ToTable("CommentReaction");

            entity.HasIndex(e => new { e.CommentId, e.UserId }, "UQ_CommentReaction").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Comment).WithMany(p => p.CommentReactions)
                .HasForeignKey(d => d.CommentId)
                .HasConstraintName("FK_CommentReaction_Comment");

            entity.HasOne(d => d.ReactionType).WithMany(p => p.CommentReactions)
                .HasForeignKey(d => d.ReactionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommentReaction_Type");
        });

        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasKey(e => e.FriendshipId).HasName("PK__Friendsh__4D531A540D7E7862");

            entity.ToTable("Friendship");

            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__Group__149AF36AE8F990B2");

            entity.ToTable("Group");

            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.GroupAvatarUrl).HasMaxLength(250);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(e => e.GroupMemberId).HasName("PK__GroupMem__344812928EF5EFAC");

            entity.ToTable("GroupMember");

            entity.Property(e => e.Role).HasMaxLength(20);

            entity.HasOne(d => d.Group).WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__GroupMemb__Group__44FF419A");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Message__C87C0C9C650A0696");

            entity.ToTable("Message");

            entity.Property(e => e.Content).HasMaxLength(2000);

            entity.HasOne(d => d.Group).WithMany(p => p.Messages)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__Message__GroupId__45F365D3");
        });

        modelBuilder.Entity<MessageAttachment>(entity =>
        {
            entity.HasKey(e => e.MessageAttachmentId).HasName("PK__MessageA__12924620E604F136");

            entity.ToTable("MessageAttachment");

            entity.Property(e => e.FileType).HasMaxLength(50);
            entity.Property(e => e.FileUrl).HasMaxLength(250);

            entity.HasOne(d => d.Message).WithMany(p => p.MessageAttachments)
                .HasForeignKey(d => d.MessageId)
                .HasConstraintName("FK__MessageAt__Messa__46E78A0C");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__Post__AA12601835CED870");

            entity.ToTable("Post");

            entity.HasIndex(e => e.AuthorUserId, "IX_Post_AuthorUserId");

            entity.HasIndex(e => e.CreatedAt, "IX_Post_CreatedAt").IsDescending();

            entity.Property(e => e.Content).HasMaxLength(4000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsReview).HasDefaultValue(true);
            entity.Property(e => e.ShareComment).HasMaxLength(1000);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Visibility)
                .HasMaxLength(20)
                .HasDefaultValue("Public");

            entity.HasMany(d => d.Tags).WithMany(p => p.Posts)
                .UsingEntity<Dictionary<string, object>>(
                    "PostTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("FK__PostTag__TagId__60A75C0F"),
                    l => l.HasOne<Post>().WithMany()
                        .HasForeignKey("PostId")
                        .HasConstraintName("FK__PostTag__PostId__5FB337D6"),
                    j =>
                    {
                        j.HasKey("PostId", "TagId").HasName("PK__PostTag__7C45AF82A1BEA8DF");
                        j.ToTable("PostTag");
                    });
        });

        modelBuilder.Entity<PostMedium>(entity =>
        {
            entity.HasKey(e => e.PostMediaId).HasName("PK__PostMedi__75C2313476B4778D");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.MediaType)
                .HasMaxLength(20)
                .HasDefaultValue("Image");
            entity.Property(e => e.MediaUrl).HasMaxLength(500);

            entity.HasOne(d => d.Post).WithMany(p => p.PostMedia)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK_PostMedia_Post");
        });

        modelBuilder.Entity<PostReaction>(entity =>
        {
            entity.HasKey(e => e.PostReactionId).HasName("PK__PostReac__CD046ABBC8E8AE72");

            entity.ToTable("PostReaction", tb => tb.HasTrigger("tr_PostReaction_Count"));

            entity.HasIndex(e => new { e.PostId, e.UserId }, "UQ_PostReaction").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Post).WithMany(p => p.PostReactions)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK_PostReaction_Post");

            entity.HasOne(d => d.ReactionType).WithMany(p => p.PostReactions)
                .HasForeignKey(d => d.ReactionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostReaction_Type");
        });

        modelBuilder.Entity<PostRestaurantTag>(entity =>
        {
            entity.HasKey(e => e.PostRestaurantTagId).HasName("PK__PostRest__F3CAD53DC301DB88");

            entity.ToTable("PostRestaurantTag");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.GooglePlaceId).HasMaxLength(100);
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Note).HasMaxLength(250);

            entity.HasOne(d => d.Post).WithMany(p => p.PostRestaurantTags)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK_PostRestaurantTag_Post");
        });

        modelBuilder.Entity<PostShare>(entity =>
        {
            entity.HasKey(e => e.PostShareId).HasName("PK__PostShar__D336F0FF44B49B28");

            entity.ToTable("PostShare", tb => tb.HasTrigger("tr_PostShare_Count"));

            entity.HasIndex(e => new { e.OriginalPostId, e.SharerUserId }, "UQ_PostShare").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ShareComment).HasMaxLength(1000);

            entity.HasOne(d => d.OriginalPost).WithMany(p => p.PostShares)
                .HasForeignKey(d => d.OriginalPostId)
                .HasConstraintName("FK_PostShare_Post");
        });

        modelBuilder.Entity<ReactionType>(entity =>
        {
            entity.HasKey(e => e.ReactionTypeId).HasName("PK__Reaction__01E622206E8BCA22");

            entity.ToTable("ReactionType");

            entity.HasIndex(e => e.Code, "UQ__Reaction__A25C5AA7FBBD5F6E").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(30);
            entity.Property(e => e.Label).HasMaxLength(50);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__Tag__657CF9AC727CA083");

            entity.ToTable("Tag");

            entity.HasIndex(e => e.Name, "UQ__Tag__737584F6F33BD0D8").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<UserFeedback>(entity =>
        {
            entity.HasKey(e => e.UserFeedbackId);
            entity.ToTable("UserFeedback");
            entity.Property(e => e.Comment).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
