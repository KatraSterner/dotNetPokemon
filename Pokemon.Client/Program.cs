using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pokemon.Application;
using Pokemon.Application.Services;
using Pokemon.Client.Components;
using Pokemon.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// \/ \/ \/ \/ \/ added \/ \/ \/ \/ \/
builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration
);

Console.WriteLine("--------------------------------------------------------------------------------------");
Console.WriteLine(builder.Configuration.GetConnectionString("DefaultConnection"));
Console.WriteLine("ENV: " + builder.Environment.EnvironmentName);
Console.WriteLine("APP: " + builder.Environment.ApplicationName);
Console.WriteLine("--------------------------------------------------------------------------------------");


builder.Services.AddHttpClient<PokemonService>(client =>
{
    client.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
});

// /\ /\ /\ /\ /\ added /\ /\ /\ /\ /\

builder.WebHost.UseUrls("http://localhost:5129");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
