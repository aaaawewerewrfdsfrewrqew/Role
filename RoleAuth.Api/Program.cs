using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RoleAuth.Api.Data;
using RoleAuth.Api.Middleware;
using RoleAuth.Api.Models;
using RoleAuth.Api.Options;
using RoleAuth.Api.Repositories;
using RoleAuth.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- Конфігурація JWT ----
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
          ?? throw new InvalidOperationException("Не знайдено секцію 'Jwt' у конфігурації");

if (string.IsNullOrWhiteSpace(jwt.Secret) || jwt.Secret.Length < 32)
    throw new InvalidOperationException("Jwt:Secret має містити щонайменше 32 символи");

builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.Configure<BirthdayOptions>(builder.Configuration.GetSection(BirthdayOptions.SectionName));

// ---- БД ----
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// ---- DI ----
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();

// Привітання з днем народження: фоновий сервіс
builder.Services.AddSingleton<BirthdayClock>();
builder.Services.AddScoped<IBirthdayService, BirthdayService>();
builder.Services.AddHostedService<BirthdayGreetingWorker>();

// Email: "Log" (за замовчуванням, лист пишеться в лог) або "Smtp"
if (string.Equals(builder.Configuration["Email:Provider"], "Smtp", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
else
    builder.Services.AddScoped<IEmailSender, LoggingEmailSender>();

// ---- Автентифікація / авторизація ----
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false; // залишаємо імена claim як у токені ("sub", "role", ...)
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = JwtRegisteredClaimNames.UniqueName,
            RoleClaimType = JwtService.RoleClaim
        };
    });
builder.Services.AddAuthorization();

// ---- Controllers + Swagger ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RoleAuth API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Вставте JWT токен (без слова Bearer)"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Створення БД + системні ролі + початковий адмін
await DbSeeder.SeedAsync(app.Services, app.Configuration);

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // wwwroot: сторінки reset-password.html та confirm-email.html
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
