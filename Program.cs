using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using PizzaManagement.AI.Models;
using PizzaManagement.AI.Predictors;
using PizzaManagement.AI.Trainners;
using PizzaManagement.Data;
using PizzaManagement.service;
using PizzaManagement.Service;
using PizzaManagement.MiddleWare;
using Serilog;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/pizza_api_.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
try
{
    Log.Information("STARTING UP PIZZA MANAGEMENT API SYSTEM");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(Options =>
    {
        Options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Bearer {token}"
        });
        Options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new String []{}
        }
    });
    });

    builder.Services.AddDbContext<AppDbContext>(Options => Options.UseSqlServer(connectionString));

    builder.Services.AddAuthentication(options => {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
        .AddJwtBearer(options => {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name
            };
        });

    builder.Services.AddSingleton<QTable>();
    builder.Services.AddSingleton<ActionPredictor>();
    builder.Services.AddSingleton<AIService>();
    builder.Services.AddTransient<MLTrainer>();
    builder.Services.AddScoped<RecommendationService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddExceptionHandler<ExceptionHandling>();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    app.UseExceptionHandler();

    app.UseSerilogRequestLogging();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pizza API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.UseStaticFiles();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API error: Unable to start!");
}
finally
{
    Log.CloseAndFlush();
}