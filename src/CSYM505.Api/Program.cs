using CSYM505.Api.Controllers.ImportDataApi;
using CSYM505.ORM.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IImportDataService, ImportDataService>();

var connectionString = builder.Configuration.GetConnectionString("Sqlite")
    ?? throw new InvalidOperationException("SQLite connection string was not found.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
