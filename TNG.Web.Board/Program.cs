using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TNG.Web.Board.Areas.Identity;
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
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsBoardMember", policy =>
        policy.RequireRole("Boardmember", "Administrator"));
});
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Users", "IsBoardMember");
});

builder.Services.AddServerSideBlazor();

builder.Services.AddHttpClient();

builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
builder.Services.AddSingleton<GoogleServices>();
builder.Services.AddSingleton<SquareService>();
builder.Services.AddScoped<AuthUtilities>();
builder.Services.AddSingleton<IEmailSender, GoogleServices>();
builder.Services.AddSingleton<LinksService>();
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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

//Square webhooks
app.MapPost("/api/square/invoicepaid", SquareService.HandleInvoicePaid);

//Discord APIs
app.MapGet("/api/discord/aged/", DiscordAPIService.GetAgedOutMembers);
app.MapGet("/api/discord/lapsed/", DiscordAPIService.GetLapsedMembers);
app.MapGet("/api/discord/current/", DiscordAPIService.GetCurrentMembers);
app.MapGet("/api/discord/attended/", DiscordAPIService.GetAttendedMembers);

SecretCodeService.SetCode(builder.Configuration["TNGRegistrationCode"]);
await RolesData.SeedRoles(app.Services);

app.Run();
