using Fcg.UserLibrary.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServicesExtensions();

var app = builder.Build();
app.AddAppConfiguration();
await app.Services.EnsureMongoIndexesAsync();

app.Run();