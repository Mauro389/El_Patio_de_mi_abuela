using Datos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Negocio;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Controladores
builder.Services.AddControllers();

// 2. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Restaurante El Patio de mi Abuela",
        Version = "v1",
        Description = "Sistema seguro con Roles, Usuarios, Categorías y JWT"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Escribe: Bearer TU_TOKEN"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// --- CORS CONFIG ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
// -------------------

// 1. Controladores
builder.Services.AddControllers();

// 3. Dependencias (Inyección de Servicios)
builder.Services.AddScoped<Conexion>();

// --- MÓDULO USUARIOS ---
builder.Services.AddScoped<UsuarioDatos>();
builder.Services.AddScoped<UsuarioNegocio>();

// --- MÓDULO ROLES ---
builder.Services.AddScoped<RolDatos>();
builder.Services.AddScoped<RolNegocio>();

// --- MÓDULO CATEGORÍAS 
builder.Services.AddScoped<CategoriaDatos>();
builder.Services.AddScoped<CategoriaNegocio>();

// --- FUTUROS MÓDULOS 
 builder.Services.AddScoped<ProductoDatos>();

 builder.Services.AddScoped<ProductoNegocio>();
 builder.Services.AddScoped<MesaDatos>();
 builder.Services.AddScoped<MesaNegocio>();
 builder.Services.AddScoped<OrdenDatos>();
 builder.Services.AddScoped<OrdenNegocio>();
 builder.Services.AddScoped<DetalleOrdenDatos>();
 builder.Services.AddScoped<DetalleOrdenNegocio>();
 builder.Services.AddScoped<FacturaDatos>();
 builder.Services.AddScoped<FacturaNegocio>();
 builder.Services.AddScoped<BitacoraDatos>();
 builder.Services.AddScoped<BitacoraNegocio>();
// ... otros servicios
builder.Services.AddScoped<ApiRestaurante2.Servicios.FacturaPdfService>();

// --- HELPERS ---
builder.Services.AddScoped<JwtHelper>();

// 4. Configuración JWT
var jwtConfig = builder.Configuration.GetSection("JwtConfig");
var secretKey = jwtConfig["SecretKey"];
var issuer = jwtConfig["Issuer"];
var audience = jwtConfig["Audience"];

// Diagnóstico en consola al iniciar
Console.WriteLine("========================================");
Console.WriteLine($"🔑 Llave cargada (inicio): {secretKey?.Substring(0, Math.Min(15, secretKey?.Length ?? 0))}...");
Console.WriteLine($" Issuer: {issuer}");
Console.WriteLine($"🎯 Audience: {audience}");
Console.WriteLine("========================================");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = false, // Desactivado temporalmente para pruebas
        ValidateIssuer = false,           // Desactivado temporalmente
        ValidateAudience = false,         // Desactivado temporalmente
        ValidateLifetime = false,         // Desactivado temporalmente

        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // Eventos para depuración en consola
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"❌ FALLO AUTENTICACIÓN: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("✅ TOKEN VÁLIDO (Llegó y pasó el filtro).");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// --- APLICAR CORS ---
app.UseCors("CorsApp");
// --------------------

// --- CÓDIGO ESPÍA: 
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].ToString();

    if (!string.IsNullOrEmpty(authHeader))
    {
        Console.WriteLine($"️ [ESPÍA] HEADER RECIBIDO: {authHeader.Substring(0, Math.Min(60, authHeader.Length))}...");

        if (authHeader.StartsWith("\"") || authHeader.EndsWith("\""))
            Console.WriteLine("️ [ESPÍA] ¡ALERTA! El token tiene COMILLAS alrededor.");

        if (authHeader.Contains("\n") || authHeader.Contains("\r"))
            Console.WriteLine("⚠️ [ESPÍA] ¡ALERTA! El token tiene saltos de línea.");
    }
    else
    {
        
        if (context.Request.Path.StartsWithSegments("/api/Usuarios") ||
            context.Request.Path.StartsWithSegments("/api/Roles") ||
            context.Request.Path.StartsWithSegments("/api/Categorias"))
        {
            Console.WriteLine("⚠️ [ESPÍA] NO LLEGÓ NINGÚN HEADER 'Authorization'. Revisa Swagger/Postman.");
        }
    }

    await next();
});
// -----------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



// Orden Crítico: Primero Autenticación, luego Autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();