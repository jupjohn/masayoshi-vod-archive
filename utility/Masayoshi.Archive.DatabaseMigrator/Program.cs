using Masayoshi.Archive.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthenticationDbContext>(x =>
{
    x.UseSqlite("Filename=:memory:");
    x.UseOpenIddict();
});

var app = builder.Build();
app.Run();
