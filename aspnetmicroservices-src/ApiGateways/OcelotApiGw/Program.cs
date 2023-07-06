using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json");
builder.Logging.AddConfiguration(configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddOcelot().AddCacheManager(settings => settings.WithDictionaryHandle());
//builder.Services.AddSwaggerForOcelot();

var app = builder.Build();
//app.UseSwaggerForOcelotUI();
app.UseOcelot().Wait();
app.MapGet("/", () => "Hello World!");

app.Run();
