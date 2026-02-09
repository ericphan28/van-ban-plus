using AIVanBan.API.Middleware;
using AIVanBan.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Hỗ trợ biến môi trường (cloud hosting: Render, Fly.io, Railway...)
// Ưu tiên: Environment Variable > appsettings.json
builder.Configuration.AddEnvironmentVariables();

// ============================================================
// Services
// ============================================================

// Database (Singleton — LiteDB thread-safe)
builder.Services.AddSingleton<DatabaseService>();

// Business services
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<UsageService>();
builder.Services.AddSingleton<GeminiProxyService>();

// Controllers
builder.Services.AddControllers();

// Swagger (API docs)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "VanBanPlus API",
        Version = "v1",
        Description = "API Gateway cho VanBanPlus — Quản lý văn bản thông minh với AI.\n\n" +
                      "🔑 Xác thực: Thêm header `X-API-Key` với API key nhận được khi đăng ký.\n\n" +
                      "📊 Quota: Mỗi gói có giới hạn request/tháng và token/tháng khác nhau.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "VanBanPlus Support",
            Email = "ericphan28@gmail.com"
        }
    });

    // Add API Key authentication to Swagger
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "X-API-Key",
        Description = "API Key từ khi đăng ký tài khoản"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS — cho phép desktop app gọi API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()   // Desktop app gọi từ mọi nơi
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ============================================================
// App
// ============================================================

var app = builder.Build();

// Swagger UI (chỉ bật ở Development, hoặc bật luôn nếu muốn)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "VanBanPlus API v1");
    c.RoutePrefix = "swagger";
});

// CORS
app.UseCors();

// Middleware pipeline
app.UseMiddleware<ApiKeyAuthMiddleware>();  // Xác thực API key
app.UseMiddleware<QuotaCheckMiddleware>();  // Kiểm tra quota

app.MapControllers();

// Health check
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "VanBanPlus API",
    version = "1.0.0",
    time = DateTime.UtcNow
}));

// Tạo admin mặc định
using (var scope = app.Services.CreateScope())
{
    var userService = scope.ServiceProvider.GetRequiredService<UserService>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var adminEmail = config.GetValue<string>("Admin:Email") ?? "admin@vanbanplus.com";
    var adminPassword = config.GetValue<string>("Admin:Password") ?? "Admin@123456";
    userService.EnsureAdminExists(adminEmail, adminPassword);
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "5100";
Console.WriteLine("==========================================================");
Console.WriteLine("   VanBanPlus API — AI Gateway for Document Management");
Console.WriteLine("==========================================================");
Console.WriteLine($"   Swagger UI: /swagger");
Console.WriteLine($"   Health:     /health");
Console.WriteLine($"   Port:       {port}");
Console.WriteLine("==========================================================");

app.Run();
