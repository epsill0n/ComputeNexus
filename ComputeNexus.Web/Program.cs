using System.Text;
using System.Threading.Channels;
using ComputeNexus.Domain.Configuration;
using ComputeNexus.Domain.Dtos.Email;
using ComputeNexus.Domain.Entities;
using ComputeNexus.Repository;
using ComputeNexus.Repository.Implementation;
using ComputeNexus.Repository.Interface;
using ComputeNexus.Service.Implementation;
using ComputeNexus.Service.Interface;
using ComputeNexus.Service.Jobs;
using ComputeNexus.Web.Interceptor;
using ComputeNexus.Web.Mapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlite(connectionString);
    options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// identity configuration
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// jwt auth
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] 
    ?? throw new InvalidOperationException("Jwt:Key is missing from configuration."));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

// http context accessor
builder.Services.AddHttpContextAccessor();

// repository layer
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IHardwareNodeRepository, HardwareNodeRepository>();

// service layer
builder.Services.AddScoped<IComputeInstanceService, ComputeInstanceService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IHardwareNodeService, HardwareNodeService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IEmailRetryPolicy, ExponentialBackoffEmailRetryPolicy>();

// interceptors, helpers
builder.Services.AddScoped<AuditInterceptor>();

// mappers
builder.Services.AddScoped<ComputeInstanceMapper>();
builder.Services.AddScoped<HardwareNodeMapper>();
builder.Services.AddScoped<JobMapper>();

// mvc & api controllers
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


// email integration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddSingleton<IEmailQueue, ChannelEmailQueue>();
builder.Services.AddSingleton(Channel.CreateUnbounded<EmailMessage>());
builder.Services.AddHostedService<EmailBackgroundService>();

// external api client
builder.Services.AddMemoryCache();
builder.Services.Configure<ProviderApiSettings>(builder.Configuration.GetSection("ProviderApi"));

builder.Services.AddHttpClient<IProviderApiClient, ProviderApiClient>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ProviderApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseAddress);
    client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
    // Add Bearer token for Vast.ai API
    client.DefaultRequestHeaders.Authorization = 
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", settings.ApiKey);
});

// etl service
builder.Services.AddScoped<IHardwareSyncService, HardwareSyncService>();
builder.Services.AddHostedService<HardwareSyncBackgroundService>();

// quartz scheduler
builder.Services.AddQuartz(options =>
{
    var jobKey = new JobKey("instance-cleanup", "maintenance");
    options.AddJob<InstanceCleanupJob>(o => o.WithIdentity(jobKey));
    options.AddTrigger(o =>
    {
        o.ForJob(jobKey)
            .WithIdentity("instance-cleanup-trigger")
            .WithCronSchedule("0 0 2 * * ?") // run daily at 2 AM
            .WithDescription("Clean up old instances");
    });
});
builder.Services.AddQuartzHostedService();

// excel services
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<IExcelImportService, ExcelImportService>();


// build app
var app = builder.Build();

// pipeline config
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();