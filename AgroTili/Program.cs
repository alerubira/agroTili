using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AgroTili.Models;
using AgroTili.Services;  // <--- esto es necesario
using DotNetEnv;
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Cargar variables del archivo .env
Env.Load("claves.env");

builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailService>();//una instancia por request
//builder.Services.AddScoped<SeguridadService>();
builder.Services.AddSingleton<SeguridadService>();//unica instancia para toda la app
builder.Services.AddDbContext<DataContext>(options =>  // Contexto de base de datos
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    
    .AddJwtBearer(options =>//la api web valida con token
    {
        var secreto = configuration["TokenAuthentication:SecretKey"];
        if (string.IsNullOrEmpty(secreto))
            throw new Exception("Falta configurar TokenAuthentication:Secret");
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["TokenAuthentication:Issuer"],
            ValidAudience = configuration["TokenAuthentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(secreto)),
        };
    });

 
//builder.Services.AddControllers(); // ← agrega soporte para controladores API
builder.Services.AddEndpointsApiExplorer(); // ← necesario para Swagger
var app = builder.Build();
//app.MapStaticAssets();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
    //.WithStaticAssets();


app.Run();
