using Aephy.WEB.Controllers;
using Aephy.WEB.DashboardHubs;
using Aephy.WEB.Provider;
using Aephy.WEB.Repository;
//using Aephy.WEB.SubscribeTableDependencies;
using Azure.Identity;
using Azure.Storage.Blobs;
//using ProductsUI.MiddlewareExtensions;
//using ProductsUI.SubscribeTableDependencies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddSingleton<DashboardHub>();
//builder.Services.AddSingleton<SubscribeProductTableDependencies>();
//builder.Services.AddSingleton<SubscribeApprovedListTableDependency>();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(1800);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IApiRepository, ApiRepository>();
// Use DefaultAzureCredential to authenticate with Azure Blob Storage
var configuration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json")
    .Build();
var connectionString = configuration.GetConnectionString("AzureBlobStorage");
var localConnection = configuration.GetConnectionString("ConnStr");

var credential = new DefaultAzureCredential();

var blobServiceClient = new BlobServiceClient(connectionString);

builder.Services.AddSingleton<BlobServiceClient>(blobServiceClient);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();
//app.MapHub<DashboardHub>("/dashboardHub");
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LandingPage}/{action=Index}/{id?}");
/*app.MapRazorPages();*/

//app.UseSqlTableDependency<SubscribeProductTableDependencies>(localConnection);
//app.UseSqlTableDependency<SubscribeApprovedListTableDependency>(localConnection);

app.Run();