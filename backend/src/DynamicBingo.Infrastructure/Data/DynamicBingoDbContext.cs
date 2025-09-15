using DynamicBingo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DynamicBingo.Infrastructure.Data;

public class DynamicBingoDbContext : DbContext
{
    public DynamicBingoDbContext(DbContextOptions<DynamicBingoDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GamePlayer> GamePlayers { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<Mark> Marks { get; set; }
    public DbSet<Turn> Turns { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<AuthMagicLink> AuthMagicLinks { get; set; }
    public DbSet<Ban> Bans { get; set; }
    public DbSet<OpenChallenge> OpenChallenges { get; set; }
    public DbSet<Presence> Presences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
            entity.Property(e => e.DisplayName).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255);
            
            entity.HasMany(e => e.InitiatedFriendships)
                .WithOne(e => e.UserA)
                .HasForeignKey(e => e.UserAId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasMany(e => e.ReceivedFriendships)
                .WithOne(e => e.UserB)
                .HasForeignKey(e => e.UserBId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Word).HasMaxLength(8).IsRequired();
            entity.Property(e => e.SettingsJson).IsRequired();
            
            entity.HasOne(e => e.Creator)
                .WithMany(e => e.CreatedGames)
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Opponent)
                .WithMany()
                .HasForeignKey(e => e.OpponentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.CreatorId, e.Status });
            entity.HasIndex(e => new { e.OpponentId, e.Status });
        });

        modelBuilder.Entity<GamePlayer>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Game)
                .WithMany(e => e.Players)
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany(e => e.GamePlayers)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.GameId, e.UserId }).IsUnique();
        });

        modelBuilder.Entity<Board>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LayoutJson).IsRequired();
            
            entity.HasOne(e => e.Game)
                .WithMany(e => e.Boards)
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.GameId, e.UserId }).IsUnique();
        });

        modelBuilder.Entity<Mark>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Game)
                .WithMany(e => e.Marks)
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.MarkedByUser)
                .WithMany()
                .HasForeignKey(e => e.MarkedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.GameId, e.Number }).IsUnique();
            entity.HasIndex(e => e.TurnIndex);
        });

        modelBuilder.Entity<Turn>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Game)
                .WithMany(e => e.Turns)
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.PlayerToMove)
                .WithMany()
                .HasForeignKey(e => e.PlayerToMoveId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.GameId, e.Index }).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);
        });

        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.UserA)
                .WithMany(e => e.InitiatedFriendships)
                .HasForeignKey(e => e.UserAId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.UserB)
                .WithMany(e => e.ReceivedFriendships)
                .HasForeignKey(e => e.UserBId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.UserAId, e.UserBId }).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<AuthMagicLink>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TokenHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            
            entity.HasOne(e => e.User)
                .WithMany(e => e.MagicLinks)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);
        });

        modelBuilder.Entity<Ban>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).HasMaxLength(500).IsRequired();
            
            entity.HasOne(e => e.User)
                .WithMany(e => e.Bans)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.PreviousBan)
                .WithMany()
                .HasForeignKey(e => e.PreviousBanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.UserId, e.EndsAt });
        });

        modelBuilder.Entity<OpenChallenge>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Word).HasMaxLength(8).IsRequired();
            
            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.IsActive, e.Visibility });
        });

        modelBuilder.Entity<Presence>(entity =>
        {
            entity.HasKey(e => e.UserId);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.LastSeenAt);
        });
    }
}
