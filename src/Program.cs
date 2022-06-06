var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMvc(options =>
{
    options.EnableEndpointRouting = false;
});
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

var app = builder.Build();

app.UseRouting();
app.UseMvc();

app.Configuration.GetSection("app").Bind(AppConfigSection.Current);
AppConfigSection.Current.RootPath = Path.GetFullPath(AppConfigSection.Current?.RootPath ?? ".");

app.Run();
