using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using CurrencyConverter.Api;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console());

// Add services (implemented in StartupExtensions)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Application & infrastructure wiring
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);

var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        // Swagger not configured (Swashbuckle not referenced). Add if desired.
    }

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();
