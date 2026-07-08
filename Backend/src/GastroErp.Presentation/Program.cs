using GastroErp.Application;
using GastroErp.Infrastructure;
using GastroErp.Persistence;
using GastroErp.Presentation.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog Setup
builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration)
                       .Enrich.FromLogContext()
                       .Enrich.WithMachineName()
                       .Enrich.WithThreadId()
                       .Enrich.WithEnvironmentName()
                       .WriteTo.Console()
                       .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day);
});

// 2. Add Layer Services
builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistenceServices(builder.Configuration);

// 3. Add Presentation Services
builder.Services.AddPresentationServices(builder.Configuration);

var app = builder.Build();

// 4. Initialize Database
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();
    await initializer.InitializeAsync();
    await initializer.SeedAsync();
}

// 5. Use Presentation Pipeline
app.UsePresentationPipeline();

app.Run();
