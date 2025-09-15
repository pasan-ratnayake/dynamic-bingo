using DynamicBingo.Application.Interfaces;
using DynamicBingo.Application.Services;
using DynamicBingo.Infrastructure.Data;
using DynamicBingo.Infrastructure.Repositories;
using DynamicBingo.Infrastructure.Services;
using DynamicBingo.WebApi.Hubs;
using DynamicBingo.WebApi.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DynamicBingoDbContext>(options =>
    options.UseInMemoryDatabase("DynamicBingo"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();
builder.Services.AddScoped<IOpenChallengeRepository, OpenChallengeRepository>();
builder.Services.AddScoped<IBanRepository, BanRepository>();
builder.Services.AddScoped<IAuthMagicLinkRepository, AuthMagicLinkRepository>();

builder.Services.AddScoped<ITimeProvider, DynamicBingo.Infrastructure.Services.TimeProvider>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IRealtimeTransport, SignalRRealtimeTransport>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<GameEngineService>();

builder.Services.AddAuthentication("Bearer")
    .AddScheme<AuthenticationSchemeOptions, SimpleAuthenticationHandler>("Bearer", options => { });

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<DynamicBingoDbContext>();
        var seeder = new DbSeeder(context);
        await seeder.SeedAsync();
    }
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<DynamicBingo.WebApi.Hubs.LobbyHub>("/hubs/lobby").RequireAuthorization();
app.MapHub<DynamicBingo.WebApi.Hubs.GameHub>("/hubs/game").RequireAuthorization();

app.Run();
