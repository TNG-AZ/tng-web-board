using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TNG.Web.Board.Data;
using TNG.Web.Board.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration["Database"]),
    ServiceLifetime.Transient,
    ServiceLifetime.Transient);

builder.Services.AddSingleton<GoogleServices>();

builder.Build().Run();
