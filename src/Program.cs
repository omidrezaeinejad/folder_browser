var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMvc(options =>
{
    options.EnableEndpointRouting = false;
});
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{

});

app.UseRouting();
app.UseMvc();

app.Configuration.GetSection("app").Bind(AppConfigSection.Current);
AppConfigSection.Current.RootPath = Path.GetFullPath(AppConfigSection.Current?.RootPath ?? ".");

app.Run();
