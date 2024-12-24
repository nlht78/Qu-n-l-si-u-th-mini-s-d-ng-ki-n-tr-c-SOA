using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Thêm Razor Pages
builder.Services.AddRazorPages();

// Thêm cấu hình Session
builder.Services.AddDistributedMemoryCache(); // Dùng bộ nhớ trong để lưu session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session
    options.Cookie.HttpOnly = true; // Chỉ cho phép truy cập session qua HTTP
    options.Cookie.IsEssential = true; // Bắt buộc bật cookie
});

// Cấu hình HttpClient
builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri("https://localhost:44360");
});

builder.Services.AddHttpClient("ProductService", client =>
{
    client.BaseAddress = new Uri("https://localhost:44361");
});

builder.Services.AddHttpClient("OrderService", client =>
{
    client.BaseAddress = new Uri("https://localhost:44362");
});


builder.Services.AddHttpClient("ReportService", client =>
{
    client.BaseAddress = new Uri("https://localhost:44363");
});

// Cấu hình JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

var app = builder.Build();

// Thêm Session Middleware
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
