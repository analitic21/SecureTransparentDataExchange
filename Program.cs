using AutoMapper;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SecureTransparentDataExchange.AI.Detection;
using SecureTransparentDataExchange.AI.Prediction;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Data.Seeding;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.API.LegalEntities;
using SecureTransparentDataExchange.Models.Crypto;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.identity;
using SecureTransparentDataExchange.Models.Repositories;
using SecureTransparentDataExchange.Options;
using SecureTransparentDataExchange.Repositories;
using SecureTransparentDataExchange.Services;
using SecureTransparentDataExchange.Services.Blockchain;
using SecureTransparentDataExchange.Services.Interfaces;
using SecureTransparentDataExchange.Services.Security;
using SecureTransparentDataExchange.Services.StateMachines;
using Stripe;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using InvoiceService = SecureTransparentDataExchange.Services.InvoiceService;
using RecyclableMemoryStreamManager = Microsoft.IO.RecyclableMemoryStreamManager;
using SecureTransparentDataExchange.Services.Realtime;
using SecureTransparentDataExchange.Services.Background;
using SecureTransparentDataExchange.Services.AI;

var builder = WebApplication.CreateBuilder(args);

#region 1. Configuration

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

var configuration = builder.Configuration;

builder.Services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
builder.Services.Configure<SmsSettings>(configuration.GetSection("SmsService"));
builder.Services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
builder.Services.AddScoped<SecureTransparentDataExchange.Services.StripeWebhookService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<RecyclableMemoryStreamManager>();

// ✅ IMPORTANT: register IoTWebSocketManager ONLY ONCE
builder.Services.AddSingleton<IoTWebSocketManager>();

#endregion

#region 2. Database & Identity

var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
    }));

builder.Services
    .AddIdentity<Login, Role>(opts =>
    {
        opts.Password.RequireDigit = true;
        opts.Password.RequireLowercase = true;
        opts.Password.RequireUppercase = true;
        opts.Password.RequiredLength = 6;
        opts.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
    {
        OnRedirectToLogin = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        },

        OnRedirectToAccessDenied = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
            ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        }
    };
});

#endregion



// JWT runtime config holder in memory (loaded once after Build)
#region 3. JWT Authentication (DB-based, FIXED)

builder.Services.AddSingleton<JwtRuntimeConfigHolder>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,

            ValidIssuer = "temp",
            ValidAudience = "temp",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("temp-temp-temp-temp-temp-temp-temp-temp")),

            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            ip,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });

    options.AddPolicy("api-write", context =>
    {
        var userId =
            context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            userId,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });

    options.AddPolicy("twofa", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            ip,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(2),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });

    options.AddPolicy("iot", context =>
    {
        var key =
            context.User?.FindFirst("tracking")?.Value
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });
});
#endregion


#region 4. CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCors", policy =>
    {
        policy
            .WithOrigins(
                "https://idlogichain.com",
                "https://www.idlogichain.com",
                "https://api.idlogichain.com",
                "https://billing.idlogichain.com",
                "https://support.idlogichain.com",
                "https://reply.idlogichain.com",
                "https://admin.idlogichain.com",
                "http://localhost:5173",
                 "http://localhost:3000"

            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

#endregion

#region 5. Swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Improvement of security and transparency in data exchange procedures applying blockchain and identity based encryption technologies ",
        Version = "v1"
    });

    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    opts.AddSecurityRequirement(new OpenApiSecurityRequirement
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

#endregion

#region 6. MVC

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

#endregion

#region 7. Domain Services (ALL YOUR SERVICES ARE SAVED)

builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<AddressService>();
builder.Services.AddScoped<AuditLogService>();

builder.Services.AddScoped<IBlockchainLogRepository, BlockchainLogRepository>();
builder.Services.AddScoped<BlockchainLogService>();
builder.Services.AddScoped<BlockchainValidationService>();
builder.Services.AddScoped<LedgerFacade>();
builder.Services.AddScoped<CargoTypeService>();
builder.Services.AddScoped<CityService>();
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<CountryService>();
builder.Services.AddScoped<DeliveryHistoryService>();
builder.Services.AddScoped<DeliveryPriceService>();
builder.Services.AddScoped<DeliveryRouteService>();
builder.Services.AddScoped<DemandForecastService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<EncryptionService>();
builder.Services.AddScoped<FeedbackService>();
builder.Services.AddScoped<IoTDeviceService>();
builder.Services.AddScoped<DeliveryDestinationResolver>();
builder.Services.AddScoped<EncryptedKeyService>();
builder.Services.AddScoped<ILegalEntityService,LegalEntityService>();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<LoginAgreementArticleService>();
builder.Services.AddScoped<LoginAgreementConsentService>();
builder.Services.AddScoped<LoginAgreementService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<PackageService>();
builder.Services.AddScoped<ParcelService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<PaymentStatusManager>();
builder.Services.AddScoped<SeedDataService>();
builder.Services.AddScoped<PipelineTrainingService>();
builder.Services.AddScoped<PostalCodeService>();
builder.Services.AddScoped<QRCodeService>();
builder.Services.AddScoped<QRCodeStorageService>();
builder.Services.AddScoped<RandomNumberGeneratorService>();
builder.Services.AddScoped<RegistrationService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<RouteOptimizer>();
builder.Services.AddScoped<RouteService>();
builder.Services.AddScoped<SmsService>();
builder.Services.AddScoped<ShipmentService>();
builder.Services.AddScoped<ShipmentStatusManager>();
builder.Services.AddScoped<JwtSettingService>();
builder.Services.AddScoped<SystemLogService>();
builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<TwoFactorAuthService>();
builder.Services.AddScoped<TrackingNumberService>();
builder.Services.AddScoped<WeatherForecastService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IOrderNumberService, OrderNumberService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddHostedService<IoTOfflineDetectionHostedService>();
builder.Services.AddSingleton<IoTMovementService>();
builder.Services.AddSingleton<MovementAnalyticsService>();
builder.Services.AddSingleton<MovementPredictionService>();
builder.Services.AddHttpClient<SmsService>();
builder.Services.AddScoped<AnomalyPredictionService>();
builder.Services.AddScoped<AnomalyDetectionService>();
builder.Services.AddScoped<PasswordHasher<Login>>();
builder.Services.AddScoped<StripePaymentService>();
builder.Services.AddScoped<InvoicePdfService>();
builder.Services.AddScoped<ShipmentStateMachine>();
builder.Services.AddScoped<DeviceTokenService>();
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddScoped<SmartDeliveryService>();
builder.Services.AddSingleton<DeliveryEtaService>();
builder.Services.AddScoped<GeoFenceService>();
builder.Services.AddScoped<GeoFenceSecurityService>();


var stripeSecret = builder.Configuration["Stripe:SecretKey"];
if (!string.IsNullOrWhiteSpace(stripeSecret))
{
    StripeConfiguration.ApiKey = stripeSecret;
}

// ✅ IoT authorization policy based on claim (no DB change)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IotDevice", policy =>
        policy.RequireClaim("type", "iot-device"));
});

#endregion

#region 8. AutoMapper

builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);

#endregion

#region 9. Forwarded headers

builder.Services.Configure<ForwardedHeadersOptions>(opts =>
{
    opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    opts.KnownNetworks.Clear();
    opts.KnownProxies.Clear();
});

#endregion

#region 10. Build

var app = builder.Build();

#endregion

#region 10.1 LOAD JWT FROM DB ONCE (KEY FIX)

using (var scope = app.Services.CreateScope())
{
    var jwtSettingService = scope.ServiceProvider.GetRequiredService<JwtSettingService>();
    var holder = scope.ServiceProvider.GetRequiredService<JwtRuntimeConfigHolder>();

    var jwt = await jwtSettingService.GetOrCreateJwtConfigAsync();

    holder.SecretKeyBase64 = jwt.SecretKey;
    holder.Issuer = jwt.Issuer;
    holder.Audience = jwt.Audience;

    var monitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
    var bearerOptions = monitor.Get(JwtBearerDefaults.AuthenticationScheme);
    bearerOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = holder.Issuer,
        ValidAudience = holder.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
        Convert.FromBase64String(holder.SecretKeyBase64)
    ),

        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role,

        ClockSkew = TimeSpan.Zero
    };
}

#endregion
#region 11. Static Well-Known

var wellKnownPath = Path.Combine(Directory.GetCurrentDirectory(), ".well-known");
if (Directory.Exists(wellKnownPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(wellKnownPath),
        RequestPath = "/.well-known"
    });
}

#endregion

#region 12. Migrations & Seeding

bool runSeeders = configuration.GetValue<bool>("EF:RunSeeders");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    var log = services.GetRequiredService<ILogger<Program>>();

    try
    {
        if (db.Database.CanConnect())
        {
            log.LogInformation("Database reachable.");
            await EnsureRolesAsync(services, log, new[]
{
            "User",
            "Admin",
            "Administrator",
            "Employee",
            "Business"
             });

            if (runSeeders)
            {
                await SeedLoginAgreementData.InitializeAsync(db);
                await services.GetRequiredService<SeedDataService>().InitializeData();
                log.LogInformation("Seed data initialization complete.");
            }
        }
        else
        {
            log.LogWarning("Database NOT reachable");
        }
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Error during DB init");
        if (app.Environment.IsDevelopment())
            throw;
    }
}

#endregion

#region 13. Middleware

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseRouting();

app.UseCors("ApiCors");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
});

#endregion

#region 14. Endpoints

app.MapGet("/health", () => Results.Ok("OK"));
app.MapGet("/api/health", () => Results.Ok("OK"));
app.MapGet("/", () => Results.Ok(new { status = "API alive", timeUtc = DateTime.UtcNow }));

#endregion

#region 15. WebSocket

// Existing WS (kept as-is)
app.UseWebSockets();

app.Map("/ws/iot", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    var token = context.Request.Query["access_token"];

    // 👉 You can add JWT verification here later

    var trackingNumber =
    context.Request.Query["tracking"].ToString()
    ?? context.Request.Query["trackingNumber"].ToString();

    if (string.IsNullOrWhiteSpace(trackingNumber))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    var socket = await context.WebSockets.AcceptWebSocketAsync();

    var manager = context.RequestServices.GetRequiredService<IoTWebSocketManager>();

    await manager.HandleConnectionAsync(
        socket,
        trackingNumber,
        context.RequestAborted
    );
});

#endregion

#region 16. Controllers

app.MapControllers();
app.Run();

#endregion

static async Task EnsureRolesAsync(
    IServiceProvider services,
    ILogger logger,
    IEnumerable<string> roles)
{
    using var scope = services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

    foreach (var name in roles.Distinct(StringComparer.OrdinalIgnoreCase))
    {
        if (await roleManager.FindByNameAsync(name) != null)
            continue;

        var create = await roleManager.CreateAsync(new Role
        {
            Name = name,
            NormalizedName = name.ToUpper(),
            Description = $"{name} role"
        });

        if (create.Succeeded)
        {
            logger.LogInformation("Role '{Role}' created.", name);
        }
        else
        {
            logger.LogWarning(
                "Failed to create role '{Role}': {Errors}",
                name,
                string.Join(", ", create.Errors.Select(e => e.Description))
            );
        }
    }
}
