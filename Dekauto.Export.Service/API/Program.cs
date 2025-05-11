using Dekauto.Export.Service.Domain.Interfaces;
using Dekauto.Export.Service.Domain.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
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

// јутентификаци€
builder.Services
    .AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
        "Basic",
        options => { });

// јвторизаци€
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder("Basic")
        .RequireAuthenticatedUser()
        .Build();
});


var app = builder.Build();

// Configure the HTTP request pipeline.

// явно указываем порты (дл€ Docker)
app.Urls.Add("http://*:5505");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
} else
{
    app.Urls.Add("https://*:5506");
    app.UseHttpsRedirection(); // без https редиректа в dev-версии
}

// 1. јутентификаци€
app.UseAuthentication();

// 2. јвторизаци€
app.UseAuthorization();

app.MapControllers();

app.Run();
