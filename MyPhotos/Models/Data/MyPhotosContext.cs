using Microsoft.EntityFrameworkCore;

namespace MyPhotos.Models.Data;

#pragma warning disable CS1591
public partial class MyPhotosContext : DbContext
{
    public MyPhotosContext(DbContextOptions<MyPhotosContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public virtual DbSet<Friendship> Friendships { get; set; }

    public virtual DbSet<FriendshipStatus> FriendshipStatuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserImage> UserImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Friendsh__3214EC07DB30BE7B");

            entity.ToTable("Friendship");

            entity.HasOne(d => d.RelatedUser).WithMany(p => p.FriendshipRelatedUsers)
                .HasForeignKey(d => d.RelatedUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FS_FriendID");

            entity.HasOne(d => d.RelatingUser).WithMany(p => p.FriendshipRelatingUsers)
                .HasForeignKey(d => d.RelatingUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FS_UserID");

            entity.HasOne(d => d.Status).WithMany(p => p.Friendships)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FS_StatusId");
        });

        modelBuilder.Entity<FriendshipStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Friendsh__3214EC07C03D9750");

            entity.ToTable("FriendshipStatus");

            entity.HasIndex(e => e.Status, "UQ__Friendsh__3A15923F7C1BCA88").IsUnique();

            entity.Property(e => e.Status).HasMaxLength(32);

            entity.HasData(new FriendshipStatus { Id = 1, Status = "ignored" }, new FriendshipStatus { Id = 2, Status = "friend" });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC07B731EBAB");

            entity.ToTable("User");

            entity.HasIndex(e => e.Login, "UQ__User__5E55825B94043D0D").IsUnique();

            entity.Property(e => e.Login).HasMaxLength(256);
            entity.Property(e => e.Nickname).HasMaxLength(32);
            entity.Property(e => e.Password).HasMaxLength(256);

            entity.HasMany(e => e.UserImages);
        });

        modelBuilder.Entity<UserImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserImag__3214EC0793A6427E");

            entity.HasIndex(e => new { e.UserId, e.Image }, "UQ_Images").IsUnique();

            entity.Property(e => e.Image).HasMaxLength(256);

            entity.HasOne(d => d.User).WithMany(p => p.UserImages)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Images_UserId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
#pragma warning restore CS1591