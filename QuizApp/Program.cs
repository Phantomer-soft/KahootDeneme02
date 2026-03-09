using KahootMvc.AppContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using KahootMvc;
using KahootMvc.Hubs;

var builder = WebApplication.CreateBuilder(args);
var connectionString =
    builder.Configuration.GetConnectionString("SqlCon");//app settingsden connection stringi �ektim

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly()); // Pakette hata ald�m projeyi yaparken versiyonu kontrol edip d�zelttim �nceki versiyon 15.0 yeni => 13.0.1 s�k�nt� ��z�ld�
builder.Services.AddDbContext<AppDbContext>(options =>options.UseMySql(connectionString,ServerVersion.AutoDetect(connectionString)));//sql server kullanarak ba�lant�y� sa�lad�m 
builder.Services.AddScoped<Functions>();
builder.Services.AddScoped<QuizTimerService>();
builder.Services.AddSignalR();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.SetIsOriginAllowed(origin => true) // Tüm originlere izin ver
        .AllowAnyMethod()                  // GET, POST vb. hepsine izin ver
        .AllowAnyHeader()                  // Tüm başlıklara izin ver
        .AllowCredentials()));      
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    
    app.UseHsts();
}
// Program.cs veya Startup.cs'den hat�rlatma
// Program.cs dosyas�nda 'var app = builder.Build();' sat�r�ndan sonra

app.MapControllerRoute(
    name: "areas",
    // Alan (Area) ad�n�n zorunlu olarak URL'de bulunmas�n� sa�lar
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

// Di�er y�nlendirmeler (Varsay�lan y�nlendirme)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
app.MapHub<QuizHub>("/QuizHub");
app.UseCors();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapGet("/", () => Results.File("index.html", "text/html"));

app.UseRouting();

app.UseAuthorization();


app.Run();
