using AutoHub.Data;
using AutoHub.Models;
using AutoHub.Models.Identity;
using AutoHub.Models.News;
using AutoHub.Services;
using AutoHub.Services.Admin;
using AutoHub.Services.Auth;
using AutoHub.Services.Chat;
using AutoHub.Services.News;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Load .env (KEY=VALUE lines) into environment variables before config binding.
// Existing process/host environment variables always win over .env values.
foreach (var line in File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), ".env")))
{
    var trimmed = line.Trim();
    if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;

    var eq = trimmed.IndexOf('=');
    if (eq <= 0) continue;

    var key = trimmed[..eq].Trim();
    var value = trimmed[(eq + 1)..].Trim().Trim('"', '\'');
    if (key.Length > 0 && Environment.GetEnvironmentVariable(key) is null)
        Environment.SetEnvironmentVariable(key, value);
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "AutoHub.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});
builder.Services.AddDbContext<AutoHubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AutoHub")));
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<INavigationService, NavigationService>();

var newsConfig = new NewsConfig
{
    Provider = builder.Configuration["NEWS_PROVIDER"] ?? "newsapi",
    NewsApiKey = builder.Configuration["NEWS_API_KEY"],
    GNewsApiKey = builder.Configuration["GNEWS_API_KEY"],
    CacheTtlSeconds = int.TryParse(builder.Configuration["NEWS_CACHE_TTL_SECONDS"], out var ttl) ? ttl : 1200,
    FetchTimeoutMs = int.TryParse(builder.Configuration["NEWS_FETCH_TIMEOUT_MS"], out var timeout) ? timeout : 8000,
    MaxResults = int.TryParse(builder.Configuration["NEWS_MAX_RESULTS"], out var max) ? max : 6,
    PublicLimit = int.TryParse(builder.Configuration["NEWS_PUBLIC_LIMIT"], out var limit) ? limit : 3,
    AllowedDomains = builder.Configuration["NEWS_ALLOWED_DOMAINS"] ?? "",
    RssFeeds = builder.Configuration["NEWS_RSS_FEEDS"] ?? "",
    AdminRefreshSecret = builder.Configuration["NEWS_ADMIN_REFRESH_SECRET"],
};
builder.Services.AddSingleton(newsConfig);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<INewsCacheService, NewsCacheService>();

builder.Services.AddSingleton<INewsProvider>(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    var config = sp.GetRequiredService<NewsConfig>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

    return newsConfig.Provider.ToLowerInvariant() switch
    {
        "gnews" => new GNewsProvider(httpClient, config, loggerFactory.CreateLogger<GNewsProvider>()),
        "rss" => new RssProvider(httpClient, config, loggerFactory.CreateLogger<RssProvider>()),
        "hybrid_rss" => new HybridRssProvider(httpClient, config, loggerFactory.CreateLogger<HybridRssProvider>()),
        _ => new NewsApiProvider(httpClient, config, loggerFactory.CreateLogger<NewsApiProvider>()),
    };
});
builder.Services.AddSingleton<INewsService, NewsService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password policy: requires length >= 6, a digit, a lowercase letter.
        // (Deliberately does not force uppercase / special chars so the
        //  bootstrapped SuperAdmin credential supplied by the owner works.
        //  Raise these to a stricter default if you prefer.)
        options.Password.RequiredLength = 6;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = false;

        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<AutoHubDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = "AutoHub.Auth";
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/403";
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.VehicleWrite, p => p.RequireRole(RoleNames.VehicleWriters));
    options.AddPolicy(Policies.VehicleDelete, p => p.RequireRole(RoleNames.VehicleWriters));
    options.AddPolicy(Policies.UserManagement, p => p.RequireRole(RoleNames.UserManagers));
    options.AddPolicy(Policies.AppointmentManagement, p => p.RequireRole(RoleNames.AppointmentManagers));
    options.AddPolicy(Policies.ContentManagement, p => p.RequireRole(RoleNames.ContentManagers));
    options.AddPolicy(Policies.SecurityAdministration, p => p.RequireRole(RoleNames.RoleManagers));
    options.AddPolicy(Policies.SystemConfiguration, p => p.RequireRole(RoleNames.SystemConfigurators));
    options.AddPolicy(Policies.AuditView, p => p.RequireRole(RoleNames.AuditViewers));
    options.AddPolicy(Policies.AdminArea, p => p.RequireRole(
        RoleNames.Sales, RoleNames.ContentEditor, RoleNames.Manager, RoleNames.Administrator, RoleNames.SuperAdmin));
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IConsultationService, ConsultationService>();
builder.Services.AddScoped<ITestDriveService, TestDriveService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IChatAdvisorService, ChatAdvisorService>();

var app = builder.Build();

bool dbReady = false;
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AutoHubDbContext>();
        await db.Database.MigrateAsync();
        await AutoHubSeeder.SeedAsync(db);
        await IdentitySeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
        dbReady = true;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[DB] Khong ket noi duoc SQL Server: {ex.Message}");
    Console.WriteLine("[DB] App se chay o che do khong co database.");
}
Console.WriteLine($"[DB] Trang thai ket noi: {(dbReady ? "San sang" : "Khong kha dung")}");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/vehicles", (IVehicleService service, string? brand, int? page, int? pageSize, string? q) =>
{
    if (page.HasValue || pageSize.HasValue || !string.IsNullOrWhiteSpace(q))
    {
        return Results.Ok(service.GetVehiclesPaged(
            page ?? 1,
            pageSize ?? 8,
            brand,
            q));
    }
    return Results.Ok(service.GetVehicles(brand));
});

app.MapGet("/api/vehicles/search", (IVehicleService service, string? q) =>
{
    if (string.IsNullOrWhiteSpace(q))
    {
        return Results.Ok(service.GetVehicles());
    }
    return Results.Ok(service.Search(q));
});

app.MapGet("/api/vehicles/{slug}", (IVehicleService service, string slug) =>
{
    var vehicle = service.GetVehicleBySlug(slug);
    return vehicle is null ? Results.NotFound() : Results.Ok(vehicle);
});

app.MapPost("/api/chat/message", async (IChatAdvisorService advisor, ChatAdvisorRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request?.Message))
    {
        return Results.BadRequest(new { error = "Message cannot be empty." });
    }
    var response = await advisor.ProcessMessageAsync(request.Message, request.CurrentSlug);
    return Results.Ok(response);
});

app.MapGet("/api/automotive-news", async (
    INewsService newsService,
    NewsConfig config,
    int? limit,
    string? lang,
    string? category,
    bool? forceRefresh,
    HttpContext http) =>
{
    var effectiveLimit = Math.Clamp(limit ?? config.PublicLimit, 1, 6);
    var effectiveCategory = category ?? "all";
    var shouldRefresh = forceRefresh == true &&
                        (http.Request.Host.Host.Contains("localhost") ||
                         http.Request.Headers.TryGetValue("X-Admin-Secret", out var secret) &&
                         string.Equals(secret.ToString(), config.AdminRefreshSecret, StringComparison.Ordinal));

    try
    {
        var (items, meta) = await newsService.GetNewsAsync(
            effectiveLimit, effectiveCategory, shouldRefresh, http.RequestAborted);

        return Results.Ok(new
        {
            data = items,
            meta = meta,
        });
    }
    catch (Exception)
    {
        return Results.Json(new
        {
            error = new
            {
                code = "NEWS_UNAVAILABLE",
                message = "News feed is temporarily unavailable.",
            }
        }, statusCode: 503);
    }
});

app.MapGet("/api/automotive-news/health", async (INewsService newsService) =>
{
    var health = await newsService.GetHealthAsync();
    return Results.Ok(health);
});

var customerApi = app.MapGroup("/api/me").RequireAuthorization();
customerApi.MapGet("/", async (ICurrentUser me, IUserService users) =>
{
    var uid = me.UserId!;
    var profile = await users.GetProfileAsync(uid);
    return profile is null ? Results.NotFound() : Results.Ok(profile);
});

customerApi.MapGet("/favorites", async (ICurrentUser me, IFavoriteService favs, IVehicleService vehicles) =>
{
    var ids = await favs.GetFavoriteIdsAsync(me.UserId!);
    var all = vehicles.GetVehicles();
    return Results.Ok(all.Where(v => ids.Contains(v.Id)));
});

customerApi.MapPost("/favorites/{vehicleId:int}", async (ICurrentUser me, IFavoriteService favs, int vehicleId, IAuditService audit) =>
{
    var ok = await favs.AddAsync(me.UserId!, vehicleId);
    return ok ? Results.Ok() : Results.BadRequest(new { error = "Không thể thêm xe yêu thích." });
});

customerApi.MapDelete("/favorites/{vehicleId:int}", async (ICurrentUser me, IFavoriteService favs, int vehicleId, IAuditService audit) =>
{
    var ok = await favs.RemoveAsync(me.UserId!, vehicleId);
    return ok ? Results.Ok() : Results.NotFound();
});

customerApi.MapGet("/consultations", async (ICurrentUser me, IConsultationService service) =>
{
    var rows = await service.GetMineAsync(me.UserId!);
    return Results.Ok(rows.Select(c => new
    {
        c.Id, c.FullName, c.Phone, c.Email, c.PreferredDate, c.PreferredTime,
        c.ConsultationMethod, c.Budget, c.Message, c.Status, c.CreatedAt
    }));
});

customerApi.MapGet("/test-drives", async (ICurrentUser me, ITestDriveService service) =>
{
    var rows = await service.GetMineAsync(me.UserId!);
    return Results.Ok(rows.Select(t => new
    {
        t.BookingId, t.FullName, t.Phone, t.Email, t.VariantId, t.PreferredDate,
        t.TimeSlot, t.Status, t.CreatedAt, VehicleName = t.Variant?.VariantName
    }));
});

customerApi.MapGet("/notifications", async (ICurrentUser me, AutoHub.Data.AutoHubDbContext db) =>
{
    var items = await db.UserNotifications.AsNoTracking()
        .Where(n => n.UserId == me.UserId)
        .OrderByDescending(n => n.CreatedAt)
        .Take(20)
        .ToListAsync();
    return Results.Ok(items);
});

var adminApi = app.MapGroup("/api/admin").RequireAuthorization(Policies.AdminArea);
adminApi.MapGet("/dashboard", async (IAdminDashboardService dash) =>
    Results.Ok(await dash.GetSummaryAsync()));

adminApi.MapGet("/users", async (IUserService users, string? search, string? role, string? status, int page = 1, int pageSize = 20) =>
{
    var result = await users.GetUsersAsync(page, pageSize, search, role, status);
    return Results.Ok(result);
}).RequireAuthorization(Policies.UserManagement);

adminApi.MapGet("/consultations", async (IConsultationService service, string? status, string? search, int? vehicleId, int page = 1, int pageSize = 20) =>
{
    var result = await service.GetPagedAsync(page, pageSize, status, search, vehicleId, null);
    return Results.Ok(result);
}).RequireAuthorization(Policies.AppointmentManagement);

adminApi.MapGet("/test-drives", async (ITestDriveService service, string? status, string? search, int? showroomId, int page = 1, int pageSize = 20) =>
{
    var result = await service.GetPagedAsync(page, pageSize, status, search, showroomId);
    return Results.Ok(result);
}).RequireAuthorization(Policies.AppointmentManagement);

adminApi.MapGet("/audit-logs", async (AutoHub.Data.AutoHubDbContext db, string? user, string? action, string? entityType, int page = 1, int pageSize = 20) =>
{
    var query = db.AuditLogs.AsNoTracking().AsQueryable();
    if (!string.IsNullOrWhiteSpace(user)) query = query.Where(a => a.UserName != null && a.UserName.Contains(user));
    if (!string.IsNullOrWhiteSpace(action)) query = query.Where(a => a.Action == action);
    if (!string.IsNullOrWhiteSpace(entityType)) query = query.Where(a => a.EntityType == entityType);
    var totalItems = await query.CountAsync();
    var items = await query.OrderByDescending(a => a.Timestamp)
        .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    return Results.Ok(new { items, totalItems, page, pageSize });
}).RequireAuthorization(Policies.AuditView);

adminApi.MapGet("/news", async (IContentService service, string? status, string? kind, string? search, int page = 1, int pageSize = 20) =>
{
    var result = await service.GetArticlesAsync(page, pageSize, status, kind, search);
    return Results.Ok(result);
}).RequireAuthorization(Policies.ContentManagement);

app.MapRazorPages();

app.Run();
