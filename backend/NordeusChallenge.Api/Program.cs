using NordeusChallenge.Api.Models;
using NordeusChallenge.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<MonsterMoveService>();
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();

app.MapGet("/api/run-config", () =>
    Results.Ok(GameDataService.GetRunConfig()))
    .WithName("GetRunConfig");

app.MapPost("/api/monster-move", (BattleState state, MonsterMoveService monsterMoveService) =>
{
    var monster = GameDataService.GetMonster(state.MonsterId);
    if (monster is null)
        return Results.NotFound($"Monster '{state.MonsterId}' not found.");

    var move = monsterMoveService.PickMove(monster, state);
    return Results.Ok(new MonsterMoveResponse(move));
})
.WithName("GetMonsterMove");

app.Run();
