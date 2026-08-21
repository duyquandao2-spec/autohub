using AutoHub.Data;
using AutoHub.Models.News;
using AutoHub.Services;
using AutoHub.Services.News;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<AutoHubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AutoHub")));
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IMediaStorage, LocalMediaStorage>();

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
        _ => new NewsApiProvider(httpClient, config, loggerFactory.CreateLogger<NewsApiProvider>()),
    };
});
builder.Services.AddSingleton<INewsService, NewsService>();

var app = builder.Build();

bool dbReady = false;
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AutoHubDbContext>();
        await db.Database.MigrateAsync();
        await AutoHubSeeder.SeedAsync(db);
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

app.MapGet("/api/brands", (IVehicleService service) =>
{
    return Results.Ok(service.GetBrands());
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

app.MapRazorPages();
app.Run();
