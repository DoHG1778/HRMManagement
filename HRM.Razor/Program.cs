using HRM.Razor.Authentication;
using HRM.Razor.Services.ApiClients;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Razor Pages & HttpContextAccessor
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
});
builder.Services.AddHttpContextAccessor();

// 2. Add Session State for storing server-side JWT
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".HRM.Razor.Session";
});

// 3. Add Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".HRM.Razor.Auth";
        options.Cookie.HttpOnly = true;
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// 4. Add Authorization
builder.Services.AddAuthorization();

// 5. Add DelegatingHandler & Typed ApiClients
builder.Services.AddTransient<JwtAuthorizationHandler>();

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7100/";

builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddScoped<IAuthApiClient, AuthApiClient>();
builder.Services.AddScoped<IEmployeeApiClient, EmployeeApiClient>();
builder.Services.AddScoped<IDepartmentApiClient, DepartmentApiClient>();
builder.Services.AddScoped<IPositionApiClient, PositionApiClient>();
builder.Services.AddScoped<IEmployeeAssignmentApiClient, EmployeeAssignmentApiClient>();
builder.Services.AddScoped<IPayrollApiClient, PayrollApiClient>();
builder.Services.AddScoped<IAttendanceAdjustmentApiClient, AttendanceAdjustmentApiClient>();
builder.Services.AddScoped<ILeaveApiClient, LeaveApiClient>();
builder.Services.AddScoped<IOvertimeApiClient, OvertimeApiClient>();
builder.Services.AddScoped<INotificationApiClient, NotificationApiClient>();
builder.Services.AddScoped<IUserApiClient, UserApiClient>();
builder.Services.AddScoped<IKpiApiClient, KpiApiClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
