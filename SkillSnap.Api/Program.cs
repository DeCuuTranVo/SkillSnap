using SkillSnap.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add all application services
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwaggerDocumentation();

app.UseHttpsRedirection();
app.UseCors("AllowClient");
app.MapControllers();

app.Run();