using DynamicBingo.Domain.Entities;
using DynamicBingo.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DynamicBingo.Infrastructure.Data;

public class DbSeeder
{
    private readonly DynamicBingoDbContext _context;

    public DbSeeder(DynamicBingoDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await _context.Database.EnsureCreatedAsync();

        if (await _context.Users.AnyAsync())
            return;

        var user1 = User.CreateRegistered("alice@example.com", "Alice");
        var user2 = User.CreateRegistered("bob@example.com", "Bob");
        var guest1 = User.CreateGuest("GuestPlayer");

        _context.Users.AddRange(user1, user2, guest1);
        await _context.SaveChangesAsync();

        var presence1 = Presence.Create(user1.Id, PresenceStatus.Available);
        var presence2 = Presence.Create(user2.Id, PresenceStatus.Available);
        var presence3 = Presence.Create(guest1.Id, PresenceStatus.Available);

        _context.Presences.AddRange(presence1, presence2, presence3);

        var friendship = Friendship.Create(user1.Id, user2.Id);
        friendship.Accept();
        _context.Friendships.Add(friendship);

        var challenge = OpenChallenge.Create(
            user1.Id,
            ChallengeVisibility.Public,
            "BINGO",
            FillMode.Random,
            StarterChoice.Creator
        );
        _context.OpenChallenges.Add(challenge);

        var game = Game.Create(
            "HELLO",
            user1.Id,
            user2.Id,
            new Domain.ValueObjects.GameSettings
            {
                FillMode = FillMode.Random,
                StarterChoice = StarterChoice.Creator
            }
        );

        _context.Games.Add(game);

        var board1 = Board.Create(game.Id, user1.Id, FillMode.Sequential);
        var board2 = Board.Create(game.Id, user2.Id, FillMode.Sequential);
        
        _context.Boards.AddRange(board1, board2);

        var gamePlayer1 = GamePlayer.Create(game.Id, user1.Id, true);
        var gamePlayer2 = GamePlayer.Create(game.Id, user2.Id, false);
        
        _context.GamePlayers.AddRange(gamePlayer1, gamePlayer2);

        await _context.SaveChangesAsync();
    }
}
