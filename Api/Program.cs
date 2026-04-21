using System.Text;
using DotNetEnv;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Application.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.UnitOfWork;
using AutoMapper;
using Domain.Entities;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

var mysqlConnection = Environment.GetEnvironmentVariable("MYSQL_CONNECTION");
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(mysqlConnection!));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                ctx.Token = ctx.Request.Cookies["token"];
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(frontendUrl!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

// Seeder
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!context.Roles.Any())
    {
        context.Roles.AddRange(
            new Role { Name = "Admin" },
            new Role { Name = "User" }
        );
        await context.SaveChangesAsync();
    }

    if (!context.Categories.Any())
    {
        context.Categories.AddRange(
            new Category { Name = "Alimentación" },
            new Category { Name = "Transporte" },
            new Category { Name = "Salud" },
            new Category { Name = "Servicios" }
        );
        await context.SaveChangesAsync();
    }

    if (!context.Users.Any())
    {
        var admin = new User
        {
            FullName = "Administrador",
            Email = "admin@gastos.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123*")
        };
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        var adminRole = context.Roles.First(r => r.Name == "Admin");
        context.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });
        await context.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{

}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();