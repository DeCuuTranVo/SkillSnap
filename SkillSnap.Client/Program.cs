using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SkillSnap.Client;
using SkillSnap.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5217/") // Your API URL
});

// Register services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PortfolioUserService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<SkillService>();

var app = builder.Build();

await app.RunAsync();
