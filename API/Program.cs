using API.LogEventEnrichers;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true) // props: path, reloadOnChange
                                                   // reload if below configuration changes (,true) 
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build()
    )
    .CreateLogger();

/* OT: USE ONLY FOR TESTING IF ANY ISSUES READING FROM APPSETTINGS, BELOW CONFIG IS
 * AN EXTENSION METHOD REPRESENTATION OF THE BASE STRUCTURE LAID  IN CONFIGURATION FILE, JUST
 * COPY THE CODE BELOW AND LEAVE COMMENTED AREA AS IS FOR TESTING.

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        "logs/Serilog-FileSink.json",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [LC:{Level:u3}/UC:{Level:w3}] {Message:lj} {NewLine} {Properties:j}{NewLine}{Exception}"
    )
    .Enrich.With(new ThreadIDEnricher()) // order does-not matter, above and below sinks will have ThreadID property
        .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
        .Enrich.WithProperty("Version", "6.9.6")
            .WriteTo.Seq("http://192.168.100.115:5341")
    .CreateLogger();
*/

Serilog.Debugging.SelfLog.Enable(Console.Error);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseSerilog();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
