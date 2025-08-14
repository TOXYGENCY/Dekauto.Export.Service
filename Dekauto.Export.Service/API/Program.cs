using Dekauto.Export.Service.Domain.Interfaces;
using Dekauto.Export.Service.Domain.Services;
using Dekauto.Export.Service.Domain.Services.Metric;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Loki;
using System.Text;

var tempOutputTemplate = "[EXPORT STARTUP LOGGER] {Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
// Временные логгер Serilog для этапа до создания билдера
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal) // Только критические ошибки из Microsoft-сервисов
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: tempOutputTemplate,
        restrictedToMinimumLevel: LogEventLevel.Information
    )
    .WriteTo.File(
        "logs/Export-startup-log.txt",
        outputTemplate: tempOutputTemplate,
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Warning
    )
    .CreateBootstrapLogger(); // временный логгер

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    // Полноценная настройка Serilog логгера (из конфига)
    builder.Host.UseSerilog((builderContext, serilogConfig) =>
    {
        serilogConfig
            .ReadFrom.Configuration(builderContext.Configuration)
            // Ручная настройка Loki
            .WriteTo.Loki(new LokiSinkConfigurations()
            {
                Url = new Uri("http://loki:3100"),
                Labels =
                [
                    new LokiLabel("service_name", "dekauto_export"),
                    new LokiLabel("app","dekauto_full"),
                    new LokiLabel("env",
                    builderContext.HostingEnvironment.IsDevelopment() ? "dev" : "prod")
                ]
            });
    });

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
        // Аутентификация (Проверка русского)
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

    app.MapMetrics();
    app.UseMetricsMiddleware(); // Метрики

    Log.Information("Application startup...");
    app.Run();
}
catch (Exception ex)
{
    // В случае краха приложения при запуске пытаемся отправить логи:
    // 1. Запись в файл и консоль контейнера
    Log.Fatal(ex, "An unexpected Fatal error has occurred in the application.");
    try
    {
        // 2. Попытка отправить критическую ошибку в Loki
        using var tempLogger = new LoggerConfiguration()
            .WriteTo.Loki(new LokiSinkConfigurations()
            {
                Url = new Uri("http://loki:3100"),
                Labels =
                [
                    new LokiLabel("service_name", "dekauto_export"),
                    new LokiLabel("app","dekauto_full")
                ]
            })
            .CreateLogger();
        tempLogger.Fatal(ex, "[EXPORT TEMPORARY FATAL LOGGER] Application startup failed");
    }
    catch (Exception lokiEx)
    {
        Log.Warning(lokiEx, "Failed to send log to Loki");
    }
}
finally
{
    Log.CloseAndFlush();
}