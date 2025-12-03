using KahootMvc.AppContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var connectionString =
    builder.Configuration.GetConnectionString("SqlCon");//app settingsden connection stringi çektim

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly()); // Pakette hata aldým projeyi yaparken versiyonu kontrol edip düzelttim önceki versiyon 15.0 yeni => 13.0.1 sýkýntý çözüldü

builder.Services.AddDbContext<AppDbContext>(options =>options.UseSqlServer(connectionString));//sql server kullanarak baðlantýyý saðladým 
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    
    app.UseHsts();
}
// Program.cs veya Startup.cs'den hatýrlatma
// Program.cs dosyasýnda 'var app = builder.Build();' satýrýndan sonra

app.MapControllerRoute(
    name: "areas",
    // Alan (Area) adýnýn zorunlu olarak URL'de bulunmasýný saðlar
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

// Diðer yönlendirmeler (Varsayýlan yönlendirme)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapGet("/", () => Results.File("index.html", "text/html"));

app.UseRouting();

app.UseAuthorization();


app.Run();
