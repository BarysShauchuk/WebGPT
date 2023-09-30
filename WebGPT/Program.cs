using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Azure.Core;
using Microsoft.Extensions.Azure;
using WebGPT.Data;
using WebGPT.Data.AiService;
using WebGPT.Data.ChatService;
using WebGPT.Data.MarkdownService;
using WebGPT.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<IChatService, ChatService>();
builder.Services.AddSingleton<IAiService, FakeAiService>();
builder.Services.AddSingleton<IMarkdownService, GitLabMarkdownService>();

builder.Services.AddHttpClient();
builder.Services.ConfigureNamedOptions<ApiSettings>(
    builder.Configuration.GetSection("ApiOptions"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
