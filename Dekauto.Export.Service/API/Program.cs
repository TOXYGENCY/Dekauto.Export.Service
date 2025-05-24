using System.Text;
using Dekauto.Export.Service.Domain.Interfaces;
using Dekauto.Export.Service.Domain.Services;
using Dekauto.Export.Service.Domain.Services.Metric;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    //Serilog
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentUserName()
        .WriteTo.Console(
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        )
        .WriteTo.File("logs/Dekauto-Students-.log",
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: 10_485_760,
            retainedFileCountLimit: 31,
            encoding: Encoding.UTF8)
        .CreateLogger();

    builder.Host.UseSerilog();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Export Service", Version = "v1" });

        c.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Basic",
            In = ParameterLocation.Header,
            Description = "Basic Authorization header using the Bearer scheme."
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Basic"
                }
            },
            new string[] {}
        }
        });
    });
    builder.Services.AddTransient<IStudentsService, StudentsService>();
    builder.Services.AddSingleton<IRequestMetricsService, RequestMetricsService>();

    if (Boolean.Parse(builder.Configuration["UseEndpointAuth"] ?? "true"))
    {
        // Аутентификация
        builder.Services
        .AddAuthentication("Basic")
        .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
            "Basic",
            options => { });

        // Авторизация
        builder.Services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder("Basic")
                .RequireAuthenticatedUser()
                .Build();
        });
    }
    else
    {
        // Заглушка политик доступа, если авторизация выключена
        builder.Services.AddAuthorizationBuilder()
        .SetDefaultPolicy(new AuthorizationPolicyBuilder()
        .RequireAssertion(_ => true) // Всегда разрешаем доступ
        .Build());
    }

    var app = builder.Build();

    // Configure the HTTP request pipeline.

    // Явно указываем порты (для Docker)
    app.Urls.Add("http://*:5505");

    if (app.Environment.IsDevelopment())
    {
        Log.Warning("Development version of the application is started. Swagger activation...");
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Включаем https, если указано в конфиге
    if (Boolean.Parse(app.Configuration["UseHttps"] ?? "false"))
    {
        app.Urls.Add("https://*:5506");
        app.UseHttpsRedirection();
        Log.Information("Enabled HTTPS.");
    }
    else
    {
        Log.Warning("Disabled HTTPS.");
    }

    if (Boolean.Parse(app.Configuration["UseEndpointAuth"] ?? "true"))
    {
        // Аутентификация (JWT)
        app.UseAuthentication();

        // Авторизация (защита контроллеров через [Authorize])
        app.UseAuthorization();
    }
    else
    {
        Log.Warning("Disabled all endpoint authorization.");
    }

    app.MapControllers();

    app.UseMetricsMiddleware(); // Метрики

    Log.Information("Application startup...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unexpected Fatal error has occurred in the application.");
}
finally
{
    Log.CloseAndFlush();
}