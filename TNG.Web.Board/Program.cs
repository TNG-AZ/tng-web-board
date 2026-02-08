using Blazored.Modal;
using Ixnas.AltchaNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;
using TNG.Web.Board.Data;
using TNG.Web.Board.Services;
using TNG.Web.Board.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString),
    ServiceLifetime.Transient,
    ServiceLifetime.Transient);

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityCore<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddDefaultTokenProviders()
    .AddRoles<IdentityRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.RequireUniqueEmail = false;

    //Signin
    options.SignIn.RequireConfirmedAccount = true;
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("IsBoardMember", policy =>
        policy.RequireRole("Boardmember", "Administrator"));
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Users", "IsBoardMember");
});

builder.Services.AddServerSideBlazor();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<GoogleServices>();
builder.Services.AddSingleton<SquareService>();
builder.Services.AddScoped<AuthUtilities>();
builder.Services.AddSingleton<IEmailSender, GoogleServices>();
builder.Services.AddSingleton<LinksService>();
builder.Services.AddSingleton<AltchaPageService>();
builder.Services.AddSingleton<IAltchaChallengeStore, AltchaCache>();
builder.Services.AddBlazoredModal();

builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.UseAuthentication();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "default-src * data: mediastream: blob: filesystem: about: ws: wss: 'unsafe-eval' 'wasm-unsafe-eval' 'unsafe-inline'; script-src * data: blob: 'unsafe-inline' 'unsafe-eval'; script-src-elem * data: blob: 'unsafe-inline' 'unsafe-eval';connect-src * data: blob: 'unsafe-inline'; img-src * data: blob: 'unsafe-inline'; media-src * data: blob: 'unsafe-inline'; frame-src * data: blob: ; style-src * data: blob: 'unsafe-inline';font-src * data: blob: 'unsafe-inline';frame-ancestors * data: blob:;");
    await next();
});

//Square webhooks
app.MapPost("/api/square/invoicepaid", SquareService.HandleInvoicePaid);

//Discord APIs
app.MapGet("/api/discord/aged/", DiscordAPIService.GetAgedOutMembers);
app.MapGet("/api/discord/lapsed/", DiscordAPIService.GetLapsedMembers);
app.MapGet("/api/discord/current/", DiscordAPIService.GetCurrentMembers);
app.MapGet("/api/discord/attended/", DiscordAPIService.GetAttendedMembers);
app.MapGet("/api/discord/byid/{discordId:long}", DiscordAPIService.GetMemberByDiscordId);
app.MapGet("/api/discord/byid/info/{discordId:long}", DiscordAPIService.GetMemberInfoByDiscordId);
app.MapGet("/api/discord/members", DiscordAPIService.GetMemberInfoForAllMembers);

SecretCodeService.SetCode(builder.Configuration["TNGRegistrationCode"]);
await RolesData.SeedRoles(app.Services);

app.Run();
