using LinguaMeet.Web.Services;

var b = WebApplication.CreateBuilder(args);
b.Services.AddControllersWithViews();
b.Services.AddHttpContextAccessor();
b.Services.AddDistributedMemoryCache();
b.Services.AddSession(o =>
{
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    o.IdleTimeout = TimeSpan.FromHours(8);
});
b.Services.AddHttpClient(
    "Api",
    c => c.BaseAddress = new Uri(b.Configuration["ApiSettings:BaseUrl"]!)
);
b.Services.AddScoped<ApiClientService>();
var app = b.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
